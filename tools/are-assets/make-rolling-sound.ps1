# Generates a PLACEHOLDER TownOfUs/Resources/Custom/WheelchairRoll.wav: seamless 2 s loop of a wheel rolling on a floor
# (low rumble + gentle texture + soft seam tick every 0.5 s). 22050 Hz, mono, 16-bit PCM.
# Replace the file with a real recording when available (keep: PCM 16-bit WAV, mono or stereo, seamless loop).
# Usage: powershell -ExecutionPolicy Bypass -File tools/are-assets/make-rolling-sound.ps1 -OutDir TownOfUs/Resources/Custom
param([string]$OutDir)
New-Item -ItemType Directory -Force $OutDir | Out-Null

$rate = 22050
$seconds = 2.0
$n = [int]($rate * $seconds)
$fade = [int]($rate * 0.15)
$total = $n + $fade
$rng = New-Object System.Random 1234
$raw = New-Object double[] $total

# Brown-ish noise through two one-pole low-pass filters -> rumble
$brown = 0.0; $lp1 = 0.0; $lp2 = 0.0
for ($i = 0; $i -lt $total; $i++) {
    $white = $rng.NextDouble() * 2 - 1
    $brown = ($brown * 0.985) + ($white * 0.12)
    $lp1 += 0.20 * ($brown - $lp1)
    $lp2 += 0.35 * ($white - $lp2)
    $t = $i / $rate
    $texture = 1.0 + 0.18 * [Math]::Sin(2 * [Math]::PI * 11 * $t) + 0.10 * [Math]::Sin(2 * [Math]::PI * 23 * $t)
    $raw[$i] = ($lp1 * 1.0 + $lp2 * 0.08) * $texture
}

# Soft "seam" tick every 0.5 s (divides the loop length, so the rhythm loops cleanly)
$tickEvery = [int]($rate * 0.5)
$tickLen = [int]($rate * 0.025)
for ($start = 0; $start -lt $total; $start += $tickEvery) {
    for ($k = 0; $k -lt $tickLen -and ($start + $k) -lt $total; $k++) {
        $env = [Math]::Exp(-$k / ($tickLen / 5.0))
        $raw[$start + $k] += ($rng.NextDouble() * 2 - 1) * 0.35 * $env
    }
}

# Seamless loop: crossfade the extra tail into the beginning
$out = New-Object double[] $n
for ($i = 0; $i -lt $n; $i++) { $out[$i] = $raw[$i] }
for ($i = 0; $i -lt $fade; $i++) {
    $w = $i / $fade
    $out[$i] = $raw[$i] * $w + $raw[$n + $i] * (1 - $w)
}

# Remove DC offset and normalize to 0.6 peak
$mean = ($out | Measure-Object -Average).Average
$peak = 0.0
for ($i = 0; $i -lt $n; $i++) { $out[$i] -= $mean; $a = [Math]::Abs($out[$i]); if ($a -gt $peak) { $peak = $a } }
$gain = 0.6 / $peak

$path = Join-Path $OutDir "WheelchairRoll.wav"
$fs = [System.IO.File]::Create($path)
$bw = New-Object System.IO.BinaryWriter $fs
$dataBytes = $n * 2
$bw.Write([Text.Encoding]::ASCII.GetBytes("RIFF")); $bw.Write([int](36 + $dataBytes)); $bw.Write([Text.Encoding]::ASCII.GetBytes("WAVE"))
$bw.Write([Text.Encoding]::ASCII.GetBytes("fmt ")); $bw.Write([int]16); $bw.Write([int16]1); $bw.Write([int16]1)
$bw.Write([int]$rate); $bw.Write([int]($rate * 2)); $bw.Write([int16]2); $bw.Write([int16]16)
$bw.Write([Text.Encoding]::ASCII.GetBytes("data")); $bw.Write([int]$dataBytes)
for ($i = 0; $i -lt $n; $i++) { $bw.Write([int16][Math]::Round([Math]::Max(-1, [Math]::Min(1, $out[$i] * $gain)) * 32767)) }
$bw.Close(); $fs.Close()
Write-Host "wrote WheelchairRoll.wav ($n samples, $rate Hz)"
