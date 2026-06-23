namespace ClickyWindows.Models;

/// <summary>
/// Persisted user settings. Stored in local JSON file so preferences
/// survive restarts (model choice, hotkey preference, API keys, etc.).
/// </summary>
public class AppSettings
{
    // ── API Keys ───────────────────────────────────────────────────
    public string OpenRouterApiKey { get; set; } = string.Empty;
    public string ElevenLabsApiKey { get; set; } = string.Empty;
    public string ElevenLabsVoiceId { get; set; } = string.Empty;

    // ── AI Model ───────────────────────────────────────────────────
    public string SelectedModel { get; set; } = "anthropic/claude-sonnet-4-6";
    public string FallbackModel { get; set; } = "openai/gpt-4o-mini";
    public string TranscriptionProvider { get; set; } = "openrouter"; // "openrouter", "whisper-local"

    // ── TTS ────────────────────────────────────────────────────────
    public string TtsProvider { get; set; } = "windows"; // "windows" (SAPI, free), "elevenlabs"

    // ── Hotkey ─────────────────────────────────────────────────────
    public bool UseCtrlAlt { get; set; } = true;    // Default: Ctrl+Alt
    public bool UseCtrlShift { get; set; } = false;
    public bool UseAltShift { get; set; } = false;
    public bool UseCtrlAltShift { get; set; } = false;

    // ── Behavior ───────────────────────────────────────────────────
    public bool ShowClickyCursor { get; set; } = true;
    public bool TextOnlyMode { get; set; } = false;  // Disable screen capture
    public bool AudioFeedbackEnabled { get; set; } = true;  // Chime on record start/stop
    public bool LaunchAtStartup { get; set; } = false;

    // ── Onboarding ─────────────────────────────────────────────────
    public bool HasCompletedOnboarding { get; set; } = false;

    // ── Interaction History (last 10) ──────────────────────────────
    public List<InteractionEntry> RecentInteractions { get; set; } = new();
}

/// <summary>
/// A single interaction entry stored in the settings history.
/// </summary>
public class InteractionEntry
{
    public DateTime Timestamp { get; set; }
    public string Transcript { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public bool WasError { get; set; }
}
