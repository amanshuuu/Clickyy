using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using ClickyWindows.Animations;
using ClickyWindows.Models;
using ClickyWindows.Services;
using ClickyWindows.WindowsAPI;

namespace ClickyWindows.Core;

public sealed class CompanionManager : IDisposable
{
    private readonly HotkeyManager _hotkeyManager;
    private readonly OverlayManager _overlayManager;
    private readonly ScreenCaptureService? _screenCaptureService;
    private readonly TranscriptionService? _transcriptionService;
    private readonly LlmVisionService? _llmVisionService;
    private readonly TtsService? _ttsService;
    private readonly SystemTrayManager _trayManager;
    private readonly AudioFeedbackService? _audioFeedbackService;
    private readonly AppSettings? _settings;
    private readonly SettingsService? _settingsService;
    private readonly FlightAnimation? _flightAnimation;
    private CancellationTokenSource? _interactionCts;
    private DispatcherTimer? _cursorPollTimer;
    private DispatcherTimer? _interactionTimeout;

    private const int InteractionTimeoutSeconds = 60;
    private const int CursorPollIntervalMs = 50;

    public VoiceState VoiceState { get; private set; } = VoiceState.Idle;
    public event EventHandler<VoiceState>? VoiceStateChanged;
    public float AudioPowerLevel { get; private set; }
    public event EventHandler<float>? AudioPowerLevelChanged;
    public string StreamingResponseText { get; private set; } = string.Empty;
    public event EventHandler<string>? StreamingResponseTextChanged;
    public Point? DetectedElementLocation { get; set; }
    public int? DetectedElementScreenIndex { get; set; }
    public string? DetectedElementBubbleText { get; set; }
    public event EventHandler<(Point location, int screenIndex)>? ElementDetected;

    public CompanionManager(
        HotkeyManager hotkeyManager, OverlayManager overlayManager,
        ScreenCaptureService? screenCaptureService, TranscriptionService? transcriptionService,
        LlmVisionService? llmVisionService, TtsService? ttsService,
        SystemTrayManager trayManager, AudioFeedbackService? audioFeedbackService = null,
        AppSettings? settings = null, SettingsService? settingsService = null)
    {
        _hotkeyManager = hotkeyManager;
        _overlayManager = overlayManager;
        _screenCaptureService = screenCaptureService;
        _transcriptionService = transcriptionService;
        _llmVisionService = llmVisionService;
        _ttsService = ttsService;
        _trayManager = trayManager;
        _audioFeedbackService = audioFeedbackService;
        _settings = settings;
        _settingsService = settingsService;

        _hotkeyManager.ShortcutPressed += OnShortcutPressed;
        _hotkeyManager.ShortcutReleased += OnShortcutReleased;

        // Wire up flight animation for pointing
        _flightAnimation = new FlightAnimation(
            onPositionUpdate: pos => _overlayManager.UpdateCursorPosition(pos),
            onComplete: () =>
            {
                Debug.WriteLine("Flight animation complete");
                _ = Task.Run(async () =>
                {
                    await Task.Delay(2000);
                    if (VoiceState == VoiceState.Pointing)
                    {
                        SetVoiceState(VoiceState.Idle);
                        _overlayManager.FadeOutAndHide(0.4);
                    }
                });
            });

        ElementDetected += OnElementDetected;
    }

    private void OnElementDetected(object? sender, (Point location, int screenIndex) e)
    {
        SetVoiceState(VoiceState.Pointing);
        var currentCursorPos = GetCursorPosition();
        _flightAnimation?.Start(currentCursorPos, e.location);
    }

    private static Point GetCursorPosition()
    {
        NativeMethods.GetCursorPos(out var point);
        return new Point(point.x, point.y);
    }

