# Generates TownOfUs/Resources/Custom/{AuditorIcon,AuditorButton}.png (Auditor role, AmonRa Edition)
# Usage: powershell -ExecutionPolicy Bypass -File tools/are-assets/draw-auditor-icons.ps1 -OutDir TownOfUs/Resources/Custom
# Accent color matches TownOfUsColors.Auditor (235, 130, 50).
param([string]$OutDir)
Add-Type -AssemblyName System.Drawing
New-Item -ItemType Directory -Force $OutDir | Out-Null

$outline = [System.Drawing.Color]::FromArgb(255, 30, 28, 36)
$accent = [System.Drawing.Color]::FromArgb(255, 235, 130, 50)
$board = [System.Drawing.Color]::FromArgb(255, 150, 105, 60)
$paper = [System.Drawing.Color]::FromArgb(255, 248, 246, 238)
$line = [System.Drawing.Color]::FromArgb(255, 150, 150, 160)
$clip = [System.Drawing.Color]::FromArgb(255, 190, 195, 205)
$glass = [System.Drawing.Color]::FromArgb(150, 170, 220, 255)
$check = [System.Drawing.Color]::FromArgb(255, 60, 170, 80)
$cross = [System.Drawing.Color]::FromArgb(255, 215, 60, 60)

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

# Clipboard with a checklist (check, cross, check). Scale s = width of the board.
function Draw-Clipboard($g, $x, $y, $s) {
    $h = $s * 1.3
    $o = [Math]::Max(3, $s * 0.06)
    $g.FillPath((New-Object System.Drawing.SolidBrush $outline), (RoundedRect ($x - $o) ($y - $o) ($s + 2 * $o) ($h + 2 * $o) ($s * 0.12)))
    $g.FillPath((New-Object System.Drawing.SolidBrush $board), (RoundedRect $x $y $s $h ($s * 0.1)))
    $px = $x + $s * 0.1; $py = $y + $s * 0.16; $pw = $s * 0.8; $ph = $h - $s * 0.26
    $g.FillRectangle((New-Object System.Drawing.SolidBrush $paper), $px, $py, $pw, $ph)
    # clip on top
    $cw = $s * 0.44; $ch = $s * 0.16
    $g.FillPath((New-Object System.Drawing.SolidBrush $outline), (RoundedRect ($x + ($s - $cw) / 2 - $o / 2) ($y - $ch / 2 - $o / 2) ($cw + $o) ($ch + $o) ($ch * 0.4)))
    $g.FillPath((New-Object System.Drawing.SolidBrush $clip), (RoundedRect ($x + ($s - $cw) / 2) ($y - $ch / 2) $cw $ch ($ch * 0.35)))
    # three rows: mark + line
    $penLine = New-Object System.Drawing.Pen $line, ([Math]::Max(2, $s * 0.05))
    $markPen = [Math]::Max(2.5, $s * 0.07)
    for ($i = 0; $i -lt 3; $i++) {
        $ry = $py + $ph * (0.2 + 0.3 * $i)
        $mx = $px + $pw * 0.12
        $m = $s * 0.09
        if ($i -eq 1) {
            $pen = New-Object System.Drawing.Pen $cross, $markPen
            $g.DrawLine($pen, $mx - $m, $ry - $m, $mx + $m, $ry + $m)
            $g.DrawLine($pen, $mx - $m, $ry + $m, $mx + $m, $ry - $m)
        } else {
            $pen = New-Object System.Drawing.Pen $check, $markPen
            $g.DrawLines($pen, [System.Drawing.PointF[]]@(
                (New-Object System.Drawing.PointF ($mx - $m), $ry),
                (New-Object System.Drawing.PointF ($mx - $m * 0.2), ($ry + $m * 0.8)),
                (New-Object System.Drawing.PointF ($mx + $m * 1.1), ($ry - $m))))
        }
        $g.DrawLine($penLine, $px + $pw * 0.3, $ry, $px + $pw * 0.88, $ry)
    }
}

# Magnifying glass with an orange rim.
function Draw-Magnifier($g, $cx, $cy, $r) {
    $o = [Math]::Max(3, $r * 0.16)
    $handle = New-Object System.Drawing.Pen $outline, ($r * 0.62)
    $handle.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $handle.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $a = [Math]::PI / 4
    $g.DrawLine($handle, $cx + [Math]::Cos($a) * $r, $cy + [Math]::Sin($a) * $r, $cx + [Math]::Cos($a) * $r * 2.05, $cy + [Math]::Sin($a) * $r * 2.05)
    $handleIn = New-Object System.Drawing.Pen $accent, ($r * 0.36)
    $handleIn.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $handleIn.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $g.DrawLine($handleIn, $cx + [Math]::Cos($a) * $r * 1.15, $cy + [Math]::Sin($a) * $r * 1.15, $cx + [Math]::Cos($a) * $r * 1.95, $cy + [Math]::Sin($a) * $r * 1.95)
    $g.FillEllipse((New-Object System.Drawing.SolidBrush $outline), $cx - $r - $o, $cy - $r - $o, 2 * ($r + $o), 2 * ($r + $o))
    $g.FillEllipse((New-Object System.Drawing.SolidBrush $accent), $cx - $r, $cy - $r, 2 * $r, 2 * $r)
    $ri = $r * 0.74
    $g.FillEllipse((New-Object System.Drawing.SolidBrush $paper), $cx - $ri, $cy - $ri, 2 * $ri, 2 * $ri)
    $g.FillEllipse((New-Object System.Drawing.SolidBrush $glass), $cx - $ri, $cy - $ri, 2 * $ri, 2 * $ri)
    $hl = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(200, 255, 255, 255)), ($r * 0.14)
    $g.DrawArc($hl, $cx - $ri * 0.7, $cy - $ri * 0.7, $ri * 1.4, $ri * 1.4, 200, 60)
}

# ---- Role icon (128x128, PPU 128 like the other ARE icons) ----
$c = New-Canvas 128 128; $bmp = $c[0]; $g = $c[1]
Draw-Clipboard $g 18 18 66
Draw-Magnifier $g 82 76 20
Save $bmp $g "AuditorIcon.png"

# ---- Ability / meeting button (100x100, PPU 100 like AreSettingsButton) ----
$c = New-Canvas 100 100; $bmp = $c[0]; $g = $c[1]
Draw-Clipboard $g 14 14 52
Draw-Magnifier $g 64 59 16
Save $bmp $g "AuditorButton.png"
