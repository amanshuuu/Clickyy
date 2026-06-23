using System.IO;
using System.Diagnostics;
using System.Text.Json;
using ClickyWindows.Models;

namespace ClickyWindows.Services;

/// <summary>
/// Manages loading and saving app settings from a JSON file.
/// Stored at %APPDATA%\Clicky\settings.json
/// </summary>
public sealed class SettingsService
{
    private static readonly string SettingsPath =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Clicky", "settings.json");

    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>
    /// Loads settings from disk, or returns defaults if no file exists.
    /// </summary>
    public async Task<AppSettings> LoadAsync()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = await File.ReadAllTextAsync(SettingsPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                if (settings != null) return settings;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"⚠️ Error loading settings: {ex.Message}");
        }

        return new AppSettings();
    }

    /// <summary>
    /// Saves settings to disk.
    /// </summary>
    public async Task SaveAsync(AppSettings settings)
    {
        await _lock.WaitAsync();
        try
        {
            var dir = Path.GetDirectoryName(SettingsPath);
            if (dir != null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(SettingsPath, json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"⚠️ Error saving settings: {ex.Message}");
        }
        finally
        {
            _lock.Release();
        }
    }
}
