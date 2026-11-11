namespace TownOfUs.Are;

/// <summary>
///     Central place for everything that identifies the AmonRa Edition fork.
///     The plugin GUID stays "auavengers.tou.mira" on purpose (configs, presets and extension mods depend on it).
/// </summary>
public static class AreInfo
{
    public const string DisplayName = "Town of Us: Mira - AmonRa Edition";
    public const string ShortName = "TOU Mira ARE";
    public const string DiscordName = "TOU:M ARE";

    public const string RepoOwner = "michalekjarmark";
    public const string RepoName = "TOU-Mira-ARE";

#pragma warning disable S1075 // URIs should not be hardcoded - Justification: fixed release source of this fork
    public const string RepoUrl = $"https://github.com/{RepoOwner}/{RepoName}";
    public const string ReleasesUrl = $"{RepoUrl}/releases/latest";
    public const string LatestReleaseApiUrl = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";
#pragma warning restore S1075

    /// <summary>
    ///     Release assets the updater is allowed to replace. Each one is only replaced if an assembly with the same
    ///     name is currently loaded, and it is written next to that loaded assembly.
    /// </summary>
    public static readonly string[] UpdatableAssets = ["TownOfUsMira.dll", "MiraAPI.dll", "AchievementsAPI.dll"];

    /// <summary>
    ///     Mod news are always loaded from the embedded resources, as upstream's online news do not match this fork.
    /// </summary>
    public const bool NewsAlwaysLocal = true;

    /// <summary>
    ///     Discord invite for the main menu "Discord Server" button, set at build time from the git-ignored
    ///     are.local.props (AssemblyMetadata "AreDiscordUrl"), so it is not in the public source code.
    ///     Null when not set or not a Discord invite link: the button then keeps the Town of Us: Mira server.
    /// </summary>
    public static string? DiscordUrl { get; } = ReadDiscordUrl();

    private static string? ReadDiscordUrl()
    {
        var value = ReadBuildMetadata("AreDiscordUrl");
        var isInvite = value != null &&
                       (value.StartsWith("https://discord.gg/", StringComparison.Ordinal) ||
                        value.StartsWith("https://discord.com/invite/", StringComparison.Ordinal));
        return isInvite ? value : null;
    }

    /// <summary>
    ///     Discord application (Developer Portal) used for the Rich Presence, set at build time from are.local.props
    ///     (AssemblyMetadata "AreDiscordAppId"). Discord shows that application's name and "icon" art asset as the game.
    ///     Null when not set: Town of Us: Mira's application is used (shown as "Town of Us Mira").
    /// </summary>
    public static long? DiscordAppId { get; } =
        long.TryParse(ReadBuildMetadata("AreDiscordAppId"), System.Globalization.NumberStyles.None,
            System.Globalization.CultureInfo.InvariantCulture, out var id) && id > 0
            ? id
            : null;

    private static string? ReadBuildMetadata(string key)
    {
        return typeof(AreInfo).Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyMetadataAttribute), false)
            .OfType<System.Reflection.AssemblyMetadataAttribute>()
            .FirstOrDefault(a => a.Key == key)?.Value?.Trim();
    }
}
