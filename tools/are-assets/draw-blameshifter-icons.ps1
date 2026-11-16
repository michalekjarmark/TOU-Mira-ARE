# Generates TownOfUs/Resources/Custom/{BlameShifterIcon,BlameShifterBlameButton,BlameShifterPossessButton}.png
# (AmonRa Edition: Blame Shifter)
# Usage: powershell -ExecutionPolicy Bypass -File tools/are-assets/draw-blameshifter-icons.ps1 -OutDir TownOfUs/Resources/Custom
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
# A hand pointing right: palm with a curled fist and the index finger out. ($x, $y) = top left of the palm.
function PointingHand($g, $x, $y, $s, $fill) {
    $finger = RoundedRect ($x + 26 * $s) ($y + 2 * $s) (40 * $s) (13 * $s) (6 * $s)
    FillOutlined $g $finger $fill (5 * $s)
    $palm = RoundedRect ($x) ($y) (34 * $s) (36 * $s) (10 * $s)
    FillOutlined $g $palm $fill (5 * $s)
    foreach ($k in @(14, 23)) {
        $g.DrawLine((New-Pen $outline (3 * $s)), ($x + 18 * $s), ($y + $k * $s), ($x + 32 * $s), ($y + $k * $s))
    }
    $thumb = RoundedRect ($x + 8 * $s) ($y + 28 * $s) (20 * $s) (12 * $s) (6 * $s)
    FillOutlined $g $thumb $fill (4 * $s)
}

$green = Rgb 140 180 80
$ghost = [System.Drawing.Color]::FromArgb(235, 225, 240, 205)

# ---- Blame Shifter (128): a sheet ghost pointing to the right ----
$c = New-Canvas 128 128; $bmp = $c[0]; $g = $c[1]
$body = New-Object System.Drawing.Drawing2D.GraphicsPath
$body.AddArc(8, 10, 64, 64, 180, 180)
$body.AddLine(72, 42, 72, 110)
$body.AddBezier(72, 110, 64, 100, 60, 100, 56, 110)
$body.AddBezier(56, 110, 50, 100, 46, 100, 40, 110)
$body.AddBezier(40, 110, 34, 100, 30, 100, 24, 110)
$body.AddBezier(24, 110, 18, 100, 12, 100, 8, 110)
$body.AddLine(8, 110, 8, 42)
$body.CloseFigure()
FillOutlined $g $body $ghost 6
$g.FillEllipse((Brush $outline), 24, 34, 10, 14)
$g.FillEllipse((Brush $outline), 44, 34, 10, 14)
$g.FillEllipse((Brush $outline), 32, 58, 12, 8)
PointingHand $g 66 50 0.85 $green
Save $bmp $g "BlameShifterIcon.png"

# ---- "Blame" button (100): a pointing hand ----
$c = New-Canvas 100 100; $bmp = $c[0]; $g = $c[1]
PointingHand $g 8 26 1.25 $green
Save $bmp $g "BlameShifterBlameButton.png"

# ---- "Possess" button (100): a spiral ----
$c = New-Canvas 100 100; $bmp = $c[0]; $g = $c[1]
$g.FillEllipse((Brush (Rgb 50 55 45)), 6, 6, 88, 88)
$g.DrawEllipse((New-Pen $outline 6), 6, 6, 88, 88)
$spiral = New-Object System.Drawing.Drawing2D.GraphicsPath
$points = New-Object System.Collections.Generic.List[System.Drawing.PointF]
for ($t = 0.0; $t -le 6 * [Math]::PI; $t += 0.15) {
    $r = 3 + 2.0 * $t
    $points.Add((New-Object System.Drawing.PointF (50 + $r * [Math]::Cos($t)), (50 + $r * [Math]::Sin($t))))
}
$spiral.AddLines($points.ToArray())
$g.DrawPath((New-Pen $green 6), $spiral)
Save $bmp $g "BlameShifterPossessButton.png"
