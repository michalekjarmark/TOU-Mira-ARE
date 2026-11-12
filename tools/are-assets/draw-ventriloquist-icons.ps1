# Generates TownOfUs/Resources/Custom/{VentriloquistIcon,VentriloquistButton}.png (Ventriloquist role, AmonRa Edition)
# Usage: powershell -ExecutionPolicy Bypass -File tools/are-assets/draw-ventriloquist-icons.ps1 -OutDir TownOfUs/Resources/Custom
param([string]$OutDir)
Add-Type -AssemblyName System.Drawing
New-Item -ItemType Directory -Force $OutDir | Out-Null

$outline = [System.Drawing.Color]::FromArgb(255, 30, 28, 36)
$white = [System.Drawing.Color]::FromArgb(255, 245, 245, 250)
$shade = [System.Drawing.Color]::FromArgb(255, 200, 204, 216)
$red = [System.Drawing.Color]::FromArgb(255, 214, 40, 40)
$wood = [System.Drawing.Color]::FromArgb(255, 160, 104, 56)
$woodDark = [System.Drawing.Color]::FromArgb(255, 112, 70, 36)
$string = [System.Drawing.Color]::FromArgb(255, 225, 225, 225)
$skin = [System.Drawing.Color]::FromArgb(255, 240, 190, 150)
$skinDark = [System.Drawing.Color]::FromArgb(255, 200, 140, 105)
$wave = [System.Drawing.Color]::FromArgb(255, 255, 214, 60)

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

# Rounded speech bubble with a tail at the bottom left and three red dots. (x, y, w, h) = body.
function Draw-Bubble($g, $x, $y, $w, $h) {
    $r = $h * 0.45
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddArc($x, $y, 2 * $r, 2 * $r, 180, 90)
    $path.AddArc($x + $w - 2 * $r, $y, 2 * $r, 2 * $r, 270, 90)
    $path.AddArc($x + $w - 2 * $r, $y + $h - 2 * $r, 2 * $r, 2 * $r, 0, 90)
    $path.AddLine(($x + $w * 0.42), ($y + $h), ($x + $w * 0.16), ($y + $h * 1.38))
    $path.AddLine(($x + $w * 0.2), ($y + $h), ($x + $w * 0.2), ($y + $h))
    $path.AddArc($x, $y + $h - 2 * $r, 2 * $r, 2 * $r, 90, 90)
    $path.CloseFigure()
    $g.DrawPath((New-Pen $outline ([Math]::Max(4, $h * 0.12))), $path)
    $g.FillPath((New-Object System.Drawing.SolidBrush $white), $path)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush $shade), ($x + $r * 0.6), ($y + $h * 0.78), ($w - $r * 1.2), ($h * 0.1))
    $d = $h * 0.22
    for ($i = 0; $i -lt 3; $i++) {
        $cx = $x + $w * (0.28 + 0.22 * $i)
        $g.FillEllipse((New-Object System.Drawing.SolidBrush $red), ($cx - $d / 2), ($y + $h * 0.42 - $d / 2), $d, $d)
    }
}

# Marionette control bar: a wooden cross. (cx, cy) = center, s = bar length.
function Draw-Control($g, $cx, $cy, $s) {
    $t = $s * 0.13
    $o = [Math]::Max(2, $s * 0.04)
    $dark = New-Object System.Drawing.SolidBrush $outline
    $g.FillRectangle($dark, ($cx - $s / 2 - $o), ($cy - $t / 2 - $o), ($s + 2 * $o), ($t + 2 * $o))
    $g.FillRectangle($dark, ($cx - $t / 2 - $o), ($cy - $s * 0.16 - $o), ($t + 2 * $o), ($s * 0.46 + 2 * $o))
    $g.FillRectangle((New-Object System.Drawing.SolidBrush $wood), ($cx - $s / 2), ($cy - $t / 2), $s, $t)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush $woodDark), ($cx - $t / 2), ($cy - $s * 0.16), $t, ($s * 0.46))
}

