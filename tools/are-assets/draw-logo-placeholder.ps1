# Generates a PLACEHOLDER TownOfUs/Resources/Custom/AreLogo.png (1600x800, transparent) for the splash screen.
# Replace the PNG with the real logo (same name and size) when it exists - no code change needed.
# Usage: powershell -ExecutionPolicy Bypass -File tools/are-assets/draw-logo-placeholder.ps1 -OutDir TownOfUs/Resources/Custom
param([string]$OutDir)
Add-Type -AssemblyName System.Drawing
New-Item -ItemType Directory -Force $OutDir | Out-Null

$w = 1600; $h = 800
$bmp = New-Object System.Drawing.Bitmap $w, $h, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAlias
$g.Clear([System.Drawing.Color]::Transparent)

$gold = [System.Drawing.Color]::FromArgb(255, 214, 168, 64)
$goldDark = [System.Drawing.Color]::FromArgb(255, 150, 105, 25)
$outline = [System.Drawing.Color]::FromArgb(255, 16, 16, 18)
$family = New-Object System.Drawing.FontFamily "Arial Black"
$fmt = New-Object System.Drawing.StringFormat
$fmt.Alignment = [System.Drawing.StringAlignment]::Center
$fmt.LineAlignment = [System.Drawing.StringAlignment]::Center

function Draw-OutlinedText($text, $size, $rect, $fillTop, $fillBottom) {
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddString($text, $family, [int][System.Drawing.FontStyle]::Regular, $size, $rect, $fmt)
    $pen = New-Object System.Drawing.Pen $outline, ($size / 7)
    $pen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    $g.DrawPath($pen, $path)
    $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush $rect, $fillTop, $fillBottom, 90
    $g.FillPath($brush, $path)
}

Draw-OutlinedText "AmonRa" 300 (New-Object System.Drawing.RectangleF 0, 90, $w, 400) $gold $goldDark
Draw-OutlinedText "EDITION" 130 (New-Object System.Drawing.RectangleF 0, 470, $w, 220) ([System.Drawing.Color]::White) ([System.Drawing.Color]::FromArgb(255, 200, 200, 200))

$g.Dispose()
$bmp.Save((Join-Path $OutDir "AreLogo.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Write-Host "wrote AreLogo.png (placeholder)"
