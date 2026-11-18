# Generates TownOfUs/Resources/Custom/{InformantIcon,InformantMarkButton,InformantShowButton}.png (Informant role, AmonRa Edition)
# Usage: powershell -ExecutionPolicy Bypass -File tools/are-assets/draw-informant-icons.ps1 -OutDir TownOfUs/Resources/Custom
# Accent color matches TownOfUsColors.Informant (170, 120, 220).
param([string]$OutDir)
Add-Type -AssemblyName System.Drawing
New-Item -ItemType Directory -Force $OutDir | Out-Null

$outline = [System.Drawing.Color]::FromArgb(255, 30, 28, 36)
$accent = [System.Drawing.Color]::FromArgb(255, 170, 120, 220)
$paper = [System.Drawing.Color]::FromArgb(255, 248, 246, 238)
$red = [System.Drawing.Color]::FromArgb(255, 215, 60, 60)
$green = [System.Drawing.Color]::FromArgb(255, 60, 190, 80)

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
function RoundedRect($x, $y, $w, $h, $r) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $p.AddArc($x, $y, 2 * $r, 2 * $r, 180, 90)
    $p.AddArc($x + $w - 2 * $r, $y, 2 * $r, 2 * $r, 270, 90)
    $p.AddArc($x + $w - 2 * $r, $y + $h - 2 * $r, 2 * $r, 2 * $r, 0, 90)
    $p.AddArc($x, $y + $h - 2 * $r, 2 * $r, 2 * $r, 90, 90)
    $p.CloseFigure()
    return $p
}

# Speech bubble (purple rim, light inside) with a red and a green dot. s = width of the bubble.
function Draw-Bubble($g, $x, $y, $s) {
    $h = $s * 0.7
    $o = [Math]::Max(3, $s * 0.06)
    $tail = [System.Drawing.PointF[]]@(
        (New-Object System.Drawing.PointF ($x + $s * 0.2), ($y + $h - 2)),
        (New-Object System.Drawing.PointF ($x + $s * 0.12), ($y + $h + $s * 0.24)),
        (New-Object System.Drawing.PointF ($x + $s * 0.42), ($y + $h - 2)))
    $tailOut = [System.Drawing.PointF[]]@(
        (New-Object System.Drawing.PointF ($x + $s * 0.2 - $o), ($y + $h - 2)),
        (New-Object System.Drawing.PointF ($x + $s * 0.12 - $o * 0.6), ($y + $h + $s * 0.24 + $o)),
        (New-Object System.Drawing.PointF ($x + $s * 0.42 + $o), ($y + $h - 2)))
    $g.FillPath((New-Object System.Drawing.SolidBrush $outline), (RoundedRect ($x - $o) ($y - $o) ($s + 2 * $o) ($h + 2 * $o) ($s * 0.22)))
    $g.FillPolygon((New-Object System.Drawing.SolidBrush $outline), $tailOut)
    $g.FillPath((New-Object System.Drawing.SolidBrush $accent), (RoundedRect $x $y $s $h ($s * 0.2)))
    $g.FillPolygon((New-Object System.Drawing.SolidBrush $accent), $tail)
    $i = $s * 0.08
    $g.FillPath((New-Object System.Drawing.SolidBrush $paper), (RoundedRect ($x + $i) ($y + $i) ($s - 2 * $i) ($h - 2 * $i) ($s * 0.14)))
    $r = $s * 0.13
    $cy = $y + $h / 2
    foreach ($d in @(@(0.33, $red), @(0.67, $green))) {
        $cx = $x + $s * $d[0]
        $g.FillEllipse((New-Object System.Drawing.SolidBrush $outline), $cx - $r - $o / 2, $cy - $r - $o / 2, 2 * $r + $o, 2 * $r + $o)
        $g.FillEllipse((New-Object System.Drawing.SolidBrush $d[1]), $cx - $r, $cy - $r, 2 * $r, 2 * $r)
    }
}

# Crosshair (mark a player).
function Draw-Crosshair($g, $cx, $cy, $r) {
    $o = [Math]::Max(3, $r * 0.14)
    foreach ($pass in @(@($outline, ($r * 0.22 + 2 * $o)), @($accent, ($r * 0.22)))) {
        $pen = New-Object System.Drawing.Pen $pass[0], $pass[1]
        $pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
        $pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
        $g.DrawEllipse($pen, $cx - $r * 0.72, $cy - $r * 0.72, $r * 1.44, $r * 1.44)
        $g.DrawLine($pen, $cx, $cy - $r, $cx, $cy - $r * 0.4)
        $g.DrawLine($pen, $cx, $cy + $r * 0.4, $cx, $cy + $r)
        $g.DrawLine($pen, $cx - $r, $cy, $cx - $r * 0.4, $cy)
        $g.DrawLine($pen, $cx + $r * 0.4, $cy, $cx + $r, $cy)
    }
    $d = $r * 0.16
    $g.FillEllipse((New-Object System.Drawing.SolidBrush $outline), $cx - $d - $o / 2, $cy - $d - $o / 2, 2 * $d + $o, 2 * $d + $o)
    $g.FillEllipse((New-Object System.Drawing.SolidBrush $red), $cx - $d, $cy - $d, 2 * $d, 2 * $d)
}

# ---- Role icon (128x128, PPU 128/1.44 like the other ARE icons) ----
$c = New-Canvas 128 128; $bmp = $c[0]; $g = $c[1]
Draw-Bubble $g 14 22 100
Save $bmp $g "InformantIcon.png"

# ---- "Mark" button (100x100, PPU 100) ----
$c = New-Canvas 100 100; $bmp = $c[0]; $g = $c[1]
Draw-Crosshair $g 50 50 40
Save $bmp $g "InformantMarkButton.png"

# ---- "Show" button (100x100, PPU 100) ----
$c = New-Canvas 100 100; $bmp = $c[0]; $g = $c[1]
Draw-Bubble $g 10 16 80
Save $bmp $g "InformantShowButton.png"
