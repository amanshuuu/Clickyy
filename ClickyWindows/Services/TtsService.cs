using System.Diagnostics;
using System.Speech.Synthesis;
using ClickyWindows.Models;

namespace ClickyWindows.Services;

/// <summary>
/// Text-to-speech service. Supports multiple backends:
/// 1. Windows SAPI (built-in, FREE, no API key needed, works offline)
/// 2. ElevenLabs (high quality, requires API key + voice ID)
///
/// Equivalent to macOS ElevenLabsTTSClient.
/// </summary>
public sealed class TtsService : IDisposable
{
    private readonly AppSettings _settings;
    private SpeechSynthesizer? _sapiSynth;
    private readonly HttpClient _httpClient;
    private IWavePlayer? _outputDevice;
    private MemoryStream? _audioStream;
    private bool _disposed;

    public TtsService(AppSettings settings)
    {
        _settings = settings;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    /// <summary>
    /// Speaks the given text using the configured TTS provider.
    /// </summary>
    public async Task SpeakAsync(string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        Debug.WriteLine($"🔊 TTS: Speaking \"{text[..Math.Min(50, text.Length)]}...\"");

        try
        {
            switch (_settings.TtsProvider.ToLowerInvariant())
            {
                case "elevenlabs":
                    await ElevenLabsTtsAsync(text, ct);
                    break;
                case "windows":
                default:
                    await WindowsSapiTtsAsync(text, ct);
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"🔊 TTS error: {ex.Message}");
        }
    }

    /// <summary>
    /// Uses Windows built-in SAPI (System.Speech) for TTS.
    /// Completely free, works offline, no API key needed.
    /// Available on all Windows 10/11 machines.
    /// </summary>
    private Task WindowsSapiTtsAsync(string text, CancellationToken ct)
    {
        return Task.Run(() =>
        {
            try
            {
                _sapiSynth ??= new SpeechSynthesizer();

                // Select a natural voice if available
                if (_sapiSynth.GetInstalledVoices().Count > 0)
                {
                    // Prefer Microsoft David or Zira (English US)
                    var preferredVoices = new[] { "David", "Zira", "Mark" };
                    foreach (var name in preferredVoices)
                    {
                        var voice = _sapiSynth.GetInstalledVoices()
                            .FirstOrDefault(v => v.VoiceInfo.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
                        if (voice != null)
                        {
                            _sapiSynth.SelectVoice(voice.VoiceInfo.Name);
                            break;
                        }
                    }
                }

                _sapiSynth.Rate = 0;
                _sapiSynth.Volume = 100;

                // Speak synchronously (blocks this task)
                _sapiSynth.Speak(text);

                Debug.WriteLine("🔊 SAPI TTS completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔊 SAPI TTS error: {ex.Message}");
                throw;
            }
        }, ct);
    }

    /// <summary>
    /// Uses ElevenLabs API for high-quality TTS.
    /// Requires ELEVENLABS_API_KEY and ELEVENLABS_VOICE_ID in settings.
    /// Falls back to Windows SAPI if not configured.
    /// </summary>
    private async Task ElevenLabsTtsAsync(string text, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(_settings.ElevenLabsApiKey) ||
            string.IsNullOrEmpty(_settings.ElevenLabsVoiceId))
        {
            Debug.WriteLine("⚠️ ElevenLabs not configured, falling back to Windows SAPI TTS");
            await WindowsSapiTtsAsync(text, ct);
            return;
        }

        var voiceId = _settings.ElevenLabsVoiceId;
        var payload = new
        {
            text = text,
            model_id = "eleven_flash_v2_5",
            voice_settings = new
            {
                stability = 0.5,
                similarity_boost = 0.75,
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Remove("xi-api-key");
        _httpClient.DefaultRequestHeaders.Add("xi-api-key", _settings.ElevenLabsApiKey);

        var response = await _httpClient.PostAsync(
            $"https://api.elevenlabs.io/v1/text-to-speech/{voiceId}",
            content, ct);

        if (!response.IsSuccessStatusCode)
        {
            Debug.WriteLine($"⚠️ ElevenLabs TTS failed: {response.StatusCode}");
            await WindowsSapiTtsAsync(text, ct);
            return;
        }

        var audioData = await response.Content.ReadAsByteArrayAsync(ct);
        if (audioData == null || audioData.Length == 0)
        {
            Debug.WriteLine("🔊 TTS: No audio data from ElevenLabs");
            return;
        }

        await PlayAudioAsync(audioData, ct);
    }

    /// <summary>
    /// Plays MP3 audio data through the default audio output device.
    /// Used for ElevenLabs (returns MP3 audio).
    /// </summary>
    private Task PlayAudioAsync(byte[] audioData, CancellationToken ct)
    {
        return Task.Run(() =>
        {
            try
            {
                _outputDevice?.Stop();
                _outputDevice?.Dispose();

                _audioStream = new MemoryStream(audioData);
                var reader = new Mp3FileReader(_audioStream);
                _outputDevice = new WaveOutEvent();
                _outputDevice.Init(reader);
                _outputDevice.Play();

                while (_outputDevice.PlaybackState == PlaybackState.Playing && !ct.IsCancellationRequested)
                {
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🔊 Audio playback error: {ex.Message}");
            }
        }, ct);
    }

    /// <summary>Stops any in-progress TTS playback immediately.</summary>
    public void StopPlayback()
    {
        _outputDevice?.Stop();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _outputDevice?.Dispose();
        _audioStream?.Dispose();
        _sapiSynth?.Dispose();
        _httpClient.Dispose();
    }
}
