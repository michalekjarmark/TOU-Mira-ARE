# Generates TownOfUs/Resources/Custom/{AreSettingsButton,AreUpdateIcon}.png
# Usage: powershell -ExecutionPolicy Bypass -File tools/are-assets/draw-ui-icons.ps1 -OutDir TownOfUs/Resources/Custom
param([string]$OutDir)
Add-Type -AssemblyName System.Drawing
New-Item -ItemType Directory -Force $OutDir | Out-Null

$outline = [System.Drawing.Color]::FromArgb(255, 38, 34, 46)

function New-Canvas($w, $h) {
    $bmp = New-Object System.Drawing.Bitmap $w, $h, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([System.Drawing.Color]::Transparent)
    return @($bmp, $g)
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
function Gear($cx, $cy, $rOuter, $rInner, $teeth) {
    $pts = New-Object System.Collections.Generic.List[System.Drawing.PointF]
    $steps = $teeth * 4
    for ($i = 0; $i -lt $steps; $i++) {
        $r = if (($i % 4) -lt 2) { $rOuter } else { $rInner }
        $a = ($i + 0.5) * 2 * [Math]::PI / $steps
        $pts.Add((New-Object System.Drawing.PointF ($cx + [Math]::Cos($a) * $r), ($cy + [Math]::Sin($a) * $r)))
    }
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $p.AddPolygon($pts.ToArray())
    return $p
}
function Save($bmp, $g, $name) {
    $g.Dispose(); $bmp.Save((Join-Path $OutDir $name), [System.Drawing.Imaging.ImageFormat]::Png); $bmp.Dispose()
    Write-Host "wrote $name"
}

# ---- Freeplay "Settings" ability button (100x100, PPU 100): cyan tile like the TOU freeplay buttons, white gear ----
$c = New-Canvas 100 100; $bmp = $c[0]; $g = $c[1]
$tile = RoundedRect 4 4 92 92 16
$g.FillPath((New-Object System.Drawing.SolidBrush $outline), $tile)
$inner = RoundedRect 10 10 80 80 11
$g.FillPath((New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 52, 160, 182))), $inner)
$hi = RoundedRect 14 14 72 36 9
$g.FillPath((New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(70, 255, 255, 255))), $hi)
$gear = Gear 50 50 33 25 8
$g.DrawPath((New-Object System.Drawing.Pen $outline, 6), $gear)
$g.FillPath((New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::White)), $gear)
$g.FillEllipse((New-Object System.Drawing.SolidBrush $outline), 37, 37, 26, 26)
$g.FillEllipse((New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 52, 160, 182))), 41, 41, 18, 18)
Save $bmp $g "AreSettingsButton.png"

# ---- Main menu "Update" icon (130x130, like TouAssets.SourceCode): download arrow into a tray ----
$c = New-Canvas 130 130; $bmp = $c[0]; $g = $c[1]
$fill = [System.Drawing.Color]::FromArgb(255, 186, 220, 240)
$arrow = New-Object System.Drawing.Drawing2D.GraphicsPath
$arrow.AddPolygon([System.Drawing.PointF[]]@(
    (New-Object System.Drawing.PointF 52, 16), (New-Object System.Drawing.PointF 78, 16),
    (New-Object System.Drawing.PointF 78, 56), (New-Object System.Drawing.PointF 98, 56),
    (New-Object System.Drawing.PointF 65, 90), (New-Object System.Drawing.PointF 32, 56),
    (New-Object System.Drawing.PointF 52, 56)))
$tray = New-Object System.Drawing.Drawing2D.GraphicsPath
$tray.AddPolygon([System.Drawing.PointF[]]@(
    (New-Object System.Drawing.PointF 14, 82), (New-Object System.Drawing.PointF 28, 82),
    (New-Object System.Drawing.PointF 28, 100), (New-Object System.Drawing.PointF 102, 100),
    (New-Object System.Drawing.PointF 102, 82), (New-Object System.Drawing.PointF 116, 82),
    (New-Object System.Drawing.PointF 116, 114), (New-Object System.Drawing.PointF 14, 114)))
$pen = New-Object System.Drawing.Pen $outline, 9
$pen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
foreach ($shape in $tray, $arrow) {
    $g.DrawPath($pen, $shape)
    $g.FillPath((New-Object System.Drawing.SolidBrush $fill), $shape)
}
Save $bmp $g "AreUpdateIcon.png"
