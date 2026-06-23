namespace ClickyWindows.Models;

/// <summary>
/// The companion's current voice interaction state.
/// Controls what the overlay cursor shows (waveform, spinner, text bubble, or idle triangle).
/// </summary>
public enum VoiceState
{
    /// <summary>No interaction — shows the default blue cursor triangle.</summary>
    Idle,

    /// <summary>User is holding the push-to-talk key — shows waveform animation.</summary>
    Listening,

    /// <summary>Audio is being transcribed or sent to the AI — shows spinner.</summary>
    Processing,

    /// <summary>AI response is streaming in — shows text bubble near cursor.</summary>
    Responding,

    /// <summary>Buddy is flying to point at a UI element.</summary>
    Pointing
}