    private void StartCursorPolling()
    {
        StopCursorPolling();
        _cursorPollTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(CursorPollIntervalMs)
        };
        _cursorPollTimer.Tick += (_, _) =>
        {
            var pos = GetCursorPosition();
            _overlayManager.UpdateCursorPosition(pos);
        };
        _cursorPollTimer.Start();
    }

    private void StopCursorPolling()
    {
        _cursorPollTimer?.Stop();
        _cursorPollTimer = null;
    }

    private void StartInteractionTimeout()
    {
        StopInteractionTimeout();
        _interactionTimeout = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(InteractionTimeoutSeconds)
        };
        _interactionTimeout.Tick += (_, _) =>
        {
            Debug.WriteLine($"⏱️ Interaction timed out after {InteractionTimeoutSeconds}s");
            _trayManager.ShowNotification("Clicky", "Interaction timed out. Please try again.");
            CancelInteraction();
        };
        _interactionTimeout.Start();
    }

    private void StopInteractionTimeout()
    {
        if (_interactionTimeout != null)
        {
            _interactionTimeout.Stop();
            _interactionTimeout = null;
        }
    }

    private async void OnShortcutPressed(object? sender, EventArgs e)
    {
        Debug.WriteLine("🎤 Hotkey PRESSED");
        CancelInteraction();

        _interactionCts = new CancellationTokenSource();

        // Reset state
        StreamingResponseText = string.Empty;
        StreamingResponseTextChanged?.Invoke(this, string.Empty);
        DetectedElementLocation = null;
        DetectedElementScreenIndex = null;
        DetectedElementBubbleText = null;

        SetVoiceState(VoiceState.Listening);
        _overlayManager.ShowOverlay();
        StartCursorPolling();
        StartInteractionTimeout();

        // Audio feedback
        _audioFeedbackService?.PlayStartChime();

        if (_transcriptionService != null)
        {
            await _transcriptionService.StartRecordingAsync();
            _ = Task.Run(() => PollAudioLevels(_interactionCts.Token));
        }
    }

    private async void OnShortcutReleased(object? sender, EventArgs e)
    {
        Debug.WriteLine("🎤 Hotkey RELEASED");
        var ct = _interactionCts?.Token ?? CancellationToken.None;
        string transcript = string.Empty;

        // Audio feedback
        _audioFeedbackService?.PlayStopChime();

        try
        {
            SetVoiceState(VoiceState.Processing);

            // 1. Transcription
            if (_transcriptionService != null)
            {
                transcript = await _transcriptionService.StopRecordingAndTranscribeAsync();
                Debug.WriteLine($"📝 Transcript: {transcript}");
                if (string.IsNullOrWhiteSpace(transcript))
                {
                    Debug.WriteLine("No transcript, returning to idle");
                    await ReturnToIdle();
                    return;
                }
            }

            // 2. Screen capture (unless text-only mode)
            var screenshots = new List<ScreenCaptureResult>();
            if (_settings?.TextOnlyMode != true && _screenCaptureService != null)
            {
                screenshots = await _screenCaptureService.CaptureAllScreensAsync(ct);
            }

            // 3. AI vision response
            if (_llmVisionService != null)
            {
                SetVoiceState(VoiceState.Responding);

                bool success = false;
                string[] modelsToTry = { _settings?.SelectedModel ?? "anthropic/claude-sonnet-4-6", _settings?.FallbackModel ?? "openai/gpt-4o-mini" };

                foreach (var model in modelsToTry.Distinct())
                {
                    if (ct.IsCancellationRequested) break;

                    try
                    {
                        await _llmVisionService.StreamVisionResponseAsync(
                            transcript, screenshots,
                            onTextChunk: text =>
                            {
                                StreamingResponseText = text;
                                StreamingResponseTextChanged?.Invoke(this, text);
                            },
                            onPointingTag: (coordinate, screenIndex, label) =>
                            {
                                DetectedElementLocation = coordinate;
                                DetectedElementScreenIndex = screenIndex;
                                DetectedElementBubbleText = label;
                                ElementDetected?.Invoke(this, (coordinate, screenIndex));
                            },
                            modelOverride: model,
                            cancellationToken: ct);

                        // If we got here without exception, the model worked
                        if (!string.IsNullOrEmpty(StreamingResponseText))
                        {
                            success = true;
                            if (model != _settings?.SelectedModel)
                            {
                                Debug.WriteLine($"⚠️ Primary model failed, used fallback: {model}");
                                _trayManager.ShowNotification("Clicky",
                                    $"Used fallback AI model: {model.Split('/').LastOrDefault()}");
                            }
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"❌ Model {model} failed: {ex.Message}");
                        StreamingResponseText = $"⚠️ AI error with {model.Split('/').LastOrDefault()}. Trying fallback...";
                        StreamingResponseTextChanged?.Invoke(this, StreamingResponseText);
                        continue;
                    }
                }

                if (!success && string.IsNullOrEmpty(StreamingResponseText))
                {
                    StreamingResponseText = "Sorry, I couldn't process that. Please check your API key and try again.";
                    StreamingResponseTextChanged?.Invoke(this, StreamingResponseText);
                    Debug.WriteLine("🤖 All models failed");
                }
            }

            // 4. TTS
            if (_ttsService != null && !string.IsNullOrEmpty(StreamingResponseText) && !StreamingResponseText.StartsWith("⚠️") && !StreamingResponseText.StartsWith("Sorry"))
            {
                try { await _ttsService.SpeakAsync(StreamingResponseText, ct); }
                catch (Exception ex) { Debug.WriteLine($"🔊 TTS error: {ex.Message}"); }
            }

            // 5. Store in history
            if (_settings != null && !string.IsNullOrEmpty(transcript))
            {
                _settings.RecentInteractions ??= new List<InteractionEntry>();
                _settings.RecentInteractions.Insert(0, new InteractionEntry
                {
                    Timestamp = DateTime.Now,
                    Transcript = transcript,
                    Response = StreamingResponseText,
                    WasError = StreamingResponseText.StartsWith("⚠️") || StreamingResponseText.StartsWith("Sorry"),
                });

                // Keep only last 20
                while (_settings.RecentInteractions.Count > 20)
                    _settings.RecentInteractions.RemoveAt(_settings.RecentInteractions.Count - 1);

                // Save asynchronously
                if (_settingsService != null)
                    _ = _settingsService.SaveAsync(_settings);
            }

            // 6. Auto-dismiss
            if (VoiceState != VoiceState.Pointing && !ct.IsCancellationRequested)
            {
                await Task.Delay(2000, ct);
                await ReturnToIdle();
            }
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine("Interaction cancelled");
            await ReturnToIdle();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Interaction error: {ex.Message}");
            _audioFeedbackService?.PlayErrorChime();
            _trayManager.ShowNotification("Clicky Error",
                "Something went wrong. Please check your API key and try again.");
            await ReturnToIdle();
        }
        finally
        {
            StopInteractionTimeout();
        }
    }

    private async Task ReturnToIdle()
    {
        SetVoiceState(VoiceState.Idle);
        StopCursorPolling();
        _overlayManager.FadeOutAndHide(0.3);
    }

    private void CancelInteraction()
    {
        _interactionCts?.Cancel();
        _interactionCts?.Dispose();
        _interactionCts = null;
        StopCursorPolling();
        StopInteractionTimeout();
        _overlayManager.HideOverlay();
        SetVoiceState(VoiceState.Idle);
    }

    private async Task<List<ScreenCaptureResult>> CaptureScreensAsync(CancellationToken ct)
    {
        if (_screenCaptureService == null) return new List<ScreenCaptureResult>();
        try { return await _screenCaptureService.CaptureAllScreensAsync(ct); }
        catch (Exception ex) { Debug.WriteLine($"📸 Screen capture error: {ex.Message}"); return new List<ScreenCaptureResult>(); }
    }

    private async void PollAudioLevels(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (_transcriptionService != null)
            {
                AudioPowerLevel = _transcriptionService.GetCurrentAudioLevel();
                AudioPowerLevelChanged?.Invoke(this, AudioPowerLevel);
            }
            try { await Task.Delay(50, ct); } catch { break; }
        }
    }

    private void SetVoiceState(VoiceState newState)
    {
        if (VoiceState == newState) return;
        VoiceState = newState;
        VoiceStateChanged?.Invoke(this, newState);
        Debug.WriteLine($"🔵 State: {newState}");
    }

    public async Task PerformOnboardingDemoAsync()
    {
        var ct = CancellationToken.None;
        _overlayManager.ShowOverlay();

        var screenshots = await CaptureScreensAsync(ct);
        if (screenshots.Count == 0 || _llmVisionService == null)
        {
            _overlayManager.FadeOutAndHide();
            return;
        }

        var cursorScreen = screenshots.FirstOrDefault(s => s.IsCursorScreen);
        if (cursorScreen == null)
        {
            _overlayManager.FadeOutAndHide();
            return;
        }

        StreamingResponseText = string.Empty;
        StreamingResponseTextChanged?.Invoke(this, string.Empty);
        DetectedElementLocation = null;
        DetectedElementScreenIndex = null;
        DetectedElementBubbleText = null;

        SetVoiceState(VoiceState.Responding);

        await _llmVisionService.StreamVisionResponseAsync(
            "look around my screen and find something interesting to point at",
            new List<ScreenCaptureResult> { cursorScreen },
            onTextChunk: text =>
            {
                StreamingResponseText = text;
                StreamingResponseTextChanged?.Invoke(this, text);
            },
            onPointingTag: (coordinate, screenIndex, label) =>
            {
                DetectedElementLocation = coordinate;
                DetectedElementScreenIndex = screenIndex;
                DetectedElementBubbleText = label;
                ElementDetected?.Invoke(this, (coordinate, screenIndex));
            },
            cancellationToken: ct);
    }

    public void Dispose()
    {
        CancelInteraction();
        _hotkeyManager.ShortcutPressed -= OnShortcutPressed;
        _hotkeyManager.ShortcutReleased -= OnShortcutReleased;
        ElementDetected -= OnElementDetected;
        _flightAnimation?.Stop();
        _interactionCts?.Dispose();
        _cursorPollTimer?.Stop();
        _interactionTimeout?.Stop();
    }
}
