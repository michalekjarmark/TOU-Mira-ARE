# Generates TownOfUs/Resources/Custom/{ModeratorIcon,ModeratorButton,ModeratorTypeButton}.png (AmonRa Edition: Moderator)
# Usage: powershell -ExecutionPolicy Bypass -File tools/are-assets/draw-moderator-icons.ps1 -OutDir TownOfUs/Resources/Custom
param([string]$OutDir)
Add-Type -AssemblyName System.Drawing
New-Item -ItemType Directory -Force $OutDir | Out-Null

$outline = [System.Drawing.Color]::FromArgb(255, 30, 28, 36)
$white = [System.Drawing.Color]::FromArgb(255, 245, 245, 250)

function New-Canvas($w, $h) {
    $bmp = New-Object System.Drawing.Bitmap $w, $h, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([System.Drawing.Color]::Transparent)
    return @($bmp, $g)
}
function Save($bmp, $g, $name) {
    $g.Dispose(); $bmp.Save((Join-Path $OutDir $name), [System.Drawing.Imaging.ImageFormat]::Png); $bmp.Dispose()
    Write-Host "wrote $name"
}
function New-Pen($color, $width) {
    $pen = New-Object System.Drawing.Pen $color, $width
    $pen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    $pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    return $pen
}
function Brush($color) { return New-Object System.Drawing.SolidBrush $color }
function Rgb($r, $g, $b) { return [System.Drawing.Color]::FromArgb(255, $r, $g, $b) }
function Pt($x, $y) { return New-Object System.Drawing.PointF $x, $y }
function RoundedRect($x, $y, $w, $h, $r) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $p.AddArc($x, $y, 2 * $r, 2 * $r, 180, 90)
    $p.AddArc($x + $w - 2 * $r, $y, 2 * $r, 2 * $r, 270, 90)
    $p.AddArc($x + $w - 2 * $r, $y + $h - 2 * $r, 2 * $r, 2 * $r, 0, 90)
    $p.AddArc($x, $y + $h - 2 * $r, 2 * $r, 2 * $r, 90, 90)
    $p.CloseFigure()
    return $p
}
function FillOutlined($g, $path, $fill, $width) {
    $g.DrawPath((New-Pen $outline $width), $path)
    $g.FillPath((Brush $fill), $path)
}

$red = Rgb 214 52 52
$redDark = Rgb 150 30 34
$wood = Rgb 168 112 64
$woodDark = Rgb 120 76 40
$steel = Rgb 150 156 170
$steelLight = Rgb 200 206 218

# A ban hammer, drawn around (0, 0) = middle of the head, handle pointing down.
function DrawHammer($g, $x, $y, $angle, $scale, $width) {
    $g.ResetTransform()
    $g.TranslateTransform($x, $y)
    $g.RotateTransform($angle)
    $g.ScaleTransform($scale, $scale)
    $handle = RoundedRect (-6) 8 12 56 5
    FillOutlined $g $handle $wood $width
    $g.DrawLine((New-Pen $woodDark 3), 0, 16, 0, 58)
    $head = RoundedRect (-30) (-16) 60 30 7
    FillOutlined $g $head $steel $width
    $band = RoundedRect (-30) (-16) 12 30 5
    FillOutlined $g $band $red $width
    $band2 = RoundedRect 18 (-16) 12 30 5
    FillOutlined $g $band2 $red $width
    $g.FillRectangle((Brush $steelLight), -16, -12, 32, 6)
    $g.ResetTransform()
}

# ---- Moderator (128): a ban hammer ----
$c = New-Canvas 128 128; $bmp = $c[0]; $g = $c[1]
DrawHammer $g 60 44 -30 1.45 6
Save $bmp $g "ModeratorIcon.png"

# ---- Moderator "Ban" button (100): a red prohibition sign with a small hammer ----
$c = New-Canvas 100 100; $bmp = $c[0]; $g = $c[1]
$g.DrawEllipse((New-Pen $outline 20), 14, 14, 72, 72)
$g.DrawEllipse((New-Pen $red 12), 14, 14, 72, 72)
$g.DrawLine((New-Pen $outline 20), 28, 28, 72, 72)
$g.DrawLine((New-Pen $red 12), 28, 28, 72, 72)
DrawHammer $g 70 30 -35 0.62 7
Save $bmp $g "ModeratorButton.png"

# ---- Moderator "Ban Type" button (100): a list with a turning arrow ----
$c = New-Canvas 100 100; $bmp = $c[0]; $g = $c[1]
$sheet = RoundedRect 14 12 50 66 6
FillOutlined $g $sheet $white 6
foreach ($ly in @(28, 42, 56)) {
    $g.FillEllipse((Brush $red), 20, ($ly - 4), 8, 8)
    $g.DrawLine((New-Pen (Rgb 120 124 136) 4), 34, $ly, 54, $ly)
}
# Clockwise arc from the upper left over the right side down to the bottom, the head at its end pointing left.
$g.DrawArc((New-Pen $outline 14), 46, 46, 40, 40, 200, 235)
$g.DrawArc((New-Pen $red 7), 46, 46, 40, 40, 200, 235)
$tip = New-Object System.Drawing.Drawing2D.GraphicsPath
$tip.AddPolygon([System.Drawing.PointF[]]@((Pt 72 76), (Pt 72 96), (Pt 56 86)))
FillOutlined $g $tip $red 4
Save $bmp $g "ModeratorTypeButton.png"
