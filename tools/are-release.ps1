<#
.SYNOPSIS
    Publishes a new Town of Us: Mira - AmonRa Edition release on GitHub.
    Friends' games download it automatically on the next start (TownOfUs/Are/AreUpdater.cs).

.DESCRIPTION
    1. Bump <VersionSuffix> in Directory.Build.props (are.1 -> are.2 ...) and commit everything first.
    2. Run from the repo root:  powershell -ExecutionPolicy Bypass -File tools\are-release.ps1
    Requirements: clean working tree on branch 'are', dotnet SDK, gh CLI logged in (gh auth status).

.PARAMETER NotesFile
    Optional markdown file with release notes. By default the "## [<version>]" section of CHANGELOG.md is used
    (the script refuses to release without it).

.PARAMETER DryRun
    Builds and prepares the files, but does not tag, push or publish anything.

.PARAMETER Yes
    Publishes without asking for confirmation (for non-interactive runs, e.g. from Claude Code).

.PARAMETER NoUpload
    Tags, pushes and creates the GitHub release as a draft without files (for a slow upload). Prints the commands
    that upload the files and publish the release later. Friends' updaters ignore drafts until then.
#>
param(
    [string]$NotesFile = "",
    [switch]$DryRun,
    [switch]$Yes,
    [switch]$NoUpload
)

$ErrorActionPreference = "Stop"
$Repo = "michalekjarmark/TOU-Mira-ARE"
$Branch = "are"
$Root = Split-Path -Parent $PSScriptRoot
Set-Location $Root

function Fail($msg) { Write-Host "ERROR: $msg" -ForegroundColor Red; exit 1 }

# --- Version ---------------------------------------------------------------
[xml]$props = Get-Content (Join-Path $Root "Directory.Build.props")
$group = $props.Project.PropertyGroup | Where-Object { $_.VersionPrefix } | Select-Object -First 1
$prefix = $group.VersionPrefix
$suffix = $group.VersionSuffix
if ($suffix -notmatch '^are\.\d+$') { Fail "VersionSuffix must look like 'are.<number>' (is '$suffix')." }
$version = "$prefix-$suffix"
$tag = "v$version"
Write-Host "Release: $tag" -ForegroundColor Cyan

# --- Git checks ------------------------------------------------------------
$current = (git rev-parse --abbrev-ref HEAD).Trim()
if ($current -ne $Branch) { Fail "You are on '$current', switch to '$Branch' first." }
if (git status --porcelain) {
    if ($DryRun) { Write-Warning "Working tree is not clean (allowed for -DryRun only)." }
    else { Fail "Working tree is not clean. Commit or stash your changes first." }
}
if (git tag -l $tag) { Fail "Tag $tag already exists. Bump VersionSuffix in Directory.Build.props." }

# --- Private build settings (git-ignored are.local.props) -----------------
# Without it the release would silently open the upstream Discord server from the main menu.
$localProps = Join-Path $Root "are.local.props"
$discordUrl = $null
if (Test-Path $localProps) {
    [xml]$lp = Get-Content $localProps -Raw -Encoding UTF8
    $discordUrl = @($lp.Project.PropertyGroup | ForEach-Object { $_.AreDiscordUrl } | Where-Object { $_ -and $_.Trim() }) | Select-Object -First 1
}
if (-not $discordUrl) { Fail "are.local.props with <AreDiscordUrl> is missing (copy are.local.props.example and fill it in)." }

