# Generates TownOfUs/Resources/Custom/{BleederIcon,ColorblindIcon,SleepwalkerIcon,PossumIcon,PossumButton}.png
# (AmonRa Edition: Bleeder, Colorblind, Sleepwalker, Possum)
# Usage: powershell -ExecutionPolicy Bypass -File tools/are-assets/draw-catalog-four-icons.ps1 -OutDir TownOfUs/Resources/Custom
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

# ---- Bleeder (128): a blood drop over two bloody footprints ----
$c = New-Canvas 128 128; $bmp = $c[0]; $g = $c[1]
$blood = Rgb 190 24 34
$bloodDark = Rgb 120 10 18
foreach ($fp in @(@(30, 96, -20), @(58, 108, 15))) {
    $g.ResetTransform(); $g.TranslateTransform($fp[0], $fp[1]); $g.RotateTransform($fp[2])
    $sole = New-Object System.Drawing.Drawing2D.GraphicsPath
    $sole.AddEllipse(-8, -13, 16, 22)
    FillOutlined $g $sole $bloodDark 4
    $heel = New-Object System.Drawing.Drawing2D.GraphicsPath
    $heel.AddEllipse(-6, 10, 12, 9)
    FillOutlined $g $heel $bloodDark 4
}
$g.ResetTransform()
$drop = New-Object System.Drawing.Drawing2D.GraphicsPath
$drop.AddBezier((Pt 82 8), (Pt 70 34), (Pt 52 50), (Pt 52 72))
$drop.AddArc(52, 42, 60, 60, 180, -180)
$drop.AddBezier((Pt 112 72), (Pt 112 50), (Pt 94 34), (Pt 82 8))
$drop.CloseFigure()
$g.DrawPath((New-Pen $outline 7), $drop)
$grad = New-Object System.Drawing.Drawing2D.LinearGradientBrush (Pt 52 0), (Pt 112 0), $blood, $bloodDark
$g.FillPath($grad, $drop)
$g.FillEllipse((Brush ([System.Drawing.Color]::FromArgb(170, 255, 200, 200))), 62, 58, 12, 20)
Save $bmp $g "BleederIcon.png"

# ---- Colorblind (128): an eye whose iris is half colors, half grey ----
$c = New-Canvas 128 128; $bmp = $c[0]; $g = $c[1]
$eye = New-Object System.Drawing.Drawing2D.GraphicsPath
$eye.AddBezier((Pt 8 64), (Pt 36 22), (Pt 92 22), (Pt 120 64))
$eye.AddBezier((Pt 120 64), (Pt 92 106), (Pt 36 106), (Pt 8 64))
$eye.CloseFigure()
FillOutlined $g $eye $white 7
$g.SetClip($eye)
$iris = @(36, 28, 56, 72)
$slices = @((Rgb 220 50 50), (Rgb 240 200 40), (Rgb 60 180 80), (Rgb 50 110 220))
for ($i = 0; $i -lt 4; $i++) {
    $g.FillPie((Brush $slices[$i]), 36, 36, 56, 56, (90 + 45 * $i), 45)
}
$g.FillPie((Brush (Rgb 150 152 158)), 36, 36, 56, 56, 270, 180)
$g.DrawEllipse((New-Pen $outline 4), 36, 36, 56, 56)
$g.FillEllipse((Brush $outline), 52, 52, 24, 24)
$g.FillEllipse((Brush $white), 56, 54, 8, 8)
$g.ResetClip()
$g.DrawPath((New-Pen $outline 7), $eye)
Save $bmp $g "ColorblindIcon.png"

