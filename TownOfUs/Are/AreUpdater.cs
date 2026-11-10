using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using Version = SemanticVersioning.Version;

namespace TownOfUs.Are;

public enum AreUpdateState
{
    Idle,
    Disabled,
    Checking,
    UpToDate,
    Downloading,
    Updated,
    Failed
}

/// <summary>
///     Auto-updater for the AmonRa Edition. Checks the latest GitHub release of the fork on startup, downloads the
///     updatable assets in the background, verifies them and swaps them in place. The new files take effect after a
///     game restart (the old ones are renamed to *.old and removed on the next launch).
/// </summary>
public static class AreUpdater
{
    private const string OldSuffix = ".old";
    private const string DownloadSuffix = ".download";

    private static volatile AreUpdateState _state = AreUpdateState.Idle;
    private static long _bytesDone;
    private static long _bytesTotal;

    public static ConfigEntry<bool> AutoUpdate { get; private set; } = null!;

    public static AreUpdateState State => _state;

    /// <summary>
    ///     Gets the version of the newer release, if one was found.
    /// </summary>
    public static string? RemoteVersion { get; private set; }

    public static string? LastError { get; private set; }

    /// <summary>
    ///     Gets whether a newer release was found (the popup is only shown in that case).
    /// </summary>
    public static bool UpdateFound => RemoteVersion != null;

    public static float Progress
    {
        get
        {
            var total = Interlocked.Read(ref _bytesTotal);
            return total > 0 ? Math.Clamp((float)Interlocked.Read(ref _bytesDone) / total, 0f, 1f) : 0f;
        }
    }

    public static void Initialize(ConfigFile config)
    {
        AutoUpdate = config.Bind("Updater", "AutoUpdate", true,
            "If enabled, the AmonRa Edition checks the fork's GitHub releases on startup and downloads new versions automatically. Developer builds (version containing 'dev') never update.");

        // Run after every plugin is loaded, so the locations of all updatable assemblies are known.
        IL2CPPChainloader.Instance.Finished += Start;
    }

