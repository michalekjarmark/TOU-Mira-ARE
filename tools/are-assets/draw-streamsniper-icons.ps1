# Generates TownOfUs/Resources/Custom/{StreamSniperIcon,StreamSniperWatchButton,StreamSniperMarkButton}.png
# (AmonRa Edition: Stream Sniper)
# Usage: powershell -ExecutionPolicy Bypass -File tools/are-assets/draw-streamsniper-icons.ps1 -OutDir TownOfUs/Resources/Custom
param([string]$OutDir)
Add-Type -AssemblyName System.Drawing
New-Item -ItemType Directory -Force $OutDir | Out-Null

$outline = [System.Drawing.Color]::FromArgb(255, 30, 28, 36)

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
# A crosshair: ring plus four ticks, centred on ($cx, $cy).
function Crosshair($g, $cx, $cy, $r, $color, $width) {
    $g.DrawEllipse((New-Pen $outline ($width + 6)), ($cx - $r), ($cy - $r), (2 * $r), (2 * $r))
    $g.DrawEllipse((New-Pen $color $width), ($cx - $r), ($cy - $r), (2 * $r), (2 * $r))
    foreach ($d in @(@(0, -1), @(0, 1), @(-1, 0), @(1, 0))) {
        $x1 = $cx + $d[0] * ($r * 0.45); $y1 = $cy + $d[1] * ($r * 0.45)
        $x2 = $cx + $d[0] * ($r * 1.3); $y2 = $cy + $d[1] * ($r * 1.3)
        $g.DrawLine((New-Pen $outline ($width + 6)), $x1, $y1, $x2, $y2)
        $g.DrawLine((New-Pen $color $width), $x1, $y1, $x2, $y2)
    }
    $g.FillEllipse((Brush $outline), ($cx - $width), ($cy - $width), (2 * $width), (2 * $width))
}

$red = Rgb 230 45 45
$purple = Rgb 145 70 255
$screen = Rgb 70 74 90

# ---- Stream Sniper (128): a stream screen with a red crosshair over it ----
$c = New-Canvas 128 128; $bmp = $c[0]; $g = $c[1]
$monitor = RoundedRect 8 20 100 70 8
FillOutlined $g $monitor $screen 6
$g.FillRectangle((Brush $purple), 16, 28, 84, 54)
$g.FillPolygon((Brush ([System.Drawing.Color]::FromArgb(230, 255, 255, 255))), [System.Drawing.PointF[]]@(
    (New-Object System.Drawing.PointF 48, 40), (New-Object System.Drawing.PointF 48, 70), (New-Object System.Drawing.PointF 72, 55)))
$g.DrawLine((New-Pen $outline 12), 58, 90, 58, 104)
$foot = RoundedRect 34 100 48 10 4
FillOutlined $g $foot $screen 5
Crosshair $g 94 86 22 $red 6
Save $bmp $g "StreamSniperIcon.png"

# ---- "Watch" button (100): an eye with a crosshair for a pupil ----
$c = New-Canvas 100 100; $bmp = $c[0]; $g = $c[1]
$eye = New-Object System.Drawing.Drawing2D.GraphicsPath
$eye.AddBezier(6, 50, 28, 14, 72, 14, 94, 50)
$eye.AddBezier(94, 50, 72, 86, 28, 86, 6, 50)
$eye.CloseFigure()
FillOutlined $g $eye (Rgb 245 245 250) 7
$g.FillEllipse((Brush $purple), 30, 30, 40, 40)
$g.DrawEllipse((New-Pen $outline 4), 30, 30, 40, 40)
$g.DrawEllipse((New-Pen $red 4), 39, 39, 22, 22)
$g.DrawLine((New-Pen $red 4), 50, 32, 50, 68)
$g.DrawLine((New-Pen $red 4), 32, 50, 68, 50)
Save $bmp $g "StreamSniperWatchButton.png"

# ---- "Mark" button (100): a crosshair ----
$c = New-Canvas 100 100; $bmp = $c[0]; $g = $c[1]
Crosshair $g 50 50 26 $red 8
Save $bmp $g "StreamSniperMarkButton.png"
