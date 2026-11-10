# Generates TownOfUs/Resources/Custom/{RiggerIcon,RiggerButton,RigSpark}.png (Rigger role, AmonRa Edition)
# Usage: powershell -ExecutionPolicy Bypass -File tools/are-assets/draw-rigger-icons.ps1 -OutDir TownOfUs/Resources/Custom
param([string]$OutDir)
Add-Type -AssemblyName System.Drawing
New-Item -ItemType Directory -Force $OutDir | Out-Null

$outline = [System.Drawing.Color]::FromArgb(255, 30, 28, 36)
$steel = [System.Drawing.Color]::FromArgb(255, 175, 182, 194)
$steelDark = [System.Drawing.Color]::FromArgb(255, 120, 128, 142)
$red = [System.Drawing.Color]::FromArgb(255, 214, 40, 40)
$bolt = [System.Drawing.Color]::FromArgb(255, 255, 214, 60)
$boltCore = [System.Drawing.Color]::FromArgb(255, 255, 250, 210)

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
function Points($list) {
    $pts = New-Object System.Collections.Generic.List[System.Drawing.PointF]
    for ($i = 0; $i -lt $list.Count; $i += 2) { $pts.Add((New-Object System.Drawing.PointF $list[$i], $list[$i + 1])) }
    return $pts.ToArray()
}

# Open-end wrench along a diagonal, red handle. (cx, cy) = center, s = length.
function Draw-Wrench($g, $cx, $cy, $s, $angle) {
    $state = $g.Save()
    $g.TranslateTransform($cx, $cy)
    $g.RotateTransform($angle)
    $w = $s * 0.16
    $o = [Math]::Max(3, $s * 0.045)
    # handle
    $g.FillRectangle((New-Object System.Drawing.SolidBrush $outline), -$s / 2 - $o, -$w / 2 - $o, $s * 0.75 + 2 * $o, $w + 2 * $o)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush $red), -$s / 2, -$w / 2, $s * 0.42, $w)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush $steel), -$s / 2 + $s * 0.42, -$w / 2, $s * 0.33, $w)
    # head (circle with a notch)
    $hr = $s * 0.2
    $hx = $s * 0.33
    $g.FillEllipse((New-Object System.Drawing.SolidBrush $outline), $hx - $hr - $o, -$hr - $o, 2 * ($hr + $o), 2 * ($hr + $o))
    $g.FillEllipse((New-Object System.Drawing.SolidBrush $steel), $hx - $hr, -$hr, 2 * $hr, 2 * $hr)
    $nw = $hr * 0.85
    $g.FillRectangle((New-Object System.Drawing.SolidBrush $outline), $hx - $nw * 0.1, -$nw / 2 - $o / 2, $hr + $o * 2, $nw + $o)
    $g.FillRectangle([System.Drawing.Brushes]::Transparent, 0, 0, 0, 0)
    $g.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
    $g.FillRectangle((New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::Transparent)), $hx + $o * 0.5, -$nw / 2 + $o / 2, $hr + $o * 2, $nw - $o)
    $g.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceOver
    $g.FillEllipse((New-Object System.Drawing.SolidBrush $steelDark), -$s / 2 + $s * 0.06, -$w * 0.22, $w * 0.44, $w * 0.44)
    $g.Restore($state)
}

# Lightning bolt. (x, y) = top left, s = height.
function Draw-Bolt($g, $x, $y, $s) {
    $p = Points @(
        ($x + $s * 0.38), $y,
        ($x + $s * 0.05), ($y + $s * 0.56),
        ($x + $s * 0.30), ($y + $s * 0.56),
        ($x + $s * 0.14), ($y + $s),
        ($x + $s * 0.62), ($y + $s * 0.38),
        ($x + $s * 0.37), ($y + $s * 0.38),
        ($x + $s * 0.58), $y)
    $pen = New-Object System.Drawing.Pen $outline, ([Math]::Max(3, $s * 0.08))
    $pen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    $g.DrawPolygon($pen, $p)
    $g.FillPolygon((New-Object System.Drawing.SolidBrush $bolt), $p)
}

# Four-point spark with a glow.
function Draw-Spark($g, $cx, $cy, $r) {
    $glow = New-Object System.Drawing.Drawing2D.GraphicsPath
    $glow.AddEllipse($cx - $r, $cy - $r, 2 * $r, 2 * $r)
    $brush = New-Object System.Drawing.Drawing2D.PathGradientBrush $glow
    $brush.CenterColor = [System.Drawing.Color]::FromArgb(170, 255, 230, 120)
    $brush.SurroundColors = @([System.Drawing.Color]::FromArgb(0, 255, 200, 60))
    $g.FillEllipse($brush, $cx - $r, $cy - $r, 2 * $r, 2 * $r)
    $a = $r * 0.85; $b = $r * 0.18
    $star = Points @($cx, ($cy - $a), ($cx + $b), ($cy - $b), ($cx + $a), $cy, ($cx + $b), ($cy + $b), $cx, ($cy + $a), ($cx - $b), ($cy + $b), ($cx - $a), $cy, ($cx - $b), ($cy - $b))
    $g.FillPolygon((New-Object System.Drawing.SolidBrush $bolt), $star)
    $a2 = $a * 0.55; $b2 = $b * 0.6
    $core = Points @($cx, ($cy - $a2), ($cx + $b2), ($cy - $b2), ($cx + $a2), $cy, ($cx + $b2), ($cy + $b2), $cx, ($cy + $a2), ($cx - $b2), ($cy + $b2), ($cx - $a2), $cy, ($cx - $b2), ($cy - $b2))
    $g.FillPolygon((New-Object System.Drawing.SolidBrush $boltCore), $core)
}

# ---- Role icon (128x128) ----
$c = New-Canvas 128 128; $bmp = $c[0]; $g = $c[1]
Draw-Wrench $g 58 66 96 -40
Draw-Bolt $g 70 18 54
Save $bmp $g "RiggerIcon.png"

# ---- Ability button (100x100) ----
$c = New-Canvas 100 100; $bmp = $c[0]; $g = $c[1]
Draw-Wrench $g 46 54 76 -40
Draw-Bolt $g 55 12 44
Save $bmp $g "RiggerButton.png"

# ---- Spark over a rigged console (64x64) ----
$c = New-Canvas 64 64; $bmp = $c[0]; $g = $c[1]
Draw-Spark $g 32 32 30
Save $bmp $g "RigSpark.png"