    private static void Start()
    {
        CleanupLeftovers();

        if (TownOfUsPlugin.IsMobile || TownOfUsPlugin.IsDevBuild)
        {
            Message($"ARE Updater: disabled for this build ({TownOfUsPlugin.Version}).");
            _state = AreUpdateState.Disabled;
            return;
        }

        if (!AutoUpdate.Value)
        {
            Message("ARE Updater: disabled in config.");
            _state = AreUpdateState.Disabled;
            return;
        }

        _state = AreUpdateState.Checking;
        Task.Run(RunAsync);
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "The updater must never crash the game; every failure is logged and reported in the popup.")]
    private static async Task RunAsync()
    {
        List<UpdateItem> plan = [];
        try
        {
            // UseProxy = false: system proxy detection P/Invokes winhttp.dll, which is Doorstop's proxy dll in many
            // BepInEx installs and crashed the game with an AccessViolationException on some Windows builds.
            using var handler = new HttpClientHandler { UseProxy = false };
            using var client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromMinutes(15);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("TOU-Mira-ARE-Updater");

            var release = await FetchLatestReleaseAsync(client).ConfigureAwait(false);
            if (release == null)
            {
                Message("ARE Updater: no published release found.");
                _state = AreUpdateState.UpToDate;
                return;
            }

            var current = new Version(TownOfUsPlugin.Version, loose: true);
            var remote = new Version(release.Tag.TrimStart('v', 'V'), loose: true);
            if (remote <= current)
            {
                Message($"ARE Updater: up to date ({current}, latest release {remote}).");
                _state = AreUpdateState.UpToDate;
                return;
            }

            plan = BuildPlan(release);
            if (plan.Count == 0)
            {
                Message("ARE Updater: update skipped (developer setup or nothing to update).");
                _state = AreUpdateState.UpToDate;
                return;
            }

            Message($"ARE Updater: updating {current} -> {remote} ({string.Join(", ", plan.Select(x => x.Asset.Name))}).");
            RemoteVersion = remote.ToString();
            Interlocked.Exchange(ref _bytesTotal, plan.Sum(x => x.Asset.Size));
            _state = AreUpdateState.Downloading;

            foreach (var item in plan)
            {
                await DownloadAndVerifyAsync(client, item).ConfigureAwait(false);
            }

            SwapAll(plan);

            Message($"ARE Updater: updated to {remote}. Restart required.");
            _state = AreUpdateState.Updated;
        }
        catch (Exception e)
        {
            Error($"ARE Updater: failed - {e}");
            LastError = e.Message;
            foreach (var item in plan)
            {
                TryDelete(item.TargetPath + DownloadSuffix);
            }

            _state = AreUpdateState.Failed;
        }
    }

    private static async Task<ReleaseInfo?> FetchLatestReleaseAsync(HttpClient client)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, AreInfo.LatestReleaseApiUrl);
        request.Headers.Accept.ParseAdd("application/vnd.github+json");
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        using var response = await client.SendAsync(request, cts.Token).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cts.Token).ConfigureAwait(false);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var tag = root.GetProperty("tag_name").GetString() ?? throw new InvalidDataException("Release has no tag.");

        var assets = new List<AssetInfo>();
        foreach (var asset in root.GetProperty("assets").EnumerateArray())
        {
            var digest = asset.TryGetProperty("digest", out var d) && d.ValueKind == JsonValueKind.String
                ? d.GetString()
                : null;
            assets.Add(new AssetInfo(
                asset.GetProperty("name").GetString() ?? string.Empty,
                asset.GetProperty("browser_download_url").GetString() ?? string.Empty,
                asset.GetProperty("size").GetInt64(),
                digest));
        }

        return new ReleaseInfo(tag, assets);
    }

    private static List<UpdateItem> BuildPlan(ReleaseInfo release)
    {
        var plan = new List<UpdateItem>();
        foreach (var fileName in AreInfo.UpdatableAssets)
        {
            var asset = release.Assets.FirstOrDefault(a => a.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase));
            if (asset == null)
            {
                continue;
            }

            var target = FindLoadedAssemblyPath(fileName);
            if (target == null)
            {
                Warning($"ARE Updater: {fileName} is in the release but not loaded, skipping it.");
                continue;
            }

            if (new FileInfo(target).LinkTarget != null)
            {
                // Developer setups link the build output into the game folder - never overwrite those.
                Warning($"ARE Updater: {target} is a symbolic link, not updating anything.");
                return [];
            }

            if (!asset.Url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) || asset.Size <= 0)
            {
                throw new InvalidDataException($"Release asset {asset.Name} has an invalid url or size.");
            }

            plan.Add(new UpdateItem(asset, target));
        }

        if (plan.Count > 0 && !plan.Any(x => x.Asset.Name.Equals("TownOfUsMira.dll", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidDataException("Release does not contain TownOfUsMira.dll.");
        }

        return plan;
    }

    private static async Task DownloadAndVerifyAsync(HttpClient client, UpdateItem item)
    {
        var tempPath = item.TargetPath + DownloadSuffix;
        TryDelete(tempPath);

        using (var response = await client.GetAsync(item.Asset.Url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
        {
            response.EnsureSuccessStatusCode();
            var source = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            await using (source.ConfigureAwait(false))
            {
                var destination = File.Create(tempPath);
                await using (destination.ConfigureAwait(false))
                {
                    var buffer = new byte[81920];
                    int read;
                    while ((read = await source.ReadAsync(buffer).ConfigureAwait(false)) > 0)
                    {
                        await destination.WriteAsync(buffer.AsMemory(0, read)).ConfigureAwait(false);
                        Interlocked.Add(ref _bytesDone, read);
                    }
                }
            }
        }

        var length = new FileInfo(tempPath).Length;
        if (length != item.Asset.Size)
        {
            throw new InvalidDataException($"{item.Asset.Name}: expected {item.Asset.Size} bytes, got {length}.");
        }

        // GitHub publishes a "sha256:<hex>" digest for release assets.
        if (item.Asset.Digest?.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase) == true)
        {
            string actual;
            var stream = File.OpenRead(tempPath);
            await using (stream.ConfigureAwait(false))
            {
                using var sha = SHA256.Create();
                actual = Convert.ToHexString(await sha.ComputeHashAsync(stream).ConfigureAwait(false));
            }

            var expected = item.Asset.Digest["sha256:".Length..];
            if (!actual.Equals(expected, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException($"{item.Asset.Name}: SHA-256 mismatch.");
            }
        }
        else
        {
            Warning($"ARE Updater: {item.Asset.Name} has no digest, only the size was verified.");
        }
    }

    /// <summary>
    ///     Renames the loaded files to *.old and moves the downloads into place. Windows allows renaming a loaded
    ///     assembly, but not overwriting it. If anything fails, the already swapped files are rolled back so the
    ///     installation never ends up with mixed versions.
    /// </summary>
    private static void SwapAll(List<UpdateItem> plan)
    {
        var swapped = new List<UpdateItem>();
        try
        {
            foreach (var item in plan)
            {
                var oldPath = item.TargetPath + OldSuffix;
                TryDelete(oldPath);
                MoveWithRetry(item.TargetPath, oldPath);
                swapped.Add(item);
                MoveWithRetry(item.TargetPath + DownloadSuffix, item.TargetPath);
            }
        }
        catch
        {
            foreach (var item in swapped)
            {
                var oldPath = item.TargetPath + OldSuffix;
                TryDelete(item.TargetPath);
                if (File.Exists(oldPath))
                {
                    MoveWithRetry(oldPath, item.TargetPath);
                }
            }

            throw;
        }
    }

    /// <summary>
    ///     File.Move that waits out short locks. Antivirus software (e.g. Windows Defender) scans freshly written or
    ///     copied DLLs and briefly holds them open, which made the first update attempt fail with "being used by
    ///     another process" (round 4 test). Up to ~10 s in total.
    /// </summary>
    private static void MoveWithRetry(string source, string destination)
    {
        const int attempts = 10;
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                File.Move(source, destination);
                return;
            }
            catch (IOException e) when (attempt < attempts && File.Exists(source) && !File.Exists(destination))
            {
                Warning($"ARE Updater: {Path.GetFileName(source)} is locked ({e.Message}), retry {attempt}/{attempts - 1}");
                Thread.Sleep(200 * attempt);
            }
        }
    }

    private static void CleanupLeftovers()
    {
        foreach (var fileName in AreInfo.UpdatableAssets)
        {
            var path = FindLoadedAssemblyPath(fileName);
            if (path == null)
            {
                continue;
            }

            TryDelete(path + OldSuffix);
            TryDelete(path + DownloadSuffix);
        }
    }

    private static string? FindLoadedAssemblyPath(string fileName)
    {
        var name = Path.GetFileNameWithoutExtension(fileName);
        var assembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => !a.IsDynamic && string.Equals(a.GetName().Name, name, StringComparison.OrdinalIgnoreCase));
        var location = assembly?.Location;
        return string.IsNullOrEmpty(location) ? null : location;
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Best-effort cleanup of temporary files.")]
    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception e)
        {
            Warning($"ARE Updater: could not delete {path}: {e.Message}");
        }
    }

    private sealed record AssetInfo(string Name, string Url, long Size, string? Digest);

    private sealed record ReleaseInfo(string Tag, List<AssetInfo> Assets);

    private sealed record UpdateItem(AssetInfo Asset, string TargetPath);
}
