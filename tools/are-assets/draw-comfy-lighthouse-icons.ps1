# Generates TownOfUs/Resources/Custom/{ComfyIcon,ComfyButton,LighthouseKeeperIcon,LighthouseKeeperButton,Lantern,LanternGlow}.png
# (AmonRa Edition: Comfy, Lighthouse Keeper)
# Usage: powershell -ExecutionPolicy Bypass -File tools/are-assets/draw-comfy-lighthouse-icons.ps1 -OutDir TownOfUs/Resources/Custom
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
function Polygon($points) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $p.AddPolygon([System.Drawing.PointF[]]$points)
    return $p
}
function FillOutlined($g, $path, $fill, $width) {
    $g.DrawPath((New-Pen $outline $width), $path)
    $g.FillPath((Brush $fill), $path)
}
# Soft round glow: $color in the middle, fully transparent at the edge.
function Glow($g, $cx, $cy, $r, $color) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $p.AddEllipse(($cx - $r), ($cy - $r), (2 * $r), (2 * $r))
    $brush = New-Object System.Drawing.Drawing2D.PathGradientBrush $p
    $brush.CenterColor = $color
    $brush.SurroundColors = [System.Drawing.Color[]]@([System.Drawing.Color]::FromArgb(0, $color.R, $color.G, $color.B))
    $g.FillPath($brush, $p)
}

# ---- Comfy (128): a peach armchair with a red anger mark ----
$peach = Rgb 240 150 130
$peachLight = Rgb 250 190 168
$peachDark = Rgb 190 104 88
$angry = Rgb 222 40 40
function DrawAngerMark($g, $cx, $cy, $d, $r, $width) {
    # Four curved veins around a centre, each the arc of a small circle facing the centre.
    foreach ($q in @(@(-1, -1), @(1, -1), @(1, 1), @(-1, 1))) {
        $ox = $cx + $q[0] * $d; $oy = $cy + $q[1] * $d
        $start = [Math]::Atan2(-$q[1], -$q[0]) * 180 / [Math]::PI - 55
        $g.DrawArc((New-Pen $outline ($width + 4)), ($ox - $r), ($oy - $r), (2 * $r), (2 * $r), $start, 110)
        $g.DrawArc((New-Pen $angry $width), ($ox - $r), ($oy - $r), (2 * $r), (2 * $r), $start, 110)
    }
}
$c = New-Canvas 128 128; $bmp = $c[0]; $g = $c[1]
foreach ($legX in @(26, 94)) {
    $leg = RoundedRect $legX 100 9 16 3
    FillOutlined $g $leg $peachDark 5
}
$back = RoundedRect 24 22 80 62 20
FillOutlined $g $back $peach 6
$g.DrawLine((New-Pen $peachDark 4), 64, 32, 64, 64)
$front = RoundedRect 22 80 84 24 9
FillOutlined $g $front $peachDark 6
$seat = RoundedRect 30 66 68 22 10
FillOutlined $g $seat $peachLight 6
foreach ($armX in @(10, 92)) {
    $arm = RoundedRect $armX 52 26 50 12
    FillOutlined $g $arm $peach 6
}
DrawAngerMark $g 102 24 10 12 7
Save $bmp $g "ComfyIcon.png"

# ---- Comfy "Offload Task" button (100): a task sheet pushed away with an arrow ----
$c = New-Canvas 100 100; $bmp = $c[0]; $g = $c[1]
$g.TranslateTransform(36, 50); $g.RotateTransform(-10)
$sheet = RoundedRect (-24) (-34) 48 66 6
FillOutlined $g $sheet $white 6
$clip = RoundedRect (-10) (-40) 20 10 4
FillOutlined $g $clip (Rgb 150 152 160) 4
foreach ($ly in @(-16, -4, 8, 20)) {
    $g.DrawLine((New-Pen (Rgb 120 124 136) 4), -14, $ly, 14, $ly)
}
$g.ResetTransform()
$arrow = Polygon @((Pt 52 46), (Pt 74 46), (Pt 74 34), (Pt 95 56), (Pt 74 78), (Pt 74 66), (Pt 52 66))
FillOutlined $g $arrow $peach 6
Save $bmp $g "ComfyButton.png"

