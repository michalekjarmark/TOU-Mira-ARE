# Generates TownOfUs/Resources/Custom/InsiderIcon.png (Insider role, AmonRa Edition)
# Usage: powershell -ExecutionPolicy Bypass -File tools/are-assets/draw-insider-icon.ps1 -OutDir TownOfUs/Resources/Custom
# An electric plug ("wtyczka" in Polish means both a plug and a mole) with an impostor-red body and a checkmark.
param([string]$OutDir)
Add-Type -AssemblyName System.Drawing
New-Item -ItemType Directory -Force $OutDir | Out-Null

$outline = [System.Drawing.Color]::FromArgb(255, 30, 28, 36)
$red = [System.Drawing.Color]::FromArgb(255, 214, 40, 40)
$redDark = [System.Drawing.Color]::FromArgb(255, 150, 24, 30)
$metal = [System.Drawing.Color]::FromArgb(255, 205, 210, 220)
$cable = [System.Drawing.Color]::FromArgb(255, 70, 70, 80)
$check = [System.Drawing.Color]::FromArgb(255, 245, 245, 240)

function RoundedRect($x, $y, $w, $h, $r) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $p.AddArc($x, $y, 2 * $r, 2 * $r, 180, 90)
    $p.AddArc($x + $w - 2 * $r, $y, 2 * $r, 2 * $r, 270, 90)
    $p.AddArc($x + $w - 2 * $r, $y + $h - 2 * $r, 2 * $r, 2 * $r, 0, 90)
    $p.AddArc($x, $y + $h - 2 * $r, 2 * $r, 2 * $r, 90, 90)
    $p.CloseFigure()
    return $p
}

$bmp = New-Object System.Drawing.Bitmap 128, 128, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.Clear([System.Drawing.Color]::Transparent)

# cable curling down
$penOut = New-Object System.Drawing.Pen $outline, 14
$penOut.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$penOut.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
$penIn = New-Object System.Drawing.Pen $cable, 8
$penIn.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$penIn.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
$curve = [System.Drawing.PointF[]]@(
    (New-Object System.Drawing.PointF 64, 96),
    (New-Object System.Drawing.PointF 60, 110),
    (New-Object System.Drawing.PointF 40, 116),
    (New-Object System.Drawing.PointF 22, 108))
$g.DrawCurve($penOut, $curve)
$g.DrawCurve($penIn, $curve)

# prongs
foreach ($px in @(44, 74)) {
    $g.FillPath((New-Object System.Drawing.SolidBrush $outline), (RoundedRect ($px - 3) 6 16 34 5))
    $g.FillPath((New-Object System.Drawing.SolidBrush $metal), (RoundedRect $px 9 10 30 3))
}

# body
$g.FillPath((New-Object System.Drawing.SolidBrush $outline), (RoundedRect 26 32 76 64 16))
$g.FillPath((New-Object System.Drawing.SolidBrush $red), (RoundedRect 31 37 66 54 12))
$g.FillPath((New-Object System.Drawing.SolidBrush $redDark), (RoundedRect 31 74 66 17 8))
$g.FillRectangle((New-Object System.Drawing.SolidBrush $red), 31, 74, 66, 6)
# neck
$g.FillPath((New-Object System.Drawing.SolidBrush $outline), (RoundedRect 52 90 24 12 4))

# checkmark: "the task is really done"
$pen = New-Object System.Drawing.Pen $check, 9
$pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
$pen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
$g.DrawLines($pen, [System.Drawing.PointF[]]@(
    (New-Object System.Drawing.PointF 47, 62),
    (New-Object System.Drawing.PointF 59, 73),
    (New-Object System.Drawing.PointF 81, 49)))

$g.Dispose()
$bmp.Save((Join-Path $OutDir "InsiderIcon.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Write-Host "wrote InsiderIcon.png"
