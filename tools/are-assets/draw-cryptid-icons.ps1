# Generates TownOfUs/Resources/Custom/{CryptidIcon,CryptidAppearButton,CryptidFigure}.png (AmonRa Edition: Cryptid)
# Usage: powershell -ExecutionPolicy Bypass -File tools/are-assets/draw-cryptid-icons.ps1 -OutDir TownOfUs/Resources/Custom
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
# A crewmate silhouette facing right, one colour. ($x, $y) = top left, $s = scale (about 64 x 80 at 1.0).
function Silhouette($g, $x, $y, $s, $color) {
    $b = Brush $color
    $g.FillPath($b, (RoundedRect ($x) ($y + 22 * $s) (14 * $s) (34 * $s) (6 * $s)))
    $g.FillPath($b, (RoundedRect ($x + 10 * $s) ($y) (44 * $s) (66 * $s) (20 * $s)))
    $g.FillPath($b, (RoundedRect ($x + 12 * $s) ($y + 52 * $s) (16 * $s) (26 * $s) (6 * $s)))
    $g.FillPath($b, (RoundedRect ($x + 36 * $s) ($y + 52 * $s) (16 * $s) (26 * $s) (6 * $s)))
    $g.FillPath((Brush ([System.Drawing.Color]::FromArgb($color.A, 200, 205, 210))),
        (RoundedRect ($x + 28 * $s) ($y + 12 * $s) (32 * $s) (20 * $s) (10 * $s)))
}
# The blurry figure: drawn small, then scaled up smoothly, which blurs the edges.
function BlurryFigure($w, $h) {
    $small = New-Object System.Drawing.Bitmap ([int]($w / 9)), ([int]($h / 9)), ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $sg = [System.Drawing.Graphics]::FromImage($small)
    $sg.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $sg.Clear([System.Drawing.Color]::Transparent)
    $sx = $small.Width / 80.0
    Silhouette $sg ($small.Width * 0.08) ($small.Height * 0.06) $sx (Rgb 120 122 130)
    $sg.Dispose()
    $big = New-Object System.Drawing.Bitmap $w, $h, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $bg = [System.Drawing.Graphics]::FromImage($big)
    $bg.Clear([System.Drawing.Color]::Transparent)
    $bg.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $bg.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $bg.DrawImage($small, (New-Object System.Drawing.Rectangle 0, 0, $w, $h))
    $bg.Dispose(); $small.Dispose()
    return $big
}

$fur = Rgb 165 145 115
$furDark = Rgb 120 100 75

# ---- Cryptid (128): a big footprint ----
$c = New-Canvas 128 128; $bmp = $c[0]; $g = $c[1]
$sole = New-Object System.Drawing.Drawing2D.GraphicsPath
$sole.AddEllipse(30, 40, 62, 82)
$g.DrawPath((New-Pen $outline 6), $sole)
$g.FillPath((Brush $fur), $sole)
$g.FillEllipse((Brush $furDark), 46, 62, 30, 40)
foreach ($t in @(@(22, 18, 20), @(42, 6, 22), @(66, 4, 22), @(88, 14, 20), @(102, 34, 16))) {
    $g.FillEllipse((Brush $outline), ($t[0] - 3), ($t[1] - 3), ($t[2] + 6), ($t[2] + 6))
    $g.FillEllipse((Brush $fur), $t[0], $t[1], $t[2], $t[2])
}
Save $bmp $g "CryptidIcon.png"

# ---- "Appear" button (100): the blurry figure ----
$c = New-Canvas 100 100; $bmp = $c[0]; $g = $c[1]
$fig = BlurryFigure 160 200
$g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$g.DrawImage($fig, 10, 0, 80, 100)
$fig.Dispose()
Save $bmp $g "CryptidAppearButton.png"

# ---- The sighting on the map: the blurry figure, bigger ----
$fig = BlurryFigure 160 200
$fig.Save((Join-Path $OutDir "CryptidFigure.png"), [System.Drawing.Imaging.ImageFormat]::Png)
$fig.Dispose()
Write-Host "wrote CryptidFigure.png"
