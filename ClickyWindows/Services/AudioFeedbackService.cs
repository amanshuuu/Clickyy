using System.Diagnostics;
using System.Runtime.InteropServices;
using NAudio.Wave;

namespace ClickyWindows.Services;

/// <summary>
/// Plays short audio chimes for recording start/stop feedback.
/// Uses Windows Beep API for simplicity (no extra dependencies).
/// Falls back to NAudio tone generation for richer sounds.
/// </summary>
public sealed class AudioFeedbackService : IDisposable
{
    private readonly bool _enabled;

    public AudioFeedbackService(bool enabled)
    {
        _enabled = enabled;
    }

    /// <summary>Short rising tone — recording started.</summary>
    public void PlayStartChime()
    {
        if (!_enabled) return;
        Task.Run(() => PlayBeep(880, 120));
    }

    /// <summary>Short falling tone — recording stopped.</summary>
    public void PlayStopChime()
    {
        if (!_enabled) return;
        Task.Run(() => PlayBeep(660, 120));
    }

    /// <summary>Error attention tone.</summary>
    public void PlayErrorChime()
    {
        if (!_enabled) return;
        Task.Run(() => PlayBeep(440, 250));
    }

    /// <summary>Success notification tone.</summary>
    public void PlaySuccessChime()
    {
        if (!_enabled) return;
        Task.Run(() => PlayBeep(1047, 150));
    }

    private static void PlayBeep(int frequencyHz, int durationMs)
    {
        try
        {
            // Use NAudio to generate a pure sine wave
            int sampleRate = 44100;
            int samples = sampleRate * durationMs / 1000;

            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            // Write WAV header
            short channels = 1;
            short bitsPerSample = 16;
            int dataSize = samples * channels * (bitsPerSample / 8);

            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + dataSize);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
            writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1); // PCM
            writer.Write(channels);
            writer.Write(sampleRate);
            writer.Write(sampleRate * channels * (bitsPerSample / 8));
            writer.Write((short)(channels * (bitsPerSample / 8)));
            writer.Write(bitsPerSample);

            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write(dataSize);

            // Generate sine wave with smooth attack/release
            double attackSamples = sampleRate * 0.005; // 5ms attack
            double releaseSamples = sampleRate * 0.010; // 10ms release

            for (int i = 0; i < samples; i++)
            {
                // Envelope
                double envelope = 1.0;
                if (i < attackSamples) envelope = i / attackSamples;
                else if (i > samples - releaseSamples) envelope = (samples - i) / releaseSamples;

                double t = (double)i / sampleRate;
                short sample = (short)(Math.Sin(2 * Math.PI * frequencyHz * t) * envelope * 0.25 * short.MaxValue);
                writer.Write(sample);
            }

            ms.Position = 0;
            using var waveOut = new WaveOutEvent { Volume = 0.5f };
            using var waveReader = new WaveFileReader(ms);
            waveOut.Init(waveReader);

            var finished = new ManualResetEvent(false);
            waveOut.PlaybackStopped += (_, _) => finished.Set();
            waveOut.Play();
            finished.WaitOne(durationMs + 200);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"🔊 Chime error: {ex.Message}");
        }
    }

    public void Dispose() { }
}