# --- Release notes from CHANGELOG.md ----------------------------------------
# The section "## [<version>] - <date>" becomes the GitHub release notes, so the changelog can't be forgotten.
if (-not $NotesFile) {
    $changelog = Get-Content (Join-Path $Root "CHANGELOG.md") -Raw -Encoding UTF8
    $pattern = "(?ms)^## \[" + [regex]::Escape($version) + "\][^\n]*\n(.*?)(?=^## \[|^\[[^\]]+\]:|\z)"
    $match = [regex]::Match($changelog, $pattern)
    if (-not $match.Success -or -not $match.Groups[1].Value.Trim()) {
        Fail "CHANGELOG.md has no section '## [$version] - <date>'. Rename [Unreleased] to it (and add a new empty [Unreleased])."
    }
    $NotesFile = Join-Path $env:TEMP "are-release-notes-$tag.md"
    $notes = $match.Groups[1].Value.Trim() + "`n`n---`n`n**New installation:** download ``TOU-Mira-ARE-$tag-x64-Steam-Epic.zip`` and follow ``INSTALACJA.txt`` / the [README](https://github.com/$Repo#installation). Existing installations update automatically.`n`n**Nowa instalacja:** pobierz ``TOU-Mira-ARE-$tag-x64-Steam-Epic.zip`` i postępuj zgodnie z ``INSTALACJA.txt``. Kto ma już AmonRa Edition, dostanie tę wersję automatycznie."
    [System.IO.File]::WriteAllText($NotesFile, $notes, (New-Object System.Text.UTF8Encoding $false))
    Write-Host "Release notes taken from CHANGELOG.md" -ForegroundColor Cyan
}

# --- Build -----------------------------------------------------------------
dotnet build TownOfUs-Mira.sln -c Release -v minimal
if ($LASTEXITCODE -ne 0) { Fail "Build failed." }

$files = @(
    "TownOfUs\bin\Release\TownOfUsMira.dll",
    "GitSubmodules\MiraAPI\MiraAPI\bin\Release\net6.0\MiraAPI.dll",
    "GitSubmodules\Achievements-API\AchievementsAPI\bin\Release\net6.0\AchievementsAPI.dll"
)
foreach ($f in $files) { if (-not (Test-Path $f)) { Fail "Missing build output: $f" } }

$stage = Join-Path $Root "TownOfUs\bin\are-release\$tag"
Remove-Item $stage -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force $stage | Out-Null
foreach ($f in $files) { Copy-Item $f $stage -Force }

# --- Full install zip: official TOU Mira x64 package + our DLLs -----------
# Pinned base package. When upstream publishes a new package (e.g. new BepInEx), update both values;
# the hash is the "digest" GitHub shows for the asset (gh api repos/AU-Avengers/TOU-Mira/releases).
$BaseZipUrl = "https://github.com/AU-Avengers/TOU-Mira/releases/download/1.7.9-beta1/TouMira.v1.7.9-beta1-x64-Steam-Epic-MsStore.zip"
$BaseZipSha256 = "b2e2163f7d0f41fb3cdf50352119925c42c80bc55a804507cf9e923aeb150afd"

$cache = Join-Path $Root "TownOfUs\bin\are-release\cache"
New-Item -ItemType Directory -Force $cache | Out-Null
$baseZip = Join-Path $cache (Split-Path $BaseZipUrl -Leaf)
if (-not (Test-Path $baseZip)) {
    Write-Host "Downloading base package (~100 MB, once)..." -ForegroundColor Cyan
    curl.exe -L --fail -o $baseZip $BaseZipUrl
    if ($LASTEXITCODE -ne 0) { Remove-Item $baseZip -ErrorAction SilentlyContinue; Fail "Download of the base package failed." }
}
$hash = (Get-FileHash $baseZip -Algorithm SHA256).Hash
if ($hash -ne $BaseZipSha256.ToUpperInvariant()) { Remove-Item $baseZip; Fail "Base package hash mismatch ($hash). Deleted, rerun to download again." }