# Ear seen from the side, opening to the left, with sound waves on the left. (x, y) = top left, s = height.
function Draw-Ear($g, $x, $y, $s) {
    $w = $s * 0.62
    $outer = New-Object System.Drawing.Drawing2D.GraphicsPath
    $outer.AddBezier(($x + $w * 0.15), ($y + $s * 0.35), ($x + $w * 0.05), ($y - $s * 0.02), ($x + $w * 1.05), ($y - $s * 0.08), ($x + $w * 1.0), ($y + $s * 0.4))
    $outer.AddBezier(($x + $w * 1.0), ($y + $s * 0.4), ($x + $w * 0.98), ($y + $s * 0.7), ($x + $w * 0.7), ($y + $s * 0.72), ($x + $w * 0.62), ($y + $s * 0.88))
    $outer.AddBezier(($x + $w * 0.62), ($y + $s * 0.88), ($x + $w * 0.55), ($y + $s * 1.02), ($x + $w * 0.28), ($y + $s * 1.0), ($x + $w * 0.3), ($y + $s * 0.82))
    $outer.AddBezier(($x + $w * 0.3), ($y + $s * 0.82), ($x + $w * 0.32), ($y + $s * 0.68), ($x + $w * 0.22), ($y + $s * 0.6), ($x + $w * 0.15), ($y + $s * 0.35))
    $outer.CloseFigure()
    $g.DrawPath((New-Pen $outline ([Math]::Max(4, $s * 0.07))), $outer)
    $g.FillPath((New-Object System.Drawing.SolidBrush $skin), $outer)
    # inner fold
    $fold = New-Pen $skinDark ([Math]::Max(3, $s * 0.07))
    $g.DrawBezier($fold, ($x + $w * 0.35), ($y + $s * 0.3), ($x + $w * 0.4), ($y + $s * 0.08), ($x + $w * 0.85), ($y + $s * 0.12), ($x + $w * 0.8), ($y + $s * 0.42))
    $g.DrawBezier($fold, ($x + $w * 0.8), ($y + $s * 0.42), ($x + $w * 0.76), ($y + $s * 0.58), ($x + $w * 0.55), ($y + $s * 0.55), ($x + $w * 0.5), ($y + $s * 0.68))
    $g.FillEllipse((New-Object System.Drawing.SolidBrush $skinDark), ($x + $w * 0.36), ($y + $s * 0.4), ($w * 0.2), ($s * 0.14))
    # sound waves
    $wavePen = New-Pen $outline ([Math]::Max(5, $s * 0.1))
    $waveCore = New-Pen $wave ([Math]::Max(3, $s * 0.055))
    for ($i = 0; $i -lt 2; $i++) {
        $r = $s * (0.22 + 0.16 * $i)
        $cx = $x - $s * 0.02
        $cy = $y + $s * 0.45
        $g.DrawArc($wavePen, ($cx - $r), ($cy - $r), (2 * $r), (2 * $r), 135, 90)
        $g.DrawArc($waveCore, ($cx - $r), ($cy - $r), (2 * $r), (2 * $r), 135, 90)
    }
}

# ---- Role icon (128x128): speech bubble on puppet strings ----
$c = New-Canvas 128 128; $bmp = $c[0]; $g = $c[1]
foreach ($pen in @((New-Pen $outline 4.5), (New-Pen $string 2.2))) {
    $g.DrawLine($pen, 26, 20, 30, 62)
    $g.DrawLine($pen, 64, 40, 64, 60)
    $g.DrawLine($pen, 102, 20, 98, 62)
}
Draw-Control $g 64 20 84
Draw-Bubble $g 16 58 96 46
Save $bmp $g "VentriloquistIcon.png"

# ---- Ability button (100x100): ear with sound waves ----
$c = New-Canvas 100 100; $bmp = $c[0]; $g = $c[1]
Draw-Ear $g 40 10 80
Save $bmp $g "VentriloquistButton.png"
