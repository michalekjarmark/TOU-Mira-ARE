# Generates TownOfUs/Resources/Custom/{ExpertIcon,ExpertButton}.png (AmonRa Edition: Expert)
# Usage: powershell -ExecutionPolicy Bypass -File tools/are-assets/draw-expert-icons.ps1 -OutDir TownOfUs/Resources/Custom
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
function Ellipse($x, $y, $w, $h) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $p.AddEllipse($x, $y, $w, $h)
    return $p
}
function FillOutlined($g, $path, $fill, $width) {
    $g.DrawPath((New-Pen $outline $width), $path)
    $g.FillPath((Brush $fill), $path)
}

$teal = Rgb 110 205 160
$tealLight = Rgb 170 235 205
$tealDark = Rgb 62 140 108
$glass = Rgb 160 220 255
$beam = [System.Drawing.Color]::FromArgb(120, 255, 240, 150)

# ---- Expert (128): a flying saucer with a light beam ----
$c = New-Canvas 128 128; $bmp = $c[0]; $g = $c[1]
$beamPath = New-Object System.Drawing.Drawing2D.GraphicsPath
$beamPath.AddPolygon([System.Drawing.PointF[]]@((Pt 50 70), (Pt 78 70), (Pt 100 122), (Pt 28 122)))
$g.FillPath((Brush $beam), $beamPath)
$dome = Ellipse 38 18 52 48
FillOutlined $g $dome $glass 6
$g.FillEllipse((Brush ([System.Drawing.Color]::FromArgb(200, 255, 255, 255))), 50, 28, 12, 10)
$saucer = Ellipse 8 46 112 34
FillOutlined $g $saucer $teal 6
$rim = Ellipse 22 50 84 14
$g.FillPath((Brush $tealLight), $rim)
foreach ($lx in @(28, 50, 72, 94)) {
    $light = Ellipse ($lx - 5) 64 10 9
    FillOutlined $g $light (Rgb 255 230 110) 3
}
Save $bmp $g "ExpertIcon.png"

# ---- Expert "Examine" button (100): a magnifying glass over a question mark ----
$c = New-Canvas 100 100; $bmp = $c[0]; $g = $c[1]
$g.DrawLine((New-Pen $outline 20), 62, 62, 88, 88)
$g.DrawLine((New-Pen $tealDark 12), 62, 62, 88, 88)
$lens = Ellipse 10 10 58 58
FillOutlined $g $lens $glass 8
$g.DrawEllipse((New-Pen $teal 6), 13, 13, 52, 52)
$font = New-Object System.Drawing.Font "Arial", 34, ([System.Drawing.FontStyle]::Bold), ([System.Drawing.GraphicsUnit]::Pixel)
$format = New-Object System.Drawing.StringFormat
$format.Alignment = [System.Drawing.StringAlignment]::Center
$format.LineAlignment = [System.Drawing.StringAlignment]::Center
$text = New-Object System.Drawing.Drawing2D.GraphicsPath
$text.AddString("?", $font.FontFamily, [int]$font.Style, $font.Size, (New-Object System.Drawing.RectangleF 10, 12, 58, 58), $format)
$g.DrawPath((New-Pen $outline 6), $text)
$g.FillPath((Brush $white), $text)
Save $bmp $g "ExpertButton.png"
