# Generates TownOfUs/Resources/Custom/GluttonIcon.png (AmonRa Edition: Glutton / Obżartuch role icon)
# Usage: powershell -ExecutionPolicy Bypass -File tools/are-assets/draw-glutton-icon.ps1 -OutDir TownOfUs/Resources/Custom
# A chubby crewmate with a napkin bib and a fork. Color matches TownOfUsColors.Glutton (51, 199, 74).
# The "Swallow" button (GluttonSwallowButton.png) is Project: Lotus's "pelican_swallow" sprite, not drawn here.
param([string]$OutDir)
Add-Type -AssemblyName System.Drawing
New-Item -ItemType Directory -Force $OutDir | Out-Null

function Rgb($r, $g, $b) { return [System.Drawing.Color]::FromArgb(255, $r, $g, $b) }
function Brush($color) { return New-Object System.Drawing.SolidBrush $color }
function New-Pen($color, $width) {
    $pen = New-Object System.Drawing.Pen $color, $width
    $pen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    $pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    return $pen
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

$outline = Rgb 30 28 36
$green = Rgb 51 199 74
$darkGreen = Rgb 34 150 56
$belly = Rgb 120 228 132
$visor = Rgb 150 200 225
$shine = Rgb 235 248 255
$napkin = Rgb 248 246 238
$steel = Rgb 205 210 222
$pen = New-Pen $outline 6

$bmp = New-Object System.Drawing.Bitmap 128, 128, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.Clear([System.Drawing.Color]::Transparent)

# Legs and backpack behind the body.
foreach ($part in @((RoundedRect 30 92 22 24 8), (RoundedRect 62 92 22 24 8))) {
    $g.FillPath((Brush $green), $part); $g.DrawPath($pen, $part)
}
$pack = RoundedRect 6 46 18 42 7
$g.FillPath((Brush $darkGreen), $pack); $g.DrawPath($pen, $pack)

# A wide, round body.
$body = RoundedRect 16 14 84 90 40
$g.FillPath((Brush $green), $body); $g.DrawPath($pen, $body)

# Round belly (lighter) with a little shine.
$g.FillEllipse((Brush $belly), 40, 60, 52, 38)
$g.FillEllipse((Brush (Rgb 170 240 178)), 52, 66, 16, 9)

# Napkin bib under the visor.
$bib = [System.Drawing.PointF[]]@(
    (New-Object System.Drawing.PointF 54, 56),
    (New-Object System.Drawing.PointF 92, 56),
    (New-Object System.Drawing.PointF 73, 82))
$g.FillPolygon((Brush $napkin), $bib)
$g.DrawPolygon((New-Pen $outline 4), $bib)

# Visor.
$glass = RoundedRect 54 26 46 26 13
$g.FillPath((Brush $visor), $glass); $g.DrawPath($pen, $glass)
$g.FillPath((Brush $shine), (RoundedRect 66 31 20 7 3.5))

# Fork held up at the side.
$forkOut = New-Pen $outline 10
$forkIn = New-Pen $steel 5
foreach ($p in @($forkOut, $forkIn)) {
    $g.DrawLine($p, 112, 60, 112, 112)
    $g.DrawLine($p, 104, 40, 104, 58)
    $g.DrawLine($p, 112, 40, 112, 58)
    $g.DrawLine($p, 120, 40, 120, 58)
    $g.DrawLine($p, 104, 58, 120, 58)
}

$g.Dispose()
$bmp.Save((Join-Path $OutDir "GluttonIcon.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Write-Host "wrote GluttonIcon.png"
