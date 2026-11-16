# Generates TownOfUs/Resources/Custom/{StreamerIcon,StreamerOfflineButton}.png (AmonRa Edition: Streamer)
# Usage: powershell -ExecutionPolicy Bypass -File tools/are-assets/draw-streamer-icons.ps1 -OutDir TownOfUs/Resources/Custom
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
function FillOutlined($g, $path, $fill, $width) {
    $g.DrawPath((New-Pen $outline $width), $path)
    $g.FillPath((Brush $fill), $path)
}
function Circle($g, $cx, $cy, $r, $fill, $width) {
    $g.FillEllipse((Brush $outline), ($cx - $r - $width / 2), ($cy - $r - $width / 2), (2 * $r + $width), (2 * $r + $width))
    $g.FillEllipse((Brush $fill), ($cx - $r), ($cy - $r), (2 * $r), (2 * $r))
}

$purple = Rgb 145 70 255
$purpleDark = Rgb 95 40 180
$grey = Rgb 215 218 228
$red = Rgb 235 50 50

# A webcam: round head with a lens, a neck and a foot. $s scales, ($ox, $oy) moves it.
function Webcam($g, $ox, $oy, $s) {
    $foot = Polygon @((Pt ($ox + 26 * $s) ($oy + 92 * $s)), (Pt ($ox + 74 * $s) ($oy + 92 * $s)), (Pt ($ox + 66 * $s) ($oy + 80 * $s)), (Pt ($ox + 34 * $s) ($oy + 80 * $s)))
    FillOutlined $g $foot $purpleDark (5 * $s)
    $g.DrawLine((New-Pen $outline (14 * $s)), ($ox + 50 * $s), ($oy + 66 * $s), ($ox + 50 * $s), ($oy + 82 * $s))
    $g.DrawLine((New-Pen $purpleDark (7 * $s)), ($ox + 50 * $s), ($oy + 66 * $s), ($ox + 50 * $s), ($oy + 82 * $s))
    Circle $g ($ox + 50 * $s) ($oy + 40 * $s) (32 * $s) $grey (6 * $s)
    Circle $g ($ox + 50 * $s) ($oy + 40 * $s) (19 * $s) $purple (5 * $s)
    $g.FillEllipse((Brush $outline), ($ox + 40 * $s), ($oy + 30 * $s), (20 * $s), (20 * $s))
    $g.FillEllipse((Brush ([System.Drawing.Color]::FromArgb(200, 255, 255, 255))), ($ox + 43 * $s), ($oy + 33 * $s), (7 * $s), (7 * $s))
}

# ---- Streamer (128): a webcam with a red "live" dot ----
$c = New-Canvas 128 128; $bmp = $c[0]; $g = $c[1]
Webcam $g 4 8 1.2
Circle $g 106 22 13 $red 5
Save $bmp $g "StreamerIcon.png"

# ---- "Go offline" button (100): the webcam crossed out ----
$c = New-Canvas 100 100; $bmp = $c[0]; $g = $c[1]
Webcam $g 4 4 0.92
$g.DrawLine((New-Pen $outline 16), 14, 86, 86, 14)
$g.DrawLine((New-Pen $red 9), 14, 86, 86, 14)
Save $bmp $g "StreamerOfflineButton.png"
