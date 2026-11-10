using UnityEngine;

namespace TownOfUs.Are;

/// <summary>
///     Sounds of the AmonRa Edition, loaded from embedded PCM WAV files (TownOfUs/Resources/Custom) at runtime,
///     so no asset bundle is needed. Supported: uncompressed 16-bit PCM, mono or stereo, any sample rate.
/// </summary>
public static class AreAudio
{
    private static AudioClip? _wheelchairRoll;
    private static bool _wheelchairRollFailed;

    /// <summary>
    ///     Seamless loop of a rolling wheelchair (placeholder from tools/are-assets/make-rolling-sound.ps1).
    /// </summary>
    public static AudioClip? WheelchairRoll
    {
        get
        {
            if (_wheelchairRoll == null && !_wheelchairRollFailed)
            {
                _wheelchairRoll = LoadWav("TownOfUs.Resources.Custom.WheelchairRoll.wav", "AreWheelchairRoll");
                _wheelchairRollFailed = _wheelchairRoll == null;
            }

            return _wheelchairRoll;
        }
    }

    private static AudioClip? LoadWav(string resource, string clipName)
    {
        try
        {
            using var stream = typeof(TownOfUsPlugin).Assembly.GetManifestResourceStream(resource);
            if (stream == null)
            {
                Warning($"AreAudio: resource {resource} not found.");
                return null;
            }

            using var reader = new BinaryReader(stream);
            if (new string(reader.ReadChars(4)) != "RIFF")
            {
                throw new InvalidDataException("not a RIFF file");
            }

            reader.ReadInt32();
            if (new string(reader.ReadChars(4)) != "WAVE")
            {
                throw new InvalidDataException("not a WAVE file");
            }

            short channels = 0;
            short bits = 0;
            var rate = 0;
            byte[]? data = null;
            while (stream.Position <= stream.Length - 8)
            {
                var id = new string(reader.ReadChars(4));
                var size = reader.ReadInt32();
                if (id == "fmt ")
                {
                    var format = reader.ReadInt16();
                    channels = reader.ReadInt16();
                    rate = reader.ReadInt32();
                    reader.ReadInt32(); // byte rate
                    reader.ReadInt16(); // block align
                    bits = reader.ReadInt16();
                    if (size > 16)
                    {
                        reader.ReadBytes(size - 16);
                    }

                    if (format != 1)
                    {
                        throw new InvalidDataException($"unsupported format {format} (only PCM)");
                    }
                }
                else if (id == "data")
                {
                    data = reader.ReadBytes(size);
                    break;
                }
                else
                {
                    reader.ReadBytes(size + (size & 1));
                }
            }

            if (data == null || bits != 16 || channels < 1 || rate <= 0)
            {
                throw new InvalidDataException($"unsupported wav (bits={bits}, channels={channels}, rate={rate})");
            }

            var samples = new float[data.Length / 2];
            for (var i = 0; i < samples.Length; i++)
            {
                samples[i] = BitConverter.ToInt16(data, i * 2) / 32768f;
            }

            var clip = AudioClip.Create(clipName, samples.Length / channels, channels, rate, false);
            clip.SetData(samples, 0);
            clip.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            return clip;
        }
        catch (Exception e)
        {
            Error($"AreAudio: could not load {resource}: {e.Message}");
            return null;
        }
    }
}
