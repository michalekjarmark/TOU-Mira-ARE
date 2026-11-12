# Generates TownOfUs/Resources/Custom/{VandalIcon,VandalButton}.png (Vandal role, AmonRa Edition)
# Usage: powershell -ExecutionPolicy Bypass -File tools/are-assets/draw-vandal-icons.ps1 -OutDir TownOfUs/Resources/Custom
param([string]$OutDir)
Add-Type -AssemblyName System.Drawing
New-Item -ItemType Directory -Force $OutDir | Out-Null

$outline = [System.Drawing.Color]::FromArgb(255, 30, 28, 36)
$red = [System.Drawing.Color]::FromArgb(255, 214, 40, 40)
$redDark = [System.Drawing.Color]::FromArgb(255, 150, 20, 24)
$can = [System.Drawing.Color]::FromArgb(255, 96, 104, 120)
$canLight = [System.Drawing.Color]::FromArgb(255, 160, 170, 186)
$canDark = [System.Drawing.Color]::FromArgb(255, 64, 70, 84)
$label = [System.Drawing.Color]::FromArgb(255, 255, 214, 60)
$white = [System.Drawing.Color]::FromArgb(255, 245, 245, 250)
$green = [System.Drawing.Color]::FromArgb(255, 70, 200, 90)
$greenDark = [System.Drawing.Color]::FromArgb(255, 40, 140, 60)
$wood = [System.Drawing.Color]::FromArgb(255, 160, 104, 56)
$steel = [System.Drawing.Color]::FromArgb(255, 150, 156, 168)
$steelLight = [System.Drawing.Color]::FromArgb(255, 205, 210, 220)

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
function RoundedRect($x, $y, $w, $h, $r) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $p.AddArc($x, $y, 2 * $r, 2 * $r, 180, 90)
    $p.AddArc($x + $w - 2 * $r, $y, 2 * $r, 2 * $r, 270, 90)
    $p.AddArc($x + $w - 2 * $r, $y + $h - 2 * $r, 2 * $r, 2 * $r, 0, 90)
    $p.AddArc($x, $y + $h - 2 * $r, 2 * $r, 2 * $r, 90, 90)
    $p.CloseFigure()
    return $p
}
function Pt($x, $y) { return New-Object System.Drawing.PointF $x, $y }

# ---- Role icon (128x128): spray can that painted a dripping red cross ----
$c = New-Canvas 128 128; $bmp = $c[0]; $g = $c[1]

# red cross with drips, top right
$crossOutline = New-Pen $outline 17
$crossCore = New-Pen $red 10
foreach ($pen in @($crossOutline, $crossCore)) {
    $g.DrawLine($pen, 66, 16, 112, 62)
    $g.DrawLine($pen, 112, 16, 66, 62)
}
$drip = New-Pen $outline 8
$dripCore = New-Pen $red 4
foreach ($d in @(@(70, 60, 78), @(108, 60, 84), @(89, 41, 70))) {
    $g.DrawLine($drip, $d[0], $d[1], $d[0], $d[2])
    $g.DrawLine($dripCore, $d[0], $d[1], $d[0], $d[2])
}

# spray mist from the nozzle towards the cross
$mist = Brush ([System.Drawing.Color]::FromArgb(200, 214, 40, 40))
foreach ($m in @(@(56, 40, 5), @(63, 34, 4), @(62, 45, 4), @(69, 39, 3), @(68, 29, 3))) {
    $g.FillEllipse($mist, ($m[0] - $m[2]), ($m[1] - $m[2]), (2 * $m[2]), (2 * $m[2]))
}

# can body
$body = RoundedRect 14 52 42 70 8
$g.DrawPath((New-Pen $outline 6), $body)
$grad = New-Object System.Drawing.Drawing2D.LinearGradientBrush (Pt 14 0), (Pt 56 0), $canLight, $canDark
$g.FillPath($grad, $body)
$g.FillRectangle((Brush $label), 15, 74, 40, 20)
$g.DrawLine((New-Pen $outline 2.5), 15, 74, 55, 74)
$g.DrawLine((New-Pen $outline 2.5), 15, 94, 55, 94)
# small cross on the label
$g.DrawLine((New-Pen $redDark 4), 29, 78, 41, 90)
$g.DrawLine((New-Pen $redDark 4), 41, 78, 29, 90)
# shoulder and cap
$shoulder = New-Object System.Drawing.Drawing2D.GraphicsPath
$shoulder.AddPolygon([System.Drawing.PointF[]]@((Pt 18 54), (Pt 26 44), (Pt 44 44), (Pt 52 54)))
$g.DrawPath((New-Pen $outline 6), $shoulder)
$g.FillPath((Brush $canDark), $shoulder)
$cap = RoundedRect 27 32 16 13 3
$g.DrawPath((New-Pen $outline 5), $cap)
$g.FillPath((Brush $white), $cap)
$g.FillRectangle((Brush $outline), 42, 35, 8, 5)
Save $bmp $g "VandalIcon.png"

# ---- Ability button (100x100): mallet coming down on a green check mark that cracks ----
$c = New-Canvas 100 100; $bmp = $c[0]; $g = $c[1]

# check mark: short stroke, then the long stroke up to the right
$check = [System.Drawing.PointF[]]@((Pt 12 54), (Pt 36 78), (Pt 88 26))
$g.DrawLines((New-Pen $outline 20), $check)
$g.DrawLines((New-Pen $green 11), $check)
$shade = New-Pen $greenDark 4
$g.DrawLine($shade, 17, 61, 34, 78)
$g.DrawLine($shade, 40, 80, 86, 34)

# crack across the long stroke under the mallet: a zig-zag cut, with the piece on the right knocked a little up
$g.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
$cut = New-Object System.Drawing.Pen ([System.Drawing.Color]::Transparent), 4
$g.DrawLines($cut, [System.Drawing.PointF[]]@((Pt 44 50), (Pt 51 59), (Pt 47 63), (Pt 56 73)))
$g.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceOver
# chips flying off
foreach ($chip in @(@(64, 74, 4), @(72, 68, 3), @(60, 84, 3))) {
    $g.FillEllipse((Brush $outline), ($chip[0] - $chip[2] - 1.5), ($chip[1] - $chip[2] - 1.5), (2 * $chip[2] + 3), (2 * $chip[2] + 3))
    $g.FillEllipse((Brush $green), ($chip[0] - $chip[2]), ($chip[1] - $chip[2]), (2 * $chip[2]), (2 * $chip[2]))
}

# mallet: handle to the upper left, steel head right above the crack (face at the bottom)
$g.DrawLine((New-Pen $outline 12), 8, 22, 40, 22)
$g.DrawLine((New-Pen $wood 6), 10, 22, 40, 22)
$head = RoundedRect 38 6 22 36 4
$g.DrawPath((New-Pen $outline 5), $head)
$headGrad = New-Object System.Drawing.Drawing2D.LinearGradientBrush (Pt 38 0), (Pt 60 0), $steelLight, $steel
$g.FillPath($headGrad, $head)
$g.FillRectangle((Brush $steel), 40, 37, 18, 3)

# impact lines next to the face
$impact = New-Pen $label 3.5
$g.DrawLine($impact, 32, 42, 24, 45)
$g.DrawLine($impact, 34, 48, 27, 55)
Save $bmp $g "VandalButton.png"
