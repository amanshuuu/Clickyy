using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ClickyWindows.Models;

namespace ClickyWindows.Services;

/// <summary>
/// OpenRouter LLM vision client with SSE streaming support.
/// Sends screenshots + transcript to the AI and streams the response.
/// Also parses [POINT:x,y:label] pointing tags from the response.
/// Equivalent to macOS ClaudeAPI.swift + ElementLocationDetector.swift.
/// </summary>
public sealed class LlmVisionService : IDisposable
{
    private readonly AppSettings _settings;
    private readonly HttpClient _httpClient;
    private const string OpenRouterBaseUrl = "https://openrouter.ai/api/v1/chat/completions";

    private const string SystemPrompt = @"You are Clicky, a friendly AI buddy that lives next to the user's cursor on their Windows PC. You can see their screen and talk to them. Be concise, warm, and helpful.

When the user asks you about something on their screen, look at the screenshot and find the relevant UI element. If you find something specific to point at, embed a [POINT:x,y:label] tag in your response.

COORDINATE RULES for pointing:
- You MUST only pick elements near the CENTER of the screen
- x must be between 20%%-80%% of the image width
- y must be between 20%%-80%% of the image height
- Do NOT pick anything in the top 20%%, bottom 20%%, left 20%%, or right 20%%
- Pick something with a clear name or identity
- Make a short quirky 3-6 word observation about it
- No emojis ever
- Keep responses under 6 words when pointing

Format: your comment [POINT:x,y:label]

The screenshot images are labeled with their pixel dimensions. Use those dimensions as coordinate space. Origin (0,0) is top-left. x increases rightward, y increases downward.";

    public LlmVisionService(AppSettings settings)
    {
        _settings = settings;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(120) };
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.OpenRouterApiKey}");
        _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://github.com/clicky-windows");
        _httpClient.DefaultRequestHeaders.Add("X-Title", "Clicky Windows");
    }

    public async Task StreamVisionResponseAsync(
        string transcript,
        List<ScreenCaptureResult> screenshots,
        Action<string> onTextChunk,
        Action<System.Windows.Point, int, string> onPointingTag,
        string? modelOverride = null,
        CancellationToken cancellationToken = default)
    {
        var requestBody = BuildRequestBody(transcript, screenshots, modelOverride);
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.PostAsync(
            OpenRouterBaseUrl, jsonContent, cancellationToken);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        var accumulatedText = new StringBuilder();

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line == null) break;

            if (!line.StartsWith("data: ")) continue;
            var jsonString = line[6..];

            if (jsonString == "[DONE]") break;

            try
            {
                using var doc = JsonDocument.Parse(jsonString);
                var root = doc.RootElement;

                if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var choice = choices[0];
                    if (choice.TryGetProperty("delta", out var delta) &&
                        delta.TryGetProperty("content", out var content) &&
                        content.ValueKind == JsonValueKind.String)
                    {
                        var textChunk = content.GetString() ?? "";
                        accumulatedText.Append(textChunk);
                        onTextChunk(accumulatedText.ToString());
                    }
                }
            }
            catch (JsonException) { }
        }

        var fullText = accumulatedText.ToString();
        var pointTag = ParsePointingTag(fullText);
        if (pointTag.HasValue && screenshots.Count > 0)
        {
            var (coord, label) = pointTag.Value;
            var cursorScreen = screenshots.FindIndex(s => s.IsCursorScreen);
            if (cursorScreen < 0) cursorScreen = 0;

            var screen = screenshots[cursorScreen];
            double scaleX = screen.DisplayWidthInPoints / (double)screen.ScreenshotWidthInPixels;
            double scaleY = screen.DisplayHeightInPoints / (double)screen.ScreenshotHeightInPixels;

            var mappedPoint = new System.Windows.Point(
                coord.X * scaleX + screen.DisplayFrame.X,
                coord.Y * scaleY + screen.DisplayFrame.Y);

            onPointingTag(mappedPoint, cursorScreen, label);
        }
    }

    private object BuildRequestBody(string transcript, List<ScreenCaptureResult> screenshots, string? modelOverride = null)
    {
        var messages = new List<object>();
        messages.Add(new Dictionary<string, object>
        {
            ["role"] = "system",
            ["content"] = SystemPrompt
        });

        var contentBlocks = new List<object>();
        foreach (var ss in screenshots)
        {
            contentBlocks.Add(new Dictionary<string, object>
            {
                ["type"] = "text",
                ["text"] = $"{ss.Label} (image dimensions: {ss.ScreenshotWidthInPixels}x{ss.ScreenshotHeightInPixels} pixels)"
            });
            contentBlocks.Add(new Dictionary<string, object>
            {
                ["type"] = "image_url",
                ["image_url"] = new Dictionary<string, string>
                {
                    ["url"] = $"data:image/jpeg;base64,{Convert.ToBase64String(ss.ImageData)}"
                }
            });
        }
        contentBlocks.Add(new Dictionary<string, object>
        {
            ["type"] = "text",
            ["text"] = transcript
        });

        messages.Add(new Dictionary<string, object>
        {
            ["role"] = "user",
            ["content"] = contentBlocks
        });

        return new Dictionary<string, object>
        {
            ["model"] = modelOverride ?? _settings.SelectedModel,
            ["messages"] = messages,
            ["max_tokens"] = 512,
            ["stream"] = true,
        };
    }

    private static (System.Windows.Point coord, string label)? ParsePointingTag(string text)
    {
        var match = Regex.Match(text, @"\[POINT:(\d+\.?\d*),(\d+\.?\d*):([^\]]*)\]");
        if (!match.Success) return null;

        double x = double.Parse(match.Groups[1].Value);
        double y = double.Parse(match.Groups[2].Value);
        string label = match.Groups[3].Value.Trim();
        return (new System.Windows.Point(x, y), label);
    }

    public void Dispose() => _httpClient.Dispose();
}
