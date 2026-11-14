# Builds TownOfUs/Resources/Custom/AreMenuLogo.png, the main menu logo, from AreLogo.png (the splash screen logo):
# crops it to its visible pixels and fits it into a transparent 2280x750 canvas, the size of TOU's banner it replaces,
# so the main menu layout (AspectSize of "Sizer") stays the same. Run it again after replacing AreLogo.png.
# Usage: powershell -ExecutionPolicy Bypass -File tools/are-assets/make-menu-logo.ps1 -Dir TownOfUs/Resources/Custom
param([string]$Dir)
Add-Type -AssemblyName System.Drawing

$src = New-Object System.Drawing.Bitmap (Resolve-Path (Join-Path $Dir "AreLogo.png")).Path
$minX = $src.Width; $minY = $src.Height; $maxX = -1; $maxY = -1
for ($y = 0; $y -lt $src.Height; $y++) {
    for ($x = 0; $x -lt $src.Width; $x++) {
        if ($src.GetPixel($x, $y).A -gt 8) {
            if ($x -lt $minX) { $minX = $x }; if ($x -gt $maxX) { $maxX = $x }
            if ($y -lt $minY) { $minY = $y }; if ($y -gt $maxY) { $maxY = $y }
        }
    }
}
$cw = $maxX - $minX + 1; $ch = $maxY - $minY + 1

# TOU's banner fills about 86% of its width and 87% of its height.
$w = 2280; $h = 750
$scale = [Math]::Min(1980 / $cw, 650 / $ch)
$dw = [int]($cw * $scale); $dh = [int]($ch * $scale)
$bmp = New-Object System.Drawing.Bitmap $w, $h, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.Clear([System.Drawing.Color]::Transparent)
$g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
$dest = New-Object System.Drawing.Rectangle ([int](($w - $dw) / 2)), ([int](($h - $dh) / 2)), $dw, $dh
$g.DrawImage($src, $dest, $minX, $minY, $cw, $ch, [System.Drawing.GraphicsUnit]::Pixel)
$g.Dispose()
$src.Dispose()
$bmp.Save((Join-Path $Dir "AreMenuLogo.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Write-Host "wrote AreMenuLogo.png (content ${cw}x${ch} -> ${dw}x${dh})"