Add-Type -AssemblyName System.IO.Compression.FileSystem
$work = Join-Path $stage "_zip"
[System.IO.Compression.ZipFile]::ExtractToDirectory($baseZip, $work)
$inner = Get-ChildItem $work -Directory | Select-Object -First 1
$folderName = "TOU-Mira-ARE-$tag-x64-Steam-Epic"
$pkg = Join-Path $work $folderName
Rename-Item $inner.FullName $folderName
foreach ($f in $files) { Copy-Item $f (Join-Path $pkg "BepInEx\plugins") -Force }
# Doorstop as version.dll instead of winhttp.dll: with winhttp.dll BepInEx crashed on first start
# (AccessViolation in WinHttpOpen while downloading unity-libs, confirmed on Windows 11 build 26300).
$proxy = Join-Path $pkg "winhttp.dll"
if (Test-Path $proxy) { Rename-Item $proxy "version.dll" }
$readme = (Get-Content (Join-Path $PSScriptRoot "are-release\INSTALACJA.txt") -Raw -Encoding UTF8).Replace("{VERSION}", $version).Replace("{FOLDER}", $folderName)
[System.IO.File]::WriteAllText((Join-Path $pkg "INSTALACJA.txt"), $readme, (New-Object System.Text.UTF8Encoding $true))
$fullZip = Join-Path $stage "$folderName.zip"
# Built entry by entry: ZipFile.CreateFromDirectory on .NET Framework writes '\' separators and drops empty folders.
Add-Type -AssemblyName System.IO.Compression
$zipStream = [System.IO.File]::Open($fullZip, [System.IO.FileMode]::Create)
$archive = New-Object System.IO.Compression.ZipArchive($zipStream, [System.IO.Compression.ZipArchiveMode]::Create)
try {
    Get-ChildItem $pkg -Recurse -Force | ForEach-Object {
        $rel = $folderName + "/" + $_.FullName.Substring($pkg.Length + 1).Replace("\", "/")
        if ($_.PSIsContainer) {
            if (-not (Get-ChildItem $_.FullName -Force | Select-Object -First 1)) { [void]$archive.CreateEntry($rel + "/") }
        } else {
            [void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $_.FullName, $rel, [System.IO.Compression.CompressionLevel]::Optimal)
        }
    }
} finally {
    $archive.Dispose(); $zipStream.Dispose()
}
Remove-Item $work -Recurse -Force

Write-Host "Files ready in $stage" -ForegroundColor Green
Get-ChildItem $stage -File | ForEach-Object { "{0,-50} {1,14:N0} B" -f $_.Name, $_.Length }

if ($DryRun) { Write-Host "Dry run - nothing was published." -ForegroundColor Yellow; exit 0 }

if (-not $Yes) {
    $answer = Read-Host "Publish $tag to github.com/$Repo ? (y/n)"
    if ($answer -ne "y") { Write-Host "Aborted."; exit 0 }
}

# --- Publish ---------------------------------------------------------------
git tag -a $tag -m "Town of Us: Mira - AmonRa Edition $version"
git push origin $Branch
if ($LASTEXITCODE -ne 0) { Fail "git push of branch failed." }
git push origin $tag
if ($LASTEXITCODE -ne 0) { Fail "git push of tag failed." }

if ($NoUpload) {
    gh release create $tag --repo $Repo --title "AmonRa Edition $version" --notes-file $NotesFile --draft
    if ($LASTEXITCODE -ne 0) { Fail "gh release create --draft failed (the tag is already pushed - fix and rerun it)." }
    $relStage = $stage.Substring($Root.Length + 1)
    Write-Host "Draft created (no files yet). To finish, from the repo root:" -ForegroundColor Green
    Write-Host "  gh release upload $tag $relStage\* --repo $Repo --clobber"
    Write-Host "  gh release edit $tag --repo $Repo --draft=false --latest"
    exit 0
}

$assets = Get-ChildItem $stage -File | ForEach-Object { $_.FullName }
$ghArgs = @("release", "create", $tag) + $assets + @("--repo", $Repo, "--title", "AmonRa Edition $version", "--latest")
$ghArgs += @("--notes-file", $NotesFile)
gh @ghArgs
if ($LASTEXITCODE -ne 0) { Fail "gh release create failed (the tag is already pushed - fix and rerun 'gh release create $tag ...')." }

Write-Host "Done! Friends will get $version on their next game start." -ForegroundColor Green
