# Generates TownOfUs/Resources/Custom/{ChairmanIcon,ChairmanOrderButton,ChairmanMeetingButton}.png
# (AmonRa Edition: Chairman)
# Usage: powershell -ExecutionPolicy Bypass -File tools/are-assets/draw-chairman-icons.ps1 -OutDir TownOfUs/Resources/Custom
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

$blue = Rgb 70 110 190
$blueLight = Rgb 120 160 230
$gold = Rgb 240 196 70
$goldDark = Rgb 190 140 40

# ---- Chairman (128): a necktie under a shirt collar ----
$c = New-Canvas 128 128; $bmp = $c[0]; $g = $c[1]
$collarL = Polygon @((Pt 24 14), (Pt 64 30), (Pt 50 50), (Pt 30 40))
FillOutlined $g $collarL $white 6
$collarR = Polygon @((Pt 104 14), (Pt 64 30), (Pt 78 50), (Pt 98 40))
FillOutlined $g $collarR $white 6
$tie = Polygon @((Pt 56 46), (Pt 72 46), (Pt 84 100), (Pt 64 120), (Pt 44 100))
FillOutlined $g $tie $blue 6
foreach ($sy in @(62, 80, 98)) {
    $g.DrawLine((New-Pen $blueLight 5), 52, ($sy + 6), 76, ($sy - 6))
}
$knot = Polygon @((Pt 52 26), (Pt 76 26), (Pt 72 48), (Pt 56 48))
FillOutlined $g $knot $blue 6
Save $bmp $g "ChairmanIcon.png"

# ---- Chairman "Order" button (100): a clipboard with a stopwatch ----
$c = New-Canvas 100 100; $bmp = $c[0]; $g = $c[1]
$board = RoundedRect 10 14 50 70 6
FillOutlined $g $board $goldDark 6
$paper = RoundedRect 16 22 38 56 3
$g.FillPath((Brush $white), $paper)
$clip = RoundedRect 24 8 22 12 4
FillOutlined $g $clip (Rgb 150 152 160) 4
foreach ($ly in @(34, 46, 58)) {
    $g.DrawLine((New-Pen (Rgb 120 124 136) 4), 22, $ly, 46, $ly)
}
$g.DrawEllipse((New-Pen $outline 12), 50, 46, 40, 40)
$g.FillEllipse((Brush $white), 50, 46, 40, 40)
$g.DrawEllipse((New-Pen $blue 5), 52, 48, 36, 36)
$g.DrawLine((New-Pen $outline 5), 70, 66, 70, 54)
$g.DrawLine((New-Pen $outline 5), 70, 66, 79, 70)
$g.FillRectangle((Brush $outline), 65, 38, 10, 8)
Save $bmp $g "ChairmanOrderButton.png"

# ---- Chairman "Board Meeting" button (100): a desk bell ----
$c = New-Canvas 100 100; $bmp = $c[0]; $g = $c[1]
$base = RoundedRect 10 72 80 14 5
FillOutlined $g $base (Rgb 120 80 50) 6
$dome = New-Object System.Drawing.Drawing2D.GraphicsPath
$dome.AddArc(18, 30, 64, 84, 180, 180)
$dome.CloseFigure()
FillOutlined $g $dome $gold 6
$g.FillEllipse((Brush ([System.Drawing.Color]::FromArgb(160, 255, 255, 255))), 32, 42, 12, 16)
$knob = RoundedRect 44 20 12 14 4
FillOutlined $g $knob $goldDark 5
foreach ($l in @(@(30, 22, 22, 14), @(70, 22, 78, 14), @(50, 10, 50, 4))) {
    $g.DrawLine((New-Pen $outline 7), $l[0], $l[1], $l[2], $l[3])
    $g.DrawLine((New-Pen $gold 3), $l[0], $l[1], $l[2], $l[3])
}
Save $bmp $g "ChairmanMeetingButton.png"
