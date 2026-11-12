# Generates TownOfUs/Resources/Custom/RivalIcon.png (Rival modifier, AmonRa Edition)
# Usage: powershell -ExecutionPolicy Bypass -File tools/are-assets/draw-rival-icon.ps1 -OutDir TownOfUs/Resources/Custom
# Two crossed swords with blue hilts (Rival color 56,120,240): the opposite of the Lovers' heart.
param([string]$OutDir)
Add-Type -AssemblyName System.Drawing
New-Item -ItemType Directory -Force $OutDir | Out-Null

$outline = [System.Drawing.Color]::FromArgb(255, 30, 28, 36)
$blade = [System.Drawing.Color]::FromArgb(255, 214, 220, 230)
$bladeShade = [System.Drawing.Color]::FromArgb(255, 160, 168, 182)
$hilt = [System.Drawing.Color]::FromArgb(255, 56, 120, 240)
$hiltDark = [System.Drawing.Color]::FromArgb(255, 34, 78, 170)
$pommel = [System.Drawing.Color]::FromArgb(255, 235, 190, 70)

function Poly($points) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $p.AddPolygon([System.Drawing.PointF[]]($points | ForEach-Object { New-Object System.Drawing.PointF $_[0], $_[1] }))
    return $p
}

# One sword drawn pointing up along the y axis, centered on x = 0; the caller rotates the canvas.
function DrawSword($g) {
    $ob = New-Object System.Drawing.SolidBrush $outline
    # blade (with outline), tip at y = -58
    $g.FillPath($ob, (Poly @(@(-8, 14), @(-8, -44), @(0, -60), @(8, -44), @(8, 14))))
    $g.FillPath((New-Object System.Drawing.SolidBrush $blade), (Poly @(@(-4.5, 12), @(-4.5, -42), @(0, -52), @(4.5, -42), @(4.5, 12))))
    $g.FillPath((New-Object System.Drawing.SolidBrush $bladeShade), (Poly @(@(0, 12), @(0, -52), @(4.5, -42), @(4.5, 12))))
    # cross guard
    $g.FillRectangle($ob, -22, 12, 44, 13)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush $hilt), -19, 15, 38, 7)
    # grip
    $g.FillRectangle($ob, -7, 24, 14, 26)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush $hiltDark), -4, 25, 8, 24)
    # pommel
    $g.FillEllipse($ob, -10, 46, 20, 20)
    $g.FillEllipse((New-Object System.Drawing.SolidBrush $pommel), -6.5, 49.5, 13, 13)
}

$bmp = New-Object System.Drawing.Bitmap 128, 128, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.Clear([System.Drawing.Color]::Transparent)

foreach ($angle in @(-45, 45)) {
    $g.ResetTransform()
    $g.TranslateTransform(64, 66)
    $g.RotateTransform($angle)
    DrawSword $g
}

$g.Dispose()
$bmp.Save((Join-Path $OutDir "RivalIcon.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Write-Host "wrote RivalIcon.png"
