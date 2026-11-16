# Generates TownOfUs/Resources/Custom/{FabulistIcon,FabulistAlibiButton}.png (AmonRa Edition: Fabulist)
# Usage: powershell -ExecutionPolicy Bypass -File tools/are-assets/draw-fabulist-icons.ps1 -OutDir TownOfUs/Resources/Custom
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
function Argb($a, $r, $g, $b) { return [System.Drawing.Color]::FromArgb($a, $r, $g, $b) }
function RoundedRect($x, $y, $w, $h, $r) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $p.AddArc($x, $y, 2 * $r, 2 * $r, 180, 90)
    $p.AddArc($x + $w - 2 * $r, $y, 2 * $r, 2 * $r, 270, 90)
    $p.AddArc($x + $w - 2 * $r, $y + $h - 2 * $r, 2 * $r, 2 * $r, 0, 90)
    $p.AddArc($x, $y + $h - 2 * $r, 2 * $r, 2 * $r, 90, 90)
    $p.CloseFigure()
    return $p
}

# A crewmate seen from the side, facing right: body with legs, backpack and visor. ($x, $y) = top left, $s = scale.
function Crewmate($g, $x, $y, $s, $fill, $visor, $pen) {
    $pack = RoundedRect ($x) ($y + 22 * $s) (14 * $s) (34 * $s) (6 * $s)
    $body = RoundedRect ($x + 10 * $s) ($y) (44 * $s) (66 * $s) (20 * $s)
    $legL = RoundedRect ($x + 12 * $s) ($y + 52 * $s) (16 * $s) (26 * $s) (6 * $s)
    $legR = RoundedRect ($x + 36 * $s) ($y + 52 * $s) (16 * $s) (26 * $s) (6 * $s)
    foreach ($part in @($pack, $legL, $legR, $body)) {
        $g.FillPath((Brush $fill), $part)
        $g.DrawPath($pen, $part)
    }
    $glass = RoundedRect ($x + 28 * $s) ($y + 12 * $s) (32 * $s) (20 * $s) (10 * $s)
    $g.FillPath((Brush $visor), $glass)
    $g.DrawPath($pen, $glass)
}

$red = Rgb 200 40 45
$visor = Rgb 150 200 225
$solidPen = New-Pen $outline 6
$dashPen = New-Pen (Argb 230 245 245 250) 5
$dashPen.DashStyle = [System.Drawing.Drawing2D.DashStyle]::Dash

# ---- Fabulist (128): a crewmate in front of its see-through copy ----
$c = New-Canvas 128 128; $bmp = $c[0]; $g = $c[1]
Crewmate $g 52 6 1.0 (Argb 110 200 40 45) (Argb 110 150 200 225) $dashPen
Crewmate $g 6 34 1.0 $red $visor $solidPen
Save $bmp $g "FabulistIcon.png"

# ---- "Alibi" button (100): a dashed copy at a console ----
$c = New-Canvas 100 100; $bmp = $c[0]; $g = $c[1]
$console = RoundedRect 62 44 32 46 4
$g.FillPath((Brush (Rgb 90 94 110)), $console)
$g.DrawPath((New-Pen $outline 5), $console)
$g.FillRectangle((Brush (Rgb 120 220 140)), 68, 50, 20, 14)
foreach ($ly in @(72, 80)) { $g.DrawLine((New-Pen (Rgb 200 205 215) 3), 68, $ly, 88, $ly) }
Crewmate $g 4 10 1.0 (Argb 120 200 40 45) (Argb 120 150 200 225) $dashPen
Save $bmp $g "FabulistAlibiButton.png"
