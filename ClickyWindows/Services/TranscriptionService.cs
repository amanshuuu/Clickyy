using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using NAudio.Wave;
using ClickyWindows.Models;

namespace ClickyWindows.Services;

/// <summary>
/// Handles push-to-talk microphone capture and transcription.
/// Uses NAudio WASAPI for low-latency audio capture, then sends
/// audio to OpenRouter's Whisper endpoint.
/// Equivalent to macOS BuddyDictationManager + AssemblyAIStreamingTranscriptionProvider.
/// </summary>
public sealed class TranscriptionService : IDisposable
{
    private readonly AppSettings _settings;
    private WaveInEvent? _waveIn;
    private MemoryStream? _audioStream;
    private float _currentLevel;

    public TranscriptionService(AppSettings settings) { _settings = settings; }

    public Task StartRecordingAsync()
    {
        _audioStream = new MemoryStream();
        _currentLevel = 0;
        var header = new byte[44];
        _audioStream.Write(header, 0, 44);

        _waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(16000, 16, 1),
            BufferMilliseconds = 100,
        };
        _waveIn.DataAvailable += OnDataAvailable;
        _waveIn.StartRecording();
        Debug.WriteLine("🎤 Recording started...");
        return Task.CompletedTask;
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        _audioStream?.Write(e.Buffer, 0, e.BytesRecorded);
        float sum = 0;
        for (int i = 0; i < e.BytesRecorded; i += 2)
        {
            short sample = (short)(e.Buffer[i] | (e.Buffer[i + 1] << 8));
            sum += sample * sample;
        }
        _currentLevel = Math.Min(1.0f, (float)Math.Sqrt(sum / (e.BytesRecorded / 2)) / 32768f * 3f);
    }

    public async Task<string> StopRecordingAndTranscribeAsync()
    {
        if (_waveIn == null) return string.Empty;
        _waveIn.StopRecording();

        byte[] audioData;
        if (_audioStream != null)
        {
            _audioStream.Position = 0;
            audioData = _audioStream.ToArray();
            using var wavStream = new MemoryStream();
            WriteWavHeader(wavStream, audioData.Length - 44, 16000);
            wavStream.Write(audioData, 44, audioData.Length - 44);
            audioData = wavStream.ToArray();
        }
        else return string.Empty;

        _waveIn.Dispose();
        _waveIn = null;

        if (audioData.Length < 5000) return string.Empty;
        Debug.WriteLine($"🎤 Recording stopped, {audioData.Length} bytes");
        return await TranscribeAudioAsync(audioData);
    }

    private async Task<string> TranscribeAudioAsync(byte[] audioData)
    {
        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.OpenRouterApiKey}");

            using var formData = new MultipartFormDataContent();
            formData.Add(new ByteArrayContent(audioData), "file", "recording.wav");
            formData.Add(new StringContent("whisper-1"), "model");

            var response = await httpClient.PostAsync(
                "https://openrouter.ai/api/v1/audio/transcriptions", formData);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"🎤 OpenRouter Whisper error: {response.StatusCode} - {error}");
                return string.Empty;
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("text").GetString()?.Trim() ?? "";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"🎤 Transcription error: {ex.Message}");
            return string.Empty;
        }
    }

    public float GetCurrentAudioLevel() => _currentLevel;

    private static void WriteWavHeader(Stream stream, int dataSize, int sampleRate)
    {
        var writer = new BinaryWriter(stream);
        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + dataSize);
        writer.Write(Encoding.ASCII.GetBytes("WAVE"));
        writer.Write(Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)1);
        writer.Write(sampleRate);
        writer.Write(sampleRate * 16 / 8);
        writer.Write((short)2);
        writer.Write((short)16);
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(dataSize);
        writer.Flush();
    }

    public void Dispose()
    {
        _waveIn?.Dispose();
        _audioStream?.Dispose();
    }
}
