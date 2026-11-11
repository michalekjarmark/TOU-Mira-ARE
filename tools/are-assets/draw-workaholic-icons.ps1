# Generates TownOfUs/Resources/Custom/{WorkaholicIcon,WorkaholicButton}.png (Workaholic role, AmonRa Edition)
# Usage: powershell -ExecutionPolicy Bypass -File tools/are-assets/draw-workaholic-icons.ps1 -OutDir TownOfUs/Resources/Custom
# Accent color matches TownOfUsColors.Workaholic (176, 118, 76): coffee.
param([string]$OutDir)
Add-Type -AssemblyName System.Drawing
New-Item -ItemType Directory -Force $OutDir | Out-Null

$outline = [System.Drawing.Color]::FromArgb(255, 30, 28, 36)
$accent = [System.Drawing.Color]::FromArgb(255, 176, 118, 76)
$accentDark = [System.Drawing.Color]::FromArgb(255, 120, 74, 42)
$coffee = [System.Drawing.Color]::FromArgb(255, 70, 40, 22)
$mug = [System.Drawing.Color]::FromArgb(255, 244, 240, 232)
$paper = [System.Drawing.Color]::FromArgb(255, 248, 246, 238)
$paperBack = [System.Drawing.Color]::FromArgb(255, 214, 210, 200)
$line = [System.Drawing.Color]::FromArgb(255, 150, 150, 160)
$check = [System.Drawing.Color]::FromArgb(255, 60, 170, 80)
$steam = [System.Drawing.Color]::FromArgb(210, 235, 235, 240)

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

# A sheet with a checklist (all rows checked). $s = width of the sheet.
function Draw-Sheet($g, $x, $y, $s, $fill, $rows, $checked) {
    $h = $s * 1.3
    $o = [Math]::Max(3, $s * 0.06)
    $g.FillPath((New-Object System.Drawing.SolidBrush $outline), (RoundedRect ($x - $o) ($y - $o) ($s + 2 * $o) ($h + 2 * $o) ($s * 0.1)))
    $g.FillPath((New-Object System.Drawing.SolidBrush $fill), (RoundedRect $x $y $s $h ($s * 0.08)))
    $penLine = New-Object System.Drawing.Pen $line, ([Math]::Max(2, $s * 0.05))
    $markPen = New-Object System.Drawing.Pen $check, ([Math]::Max(2.5, $s * 0.07))
    for ($i = 0; $i -lt $rows; $i++) {
        $ry = $y + $h * (0.2 + 0.6 * $i / [Math]::Max(1, $rows - 1))
        $mx = $x + $s * 0.18
        $m = $s * 0.08
        if ($checked) {
            $g.DrawLines($markPen, [System.Drawing.PointF[]]@(
                (New-Object System.Drawing.PointF ($mx - $m), $ry),
                (New-Object System.Drawing.PointF ($mx - $m * 0.2), ($ry + $m * 0.8)),
                (New-Object System.Drawing.PointF ($mx + $m * 1.1), ($ry - $m))))
        }
        $g.DrawLine($penLine, $x + $s * 0.36, $ry, $x + $s * 0.86, $ry)
    }
}

# Coffee mug with steam. $w = width of the cup body.
function Draw-Mug($g, $x, $y, $w) {
    $h = $w * 1.0
    $o = [Math]::Max(3, $w * 0.08)
    # handle (behind the body, on the right)
    $hp = New-Object System.Drawing.Pen $outline, ($w * 0.26)
    $g.DrawArc($hp, $x + $w * 0.62, $y + $h * 0.22, $w * 0.62, $h * 0.5, -90, 180)
    $hp2 = New-Object System.Drawing.Pen $accent, ($w * 0.12)
    $g.DrawArc($hp2, $x + $w * 0.62, $y + $h * 0.22, $w * 0.62, $h * 0.5, -90, 180)
    # body
    $g.FillPath((New-Object System.Drawing.SolidBrush $outline), (RoundedRect ($x - $o) ($y - $o) ($w + 2 * $o) ($h + 2 * $o) ($w * 0.2)))
    $g.FillPath((New-Object System.Drawing.SolidBrush $accent), (RoundedRect $x $y $w $h ($w * 0.16)))
    # band and coffee at the top
    $g.FillRectangle((New-Object System.Drawing.SolidBrush $accentDark), $x, $y + $h * 0.58, $w, $h * 0.12)
    $g.FillEllipse((New-Object System.Drawing.SolidBrush $coffee), $x + $w * 0.08, $y - $h * 0.06, $w * 0.84, $h * 0.2)
    # highlight
    $hl = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(150, 255, 255, 255)), ($w * 0.08)
    $hl.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $hl.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $g.DrawLine($hl, $x + $w * 0.18, $y + $h * 0.22, $x + $w * 0.18, $y + $h * 0.48)
    # steam
    $sp = New-Object System.Drawing.Pen $steam, ($w * 0.09)
    $sp.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $sp.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    foreach ($dx in @(0.3, 0.62)) {
        $sx = $x + $w * $dx
        $g.DrawBezier($sp, $sx, $y - $h * 0.12, $sx - $w * 0.12, $y - $h * 0.3, $sx + $w * 0.12, $y - $h * 0.42, $sx, $y - $h * 0.6)
    }
}

# Round badge with a plus.
function Draw-Plus($g, $cx, $cy, $r) {
    $o = [Math]::Max(3, $r * 0.18)
    $g.FillEllipse((New-Object System.Drawing.SolidBrush $outline), $cx - $r - $o, $cy - $r - $o, 2 * ($r + $o), 2 * ($r + $o))
    $g.FillEllipse((New-Object System.Drawing.SolidBrush $accent), $cx - $r, $cy - $r, 2 * $r, 2 * $r)
    $pp = New-Object System.Drawing.Pen $paper, ($r * 0.36)
    $pp.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $pp.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $g.DrawLine($pp, $cx - $r * 0.52, $cy, $cx + $r * 0.52, $cy)
    $g.DrawLine($pp, $cx, $cy - $r * 0.52, $cx, $cy + $r * 0.52)
}

# ---- Role icon (128x128, loaded with AreAssets.RoleIconPpu like the other ARE role icons) ----
$c = New-Canvas 128 128; $bmp = $c[0]; $g = $c[1]
Draw-Sheet $g 30 10 54 $paperBack 3 $false
Draw-Sheet $g 20 18 56 $paper 3 $true
Draw-Mug $g 58 66 40
Save $bmp $g "WorkaholicIcon.png"

# ---- "Take Task" button (100x100, PPU 100 like the Auditor's button) ----
$c = New-Canvas 100 100; $bmp = $c[0]; $g = $c[1]
Draw-Sheet $g 16 10 52 $paper 3 $false
Draw-Plus $g 70 72 19
Save $bmp $g "WorkaholicButton.png"