# ---- Lighthouse Keeper: a lantern (local coordinates: centre x = 0, top = 0, about 50 x 86) ----
$metal = Rgb 62 66 80
$metalLight = Rgb 110 116 132
$glass = Rgb 255 222 120
$flame = Rgb 255 150 40
$flameCore = Rgb 255 236 170
function DrawLantern($g, $x, $y, $scale, $width) {
    $g.ResetTransform()
    $g.TranslateTransform($x, $y)
    $g.ScaleTransform($scale, $scale)
    $g.DrawArc((New-Pen $outline ($width + 4)), -10, 2, 20, 22, 180, 180)
    $g.DrawArc((New-Pen $metalLight $width), -10, 2, 20, 22, 180, 180)
    $cap = Polygon @((Pt (-17) 12), (Pt 17 12), (Pt 24 24), (Pt (-24) 24))
    FillOutlined $g $cap $metal $width
    $body = RoundedRect (-19) 24 38 44 7
    FillOutlined $g $body $glass $width
    $fl = New-Object System.Drawing.Drawing2D.GraphicsPath
    $fl.AddBezier((Pt 0 34), (Pt (-4) 44), (Pt (-10) 50), (Pt (-8) 56))
    $fl.AddArc(-8, 48, 16, 16, 180, -180)
    $fl.AddBezier((Pt 8 56), (Pt 10 50), (Pt 4 44), (Pt 0 34))
    $fl.CloseFigure()
    $g.FillPath((Brush $flame), $fl)
    $g.FillEllipse((Brush $flameCore), -4, 50, 8, 10)
    foreach ($bx in @(-8, 8)) {
        $g.DrawLine((New-Pen $metal ($width * 0.7)), $bx, 26, $bx, 66)
    }
    $g.DrawPath((New-Pen $outline $width), $body)
    $base = Polygon @((Pt (-24) 68), (Pt 24 68), (Pt 19 80), (Pt (-19) 80))
    FillOutlined $g $base $metal $width
    $g.ResetTransform()
}

# Icon (128): the lantern in its own warm glow.
$c = New-Canvas 128 128; $bmp = $c[0]; $g = $c[1]
Glow $g 64 66 60 ([System.Drawing.Color]::FromArgb(150, 255, 210, 110))
DrawLantern $g 64 12 1.3 5
Save $bmp $g "LighthouseKeeperIcon.png"

# Button (100): the lantern and a plus.
$c = New-Canvas 100 100; $bmp = $c[0]; $g = $c[1]
Glow $g 44 52 44 ([System.Drawing.Color]::FromArgb(120, 255, 210, 110))
DrawLantern $g 44 8 1.05 5
$plus = Polygon @((Pt 76 58), (Pt 86 58), (Pt 86 68), (Pt 96 68), (Pt 96 78), (Pt 86 78), (Pt 86 88), (Pt 76 88), (Pt 76 78), (Pt 66 78), (Pt 66 68), (Pt 76 68))
FillOutlined $g $plus (Rgb 255 200 90) 5
Save $bmp $g "LighthouseKeeperButton.png"

# Lantern on the map (64 x 96, 0.32 x 0.48 units at PPU 200): no glow, the game draws that.
$c = New-Canvas 64 96; $bmp = $c[0]; $g = $c[1]
DrawLantern $g 32 4 1.1 5
Save $bmp $g "Lantern.png"

# Glow under a lantern (256, white, tinted and faded in code).
$c = New-Canvas 256 256; $bmp = $c[0]; $g = $c[1]
Glow $g 128 128 127 ([System.Drawing.Color]::FromArgb(255, 255, 255, 255))
Save $bmp $g "LanternGlow.png"
