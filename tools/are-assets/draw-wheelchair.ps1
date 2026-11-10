# Generates TownOfUs/Resources/Custom/{WheelchairFront,WheelchairBack,AmonRaIcon}.png
# Usage: powershell -ExecutionPolicy Bypass -File tools/are-assets/draw-wheelchair.ps1 -OutDir TownOfUs/Resources/Custom
param([string]$OutDir)
Add-Type -AssemblyName System.Drawing
New-Item -ItemType Directory -Force $OutDir | Out-Null

$outline = [System.Drawing.Color]::FromArgb(255, 16, 16, 18)
$frame   = [System.Drawing.Color]::FromArgb(255, 74, 85, 99)
$frameHi = [System.Drawing.Color]::FromArgb(255, 112, 126, 143)
$tire    = [System.Drawing.Color]::FromArgb(255, 34, 34, 36)
$rim     = [System.Drawing.Color]::FromArgb(255, 199, 206, 214)
$spoke   = [System.Drawing.Color]::FromArgb(255, 150, 158, 168)
$seat    = [System.Drawing.Color]::FromArgb(255, 40, 44, 52)

function New-Canvas($size) {
    $bmp = New-Object System.Drawing.Bitmap $size, $size, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([System.Drawing.Color]::Transparent)
    return @($bmp, $g)
}
function Pen($color, $w) {
    $p = New-Object System.Drawing.Pen $color, $w
    $p.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $p.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $p.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    return $p
}
# Thick outlined tube: black wide stroke, then colored narrower stroke, then highlight.
function Tube($g, $pts, $w, $color) {
    $arr = [System.Drawing.PointF[]]($pts | ForEach-Object { New-Object System.Drawing.PointF $_[0], $_[1] })
    $g.DrawLines((Pen $outline ($w + 8)), $arr)
    $g.DrawLines((Pen $color $w), $arr)
}
function Circle($g, $cx, $cy, $r, $fill) {
    $g.FillEllipse((New-Object System.Drawing.SolidBrush $fill), $cx - $r, $cy - $r, 2 * $r, 2 * $r)
}
function Wheel($g, $cx, $cy, $r, $s) {
    Circle $g $cx $cy ($r + 4 * $s) $outline           # outer outline
    Circle $g $cx $cy $r $tire                           # tire
    Circle $g $cx $cy ($r - 9 * $s) $outline             # inner outline of tire
    Circle $g $cx $cy ($r - 12 * $s) $rim                # rim
    Circle $g $cx $cy ($r - 17 * $s) $outline
    $g.FillEllipse((New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(0,0,0,0))), 0, 0, 0, 0)
    # spokes on a transparent-ish dark disk
    Circle $g $cx $cy ($r - 19 * $s) ([System.Drawing.Color]::FromArgb(255, 58, 62, 70))
    $sp = Pen $spoke (3 * $s)
    for ($i = 0; $i -lt 8; $i++) {
        $a = $i * [Math]::PI / 4 + 0.2
        $g.DrawLine($sp, $cx, $cy, $cx + [Math]::Cos($a) * ($r - 19 * $s), $cy + [Math]::Sin($a) * ($r - 19 * $s))
    }
    Circle $g $cx $cy (9 * $s) $outline                  # hub
    Circle $g $cx $cy (5.5 * $s) $rim
}
function Save($bmp, $g, $name) {
    $g.Dispose(); $bmp.Save((Join-Path $OutDir $name), [System.Drawing.Imaging.ImageFormat]::Png); $bmp.Dispose()
    Write-Host "wrote $name"
}

# ---- Back layer (behind the player): backrest + push handle, facing right ----
$c = New-Canvas 256; $bmp = $c[0]; $g = $c[1]
Tube $g @(@(86, 168), @(74, 70), @(46, 66)) 9 $frame
# backrest cushion
$g.FillRectangle((New-Object System.Drawing.SolidBrush $outline), 70, 84, 26, 74)
$g.FillRectangle((New-Object System.Drawing.SolidBrush $seat), 75, 89, 16, 64)
Save $bmp $g "WheelchairBack.png"

# ---- Front layer (in front of the player): seat frame, caster, footrest (the big wheel is a separate sprite) ----
# The big wheel's center in this 256px canvas is (112, 176) with radius 70 -> keep in sync with AmonRaModifier constants.
$c = New-Canvas 256; $bmp = $c[0]; $g = $c[1]
Tube $g @(@(84, 168), @(196, 168)) 10 $frame                       # seat rail
Tube $g @(@(190, 168), @(204, 214), @(228, 214)) 9 $frame          # leg tube + footrest
Tube $g @(@(112, 176), @(196, 226)) 7 $frame                       # lower brace to caster
Circle $g 200 230 17 $outline                                       # caster
Circle $g 200 230 12 $tire
Circle $g 200 230 4 $rim
Save $bmp $g "WheelchairFront.png"

# ---- Big rear wheel (rotates while moving): 160px canvas, wheel centered, radius 70 ----
$c = New-Canvas 160; $bmp = $c[0]; $g = $c[1]
Wheel $g 80 80 70 1.0
Save $bmp $g "WheelchairWheel.png"

# ---- Icon (128px): simplified full chair ----
$c = New-Canvas 128; $bmp = $c[0]; $g = $c[1]
$g.ScaleTransform(0.5, 0.5)
Tube $g @(@(80, 168), @(70, 56), @(40, 52)) 12 $frame
Tube $g @(@(80, 160), @(200, 160)) 13 $frame
Tube $g @(@(194, 160), @(208, 212), @(236, 212)) 12 $frame
Wheel $g 112 170 72 1.2
Circle $g 204 226 20 $outline
Circle $g 204 226 14 $tire
Save $bmp $g "AmonRaIcon.png"