# ---- Sleepwalker (128): a crescent moon and "Z z z" ----
$c = New-Canvas 128 128; $bmp = $c[0]; $g = $c[1]
$moonColor = Rgb 236 214 120
# Crescent = circle A minus circle B, built as one path (outer arc of A, inner arc of B) so the outline has clean tips.
$ax = 49; $ay = 69; $ar = 39; $bx = 72; $by = 54; $br = 36
$dx = $bx - $ax; $dy = $by - $ay; $d = [Math]::Sqrt($dx * $dx + $dy * $dy)
$along = ($ar * $ar - $br * $br + $d * $d) / (2 * $d); $h = [Math]::Sqrt($ar * $ar - $along * $along)
$mx = $ax + $along * $dx / $d; $my = $ay + $along * $dy / $d
$p1x = $mx - $h * $dy / $d; $p1y = $my + $h * $dx / $d
$p2x = $mx + $h * $dy / $d; $p2y = $my - $h * $dx / $d
function Ang($cx, $cy, $px, $py) { return [Math]::Atan2($py - $cy, $px - $cx) * 180 / [Math]::PI }
function AngDiff($x, $y) { $v = ($x - $y) % 360; if ($v -lt -180) { $v += 360 }; if ($v -gt 180) { $v -= 360 }; return [Math]::Abs($v) }
function ArcSweep($from, $to, $side) {
    $sweep = ($to - $from) % 360; if ($sweep -lt 0) { $sweep += 360 }
    if ((AngDiff ($from + $sweep / 2) $side) -gt 90) { $sweep -= 360 }
    return $sweep
}
$a1 = Ang $ax $ay $p1x $p1y; $a2 = Ang $ax $ay $p2x $p2y
$b1 = Ang $bx $by $p1x $p1y; $b2 = Ang $bx $by $p2x $p2y
$moon = New-Object System.Drawing.Drawing2D.GraphicsPath
$moon.AddArc(($ax - $ar), ($ay - $ar), (2 * $ar), (2 * $ar), $a1, (ArcSweep $a1 $a2 (Ang $ax $ay ($ax - $dx) ($ay - $dy))))
$moon.AddArc(($bx - $br), ($by - $br), (2 * $br), (2 * $br), $b2, (ArcSweep $b2 $b1 (Ang $bx $by $ax $ay)))
$moon.CloseFigure()
FillOutlined $g $moon $moonColor 6
function DrawZ($g, $x, $y, $s, $color) {
    $pts = [System.Drawing.PointF[]]@((Pt $x $y), (Pt ($x + $s) $y), (Pt $x ($y + $s)), (Pt ($x + $s) ($y + $s)))
    $g.DrawLines((New-Pen $outline ($s * 0.42 + 4)), $pts)
    $g.DrawLines((New-Pen $color ($s * 0.42)), $pts)
}
$zColor = Rgb 150 140 235
DrawZ $g 64 6 24 $zColor
DrawZ $g 93 37 16 $zColor
DrawZ $g 110 61 11 $zColor
Save $bmp $g "SleepwalkerIcon.png"

# ---- Possum: a red crewmate lying on its back, crossed-out eyes on the visor ----
$red = Rgb 200 32 36
$redDark = Rgb 128 14 22
$visor = Rgb 150 205 232
function DrawLyingCrewmate($g, $cx, $cy, $scale, $width) {
    $g.ResetTransform()
    $g.TranslateTransform($cx, $cy)
    $g.ScaleTransform($scale, $scale)
    $g.RotateTransform(-90) # standing crewmate faces right; -90 puts the visor up and the legs to the right
    $pack = RoundedRect (-40) (-12) 18 40 7
    FillOutlined $g $pack $redDark $width
    foreach ($leg in @(@(-24, 20), @(8, 20))) {
        $l = RoundedRect $leg[0] $leg[1] 20 26 8
        FillOutlined $g $l $redDark $width
    }
    $body = RoundedRect (-28) (-44) 58 76 28
    FillOutlined $g $body $red $width
    $v = RoundedRect 2 (-32) 38 24 12
    FillOutlined $g $v $visor $width
    $xPen = New-Pen $outline ($width * 0.75)
    foreach ($eyeX in @(12, 28)) {
        $g.DrawLine($xPen, ($eyeX - 4), -25, ($eyeX + 4), -15)
        $g.DrawLine($xPen, ($eyeX + 4), -25, ($eyeX - 4), -15)
    }
    $g.ResetTransform()
}
$c = New-Canvas 128 128; $bmp = $c[0]; $g = $c[1]
DrawLyingCrewmate $g 62 72 1.0 6
Save $bmp $g "PossumIcon.png"

$c = New-Canvas 100 100; $bmp = $c[0]; $g = $c[1]
DrawLyingCrewmate $g 48 56 0.8 6
Save $bmp $g "PossumButton.png"
