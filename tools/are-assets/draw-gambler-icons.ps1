# Generates TownOfUs/Resources/Custom/{GamblerIcon,GamblerBetButton}.png (Gambler role, AmonRa Edition)
# Usage: powershell -ExecutionPolicy Bypass -File tools/are-assets/draw-gambler-icons.ps1 -OutDir TownOfUs/Resources/Custom
# The chip color matches TownOfUsColors.Gambler (0, 150, 136).
param([string]$OutDir)
Add-Type -AssemblyName System.Drawing
New-Item -ItemType Directory -Force $OutDir | Out-Null

$outline = [System.Drawing.Color]::FromArgb(255, 30, 28, 36)
$chipColor = [System.Drawing.Color]::FromArgb(255, 0, 150, 136)
$chipDark = [System.Drawing.Color]::FromArgb(255, 0, 105, 96)
$white = [System.Drawing.Color]::FromArgb(255, 245, 245, 240)

function New-Canvas($w, $h) {
    $bmp = New-Object System.Drawing.Bitmap $w, $h, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
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

# Poker chip: outlined disc, 8 white edge spots, white inner ring, darker center with a "$".
function Draw-Chip($g, $cx, $cy, $r) {
    $o = [Math]::Max(3, $r * 0.09)
    $g.FillEllipse((New-Object System.Drawing.SolidBrush $outline), $cx - $r - $o, $cy - $r - $o, 2 * ($r + $o), 2 * ($r + $o))
    $g.FillEllipse((New-Object System.Drawing.SolidBrush $chipColor), $cx - $r, $cy - $r, 2 * $r, 2 * $r)
    # edge spots: short arcs of the outer band
    $band = $r * 0.22
    $pen = New-Object System.Drawing.Pen $white, $band
    $rr = $r - $band / 2
    for ($i = 0; $i -lt 8; $i++) {
        $g.DrawArc($pen, $cx - $rr, $cy - $rr, 2 * $rr, 2 * $rr, $i * 45 - 11, 22)
    }
    # inner ring
    $ri = $r * 0.62
    $g.FillEllipse((New-Object System.Drawing.SolidBrush $white), $cx - $ri, $cy - $ri, 2 * $ri, 2 * $ri)
    $rc = $r * 0.52
    $g.FillEllipse((New-Object System.Drawing.SolidBrush $chipDark), $cx - $rc, $cy - $rc, 2 * $rc, 2 * $rc)
    # "$" in the center
    $font = New-Object System.Drawing.Font "Arial Black", ($r * 0.62), ([System.Drawing.FontStyle]::Bold), ([System.Drawing.GraphicsUnit]::Pixel)
    $sf = New-Object System.Drawing.StringFormat
    $sf.Alignment = [System.Drawing.StringAlignment]::Center
    $sf.LineAlignment = [System.Drawing.StringAlignment]::Center
    $rect = New-Object System.Drawing.RectangleF ($cx - $r), ($cy - $r + $r * 0.06), (2 * $r), (2 * $r)
    $g.DrawString('$', $font, (New-Object System.Drawing.SolidBrush $white), $rect, $sf)
}

# Die tilted a bit, showing five pips.
function Draw-Die($g, $cx, $cy, $s, $angle) {
    $state = $g.Save()
    $g.TranslateTransform($cx, $cy)
    $g.RotateTransform($angle)
    $o = [Math]::Max(3, $s * 0.08)
    $outer = RoundedRect (-$s / 2 - $o) (-$s / 2 - $o) ($s + 2 * $o) ($s + 2 * $o) ($s * 0.24)
    $g.FillPath((New-Object System.Drawing.SolidBrush $outline), $outer)
    $body = RoundedRect (-$s / 2) (-$s / 2) $s $s ($s * 0.2)
    $g.FillPath((New-Object System.Drawing.SolidBrush $white), $body)
    $pip = $s * 0.18
    $d = $s * 0.27
    $brush = New-Object System.Drawing.SolidBrush $outline
    foreach ($p in @(@(-$d, -$d), @($d, -$d), @(0, 0), @(-$d, $d), @($d, $d))) {
        $g.FillEllipse($brush, $p[0] - $pip / 2, $p[1] - $pip / 2, $pip, $pip)
    }
    $g.Restore($state)
}

# ---- Role icon (128x128, PPU 128 like AmonRaIcon): chip with a die in front ----
$c = New-Canvas 128 128; $bmp = $c[0]; $g = $c[1]
Draw-Chip $g 56 58 44
Draw-Die $g 92 92 36 14
Save $bmp $g "GamblerIcon.png"

# ---- Meeting "Bet" button (100x100): a single chip ----
$c = New-Canvas 100 100; $bmp = $c[0]; $g = $c[1]
Draw-Chip $g 50 50 44
Save $bmp $g "GamblerBetButton.png"
