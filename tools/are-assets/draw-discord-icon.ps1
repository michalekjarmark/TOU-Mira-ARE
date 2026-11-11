# Generates AreDiscordIcon.png (1024x1024): placeholder icon of our Discord application (Rich Presence "icon" art
# asset and app icon), in the style of the logo placeholder (draw-logo-placeholder.ps1). Replace it with the real logo.
# The content stays inside the middle circle, because Discord crops icons to a circle in some places.
# Usage: powershell -ExecutionPolicy Bypass -File tools/are-assets/draw-discord-icon.ps1 -OutDir manual_tests
param([string]$OutDir)
Add-Type -AssemblyName System.Drawing
New-Item -ItemType Directory -Force $OutDir | Out-Null

$size = 1024
$bmp = New-Object System.Drawing.Bitmap $size, $size, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAlias

# Background: dark blue-violet gradient, a soft lighter circle behind the text.
$full = New-Object System.Drawing.Rectangle 0, 0, $size, $size
$bg = New-Object System.Drawing.Drawing2D.LinearGradientBrush $full, ([System.Drawing.Color]::FromArgb(255, 40, 34, 70)), ([System.Drawing.Color]::FromArgb(255, 14, 12, 28)), 90
$g.FillRectangle($bg, $full)
$glow = New-Object System.Drawing.Drawing2D.GraphicsPath
$glow.AddEllipse(112, 112, 800, 800)
$glowBrush = New-Object System.Drawing.Drawing2D.PathGradientBrush $glow
$glowBrush.CenterColor = [System.Drawing.Color]::FromArgb(150, 120, 96, 170)
$glowBrush.SurroundColors = @([System.Drawing.Color]::FromArgb(0, 120, 96, 170))
$g.FillPath($glowBrush, $glow)

$gold = [System.Drawing.Color]::FromArgb(255, 214, 168, 64)
$goldDark = [System.Drawing.Color]::FromArgb(255, 150, 105, 25)
$outline = [System.Drawing.Color]::FromArgb(255, 16, 16, 18)
$family = New-Object System.Drawing.FontFamily "Arial Black"
$fmt = New-Object System.Drawing.StringFormat
$fmt.Alignment = [System.Drawing.StringAlignment]::Center
$fmt.LineAlignment = [System.Drawing.StringAlignment]::Center

function Draw-OutlinedText($text, $emSize, $rect, $fillTop, $fillBottom) {
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddString($text, $family, [int][System.Drawing.FontStyle]::Regular, $emSize, $rect, $fmt)
    $pen = New-Object System.Drawing.Pen $outline, ($emSize / 7)
    $pen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    $g.DrawPath($pen, $path)
    $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush $rect, $fillTop, $fillBottom, 90
    $g.FillPath($brush, $path)
}

Draw-OutlinedText "ARE" 330 (New-Object System.Drawing.RectangleF 0, 250, $size, 400) $gold $goldDark
Draw-OutlinedText "TOU Mira" 120 (New-Object System.Drawing.RectangleF 0, 610, $size, 180) ([System.Drawing.Color]::White) ([System.Drawing.Color]::FromArgb(255, 200, 200, 200))

$g.Dispose()
$bmp.Save((Join-Path $OutDir "AreDiscordIcon.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Write-Host "wrote AreDiscordIcon.png (placeholder)"
