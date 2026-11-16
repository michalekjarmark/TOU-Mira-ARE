# Generates TownOfUs/Resources/Custom/{HandymanIcon,HandymanServiceButton,HandymanPizzaButton,
# HandymanWarehouseButton,HandymanStreamButton}.png (AmonRa Edition: Handyman)
# Usage: powershell -ExecutionPolicy Bypass -File tools/are-assets/draw-handyman-icons.ps1 -OutDir TownOfUs/Resources/Custom
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
function Polygon($points) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $p.AddPolygon([System.Drawing.PointF[]]$points)
    return $p
}
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
# An open-end wrench from (x1,y1) to (x2,y2): handle line plus a jaw ring at the end.
function Wrench($g, $x1, $y1, $x2, $y2, $color, $width) {
    $g.DrawLine((New-Pen $outline ($width + 6)), $x1, $y1, $x2, $y2)
    $g.DrawLine((New-Pen $color $width), $x1, $y1, $x2, $y2)
    $r = $width * 1.25
    $g.FillEllipse((Brush $outline), ($x2 - $r - 3), ($y2 - $r - 3), (2 * $r + 6), (2 * $r + 6))
    $g.FillEllipse((Brush $color), ($x2 - $r), ($y2 - $r), (2 * $r), (2 * $r))
    # The open jaw: clear a slot from the middle of the head outwards.
    $angle = [Math]::Atan2($y2 - $y1, $x2 - $x1) * 180 / [Math]::PI
    $g.TranslateTransform($x2, $y2)
    $g.RotateTransform($angle)
    $g.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
    $g.FillRectangle((Brush ([System.Drawing.Color]::Transparent)), (-$r * 0.1), (-$r * 0.42), ($r * 1.6), ($r * 0.84))
    $g.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceOver
    $g.ResetTransform()
}

$orange = Rgb 215 95 60
$orangeDark = Rgb 160 65 40
$steel = Rgb 175 180 192
$steelDark = Rgb 120 124 136

# ---- Handyman (128): a toolbox with a wrench sticking out ----
$c = New-Canvas 128 128; $bmp = $c[0]; $g = $c[1]
Wrench $g 92 70 98 22 $steel 9
$handle = New-Object System.Drawing.Drawing2D.GraphicsPath
$handle.AddArc(40, 26, 48, 36, 180, 180)
$g.DrawPath((New-Pen $outline 14), $handle)
$g.DrawPath((New-Pen $steelDark 7), $handle)
$box = RoundedRect 12 46 104 66 8
FillOutlined $g $box $orange 6
$g.DrawLine((New-Pen $orangeDark 6), 16, 64, 112, 64)
$latch = RoundedRect 54 56 20 16 3
FillOutlined $g $latch $steel 5
Save $bmp $g "HandymanIcon.png"

# ---- "Computer service" job (100): a monitor with a wrench ----
$c = New-Canvas 100 100; $bmp = $c[0]; $g = $c[1]
$screen = RoundedRect 8 12 74 54 6
FillOutlined $g $screen (Rgb 60 64 76) 6
$g.FillRectangle((Brush (Rgb 90 170 230)), 15, 19, 60, 40)
$stand = Polygon @((Pt 38 66), (Pt 52 66), (Pt 56 80), (Pt 34 80))
FillOutlined $g $stand $steelDark 5
$foot = RoundedRect 24 78 42 8 3
FillOutlined $g $foot $steelDark 5
Wrench $g 54 90 84 50 $steel 8
Save $bmp $g "HandymanServiceButton.png"

# ---- "Pizzeria" job (100): a pizza slice ----
$c = New-Canvas 100 100; $bmp = $c[0]; $g = $c[1]
$slice = Polygon @((Pt 50 92), (Pt 14 20), (Pt 86 20))
FillOutlined $g $slice (Rgb 250 200 80) 6
$crust = RoundedRect 10 10 80 16 8
FillOutlined $g $crust (Rgb 200 130 60) 6
foreach ($p in @(@(36, 34), @(58, 40), @(46, 58), @(42, 76))) {
    $g.FillEllipse((Brush $outline), ($p[0] - 8), ($p[1] - 8), 16, 16)
    $g.FillEllipse((Brush (Rgb 200 50 45)), ($p[0] - 6), ($p[1] - 6), 12, 12)
}
Save $bmp $g "HandymanPizzaButton.png"

# ---- "Warehouse" job (100): a cardboard box ----
$c = New-Canvas 100 100; $bmp = $c[0]; $g = $c[1]
$top = Polygon @((Pt 10 30), (Pt 50 14), (Pt 90 30), (Pt 50 46))
FillOutlined $g $top (Rgb 215 170 110) 6
$left = Polygon @((Pt 10 30), (Pt 50 46), (Pt 50 92), (Pt 10 76))
FillOutlined $g $left (Rgb 190 140 85) 6
$right = Polygon @((Pt 50 46), (Pt 90 30), (Pt 90 76), (Pt 50 92))
FillOutlined $g $right (Rgb 165 115 65) 6
$g.DrawLine((New-Pen (Rgb 235 220 180) 7), 30, 22, 70, 38)
$g.DrawLine((New-Pen (Rgb 235 220 180) 7), 70, 38, 70, 56)
foreach ($x in @(20, 30)) {
    $g.DrawLine((New-Pen $outline 4), $x, 58, $x, 70)
    $g.DrawLine((New-Pen $outline 4), ($x - 4), 62, $x, 57)
    $g.DrawLine((New-Pen $outline 4), ($x + 4), 62, $x, 57)
}
Save $bmp $g "HandymanWarehouseButton.png"

# ---- "Stream" job (100): a video camera with a red dot ----
$c = New-Canvas 100 100; $bmp = $c[0]; $g = $c[1]
$body = RoundedRect 8 34 60 44 8
FillOutlined $g $body (Rgb 70 74 86) 6
$lens = Polygon @((Pt 66 48), (Pt 92 36), (Pt 92 76), (Pt 66 64))
FillOutlined $g $lens (Rgb 50 54 64) 6
$g.FillEllipse((Brush $outline), 14, 8, 26, 26)
$g.FillEllipse((Brush $steel), 17, 11, 20, 20)
$g.FillEllipse((Brush $outline), 38, 8, 26, 26)
$g.FillEllipse((Brush $steel), 41, 11, 20, 20)
$g.FillEllipse((Brush $outline), 14, 42, 22, 22)
$g.FillEllipse((Brush (Rgb 230 50 50)), 17, 45, 16, 16)
$g.FillEllipse((Brush $white), 42, 46, 18, 6)
Save $bmp $g "HandymanStreamButton.png"
