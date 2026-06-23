using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using ClickyWindows.Core;
using ClickyWindows.Models;
using ClickyWindows.Services;

namespace ClickyWindows.UI.Panel;

public partial class CompanionPanel : Window
{
    private readonly CompanionManager? _companionManager;
    private readonly HotkeyManager? _hotkeyManager;
    private readonly AppSettings _settings;
    private readonly SettingsService _settingsService;
    private bool _isUpdatingUi;

    public CompanionPanel(CompanionManager? companionManager = null,
                          AppSettings? settings = null,
                          SettingsService? settingsService = null,
                          HotkeyManager? hotkeyManager = null)
    {
        _companionManager = companionManager;
        _hotkeyManager = hotkeyManager;
        _settings = settings ?? new AppSettings();
        _settingsService = settingsService ?? new SettingsService();
        InitializeComponent();

        ModelPicker.ItemsSource = new[]
        {
            "anthropic/claude-sonnet-4-6",
            "anthropic/claude-3.5-sonnet",
            "openai/gpt-4o",
            "google/gemini-2.5-pro",
            "mistral/mistral-large",
            "openai/o3-mini",
            "deepseek/deepseek-chat",
            "google/gemini-2.0-flash-lite",
            "meta-llama/llama-3.3-70b-instruct",
        };

        LoadSettingsIntoUi();
        RefreshInteractionHistory();
    }

    private void LoadSettingsIntoUi()
    {
        _isUpdatingUi = true;

        // API Keys
        OpenRouterApiKeyInput.Text = _settings.OpenRouterApiKey;
        ElevenLabsApiKeyInput.Text = _settings.ElevenLabsApiKey;
        ElevenLabsVoiceIdInput.Text = _settings.ElevenLabsVoiceId;

        // Model
        var models = ModelPicker.ItemsSource as string[];
        if (models != null)
        {
            int modelIndex = Array.IndexOf(models, _settings.SelectedModel);
            ModelPicker.SelectedIndex = modelIndex >= 0 ? modelIndex : 0;
        }

        // Hotkey
        if (_settings.UseCtrlAlt) HotkeyCtrlAlt.IsChecked = true;
        else if (_settings.UseCtrlShift) HotkeyCtrlShift.IsChecked = true;
        else if (_settings.UseAltShift) HotkeyAltShift.IsChecked = true;
        else if (_settings.UseCtrlAltShift) HotkeyCtrlAltShift.IsChecked = true;
        else HotkeyCtrlAlt.IsChecked = true;

        // TTS
        if (_settings.TtsProvider == "elevenlabs")
        {
            TtsProviderPicker.SelectedIndex = 1;
            ElevenLabsPanel.Visibility = Visibility.Visible;
        }
        else
        {
            TtsProviderPicker.SelectedIndex = 0;
            ElevenLabsPanel.Visibility = Visibility.Collapsed;
        }

        // Behavior
        AudioFeedbackCheck.IsChecked = _settings.AudioFeedbackEnabled;
        TextOnlyModeCheck.IsChecked = _settings.TextOnlyMode;
        LaunchAtStartupCheck.IsChecked = _settings.LaunchAtStartup;

        _isUpdatingUi = false;
    }

    private void RefreshInteractionHistory()
    {
        if (_settings.RecentInteractions != null && _settings.RecentInteractions.Count > 0)
        {
            RecentInteractionsList.ItemsSource = _settings.RecentInteractions
                .OrderByDescending(i => i.Timestamp).Take(10).ToList();
        }
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => Hide();

    private void OnModelChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isUpdatingUi) return;
        if (ModelPicker.SelectedItem is string model)
            _settings.SelectedModel = model;
    }

    private void OnHotkeyChanged(object sender, RoutedEventArgs e)
    {
        if (_isUpdatingUi) return;
        _settings.UseCtrlAlt = HotkeyCtrlAlt.IsChecked == true;
        _settings.UseCtrlShift = HotkeyCtrlShift.IsChecked == true;
        _settings.UseAltShift = HotkeyAltShift.IsChecked == true;
        _settings.UseCtrlAltShift = HotkeyCtrlAltShift.IsChecked == true;

        // Apply immediately to HotkeyManager
        _hotkeyManager?.SetHotkeyCombo(
            _settings.UseCtrlAlt || _settings.UseCtrlAltShift || _settings.UseCtrlShift,
            _settings.UseCtrlAlt || _settings.UseCtrlAltShift || _settings.UseAltShift,
            _settings.UseCtrlShift || _settings.UseCtrlAltShift || _settings.UseAltShift);
    }

    private void OnTtsProviderChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isUpdatingUi) return;
        _settings.TtsProvider = TtsProviderPicker.SelectedIndex == 1 ? "elevenlabs" : "windows";
        ElevenLabsPanel.Visibility = TtsProviderPicker.SelectedIndex == 1
            ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnOpenRouterApiKeyChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdatingUi) return;
        _settings.OpenRouterApiKey = OpenRouterApiKeyInput.Text;
    }

    private void OnElevenLabsApiKeyChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdatingUi) return;
        _settings.ElevenLabsApiKey = ElevenLabsApiKeyInput.Text;
    }

    private void OnElevenLabsVoiceIdChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdatingUi) return;
        _settings.ElevenLabsVoiceId = ElevenLabsVoiceIdInput.Text;
    }

    private void OnAudioFeedbackChanged(object sender, RoutedEventArgs e)
    {
        if (_isUpdatingUi) return;
        _settings.AudioFeedbackEnabled = AudioFeedbackCheck.IsChecked == true;
    }

    private void OnTextOnlyModeChanged(object sender, RoutedEventArgs e)
    {
        if (_isUpdatingUi) return;
        _settings.TextOnlyMode = TextOnlyModeCheck.IsChecked == true;
    }

    private void OnLaunchAtStartupChanged(object sender, RoutedEventArgs e)
    {
        if (_isUpdatingUi) return;
        _settings.LaunchAtStartup = LaunchAtStartupCheck.IsChecked == true;
    }

    private async void OnSaveSettings(object sender, RoutedEventArgs e)
    {
        try
        {
            // Apply registry startup setting
            ApplyStartupRegistry(_settings.LaunchAtStartup);

            await _settingsService.SaveAsync(_settings);
            Debug.WriteLine("✅ Settings saved");

            // Visual feedback
            SaveButton.Content = new TextBlock
            {
                Text = "✓ Saved",
                FontSize = 12,
                Foreground = System.Windows.Media.Brushes.LightGreen,
            };

            await Task.Delay(1500);
            SaveButton.Content = new TextBlock
            {
                Text = "Save",
                FontSize = 12,
                Foreground = System.Windows.Media.Brushes.White,
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Failed to save settings: {ex.Message}");
            MessageBox.Show("Failed to save settings. Please try again.", "Clicky Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static void ApplyStartupRegistry(bool enable)
    {
        try
        {
            const string key = @"Software\Microsoft\Windows\CurrentVersion\Run";
            const string valueName = "Clicky";

            using var regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(key, true);
            if (regKey == null) return;

            if (enable)
            {
                var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                if (!string.IsNullOrEmpty(exePath))
                    regKey.SetValue(valueName, $"\"{exePath}\"");
            }
            else
            {
                if (regKey.GetValue(valueName) != null)
                    regKey.DeleteValue(valueName);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"⚠️ Startup registry error: {ex.Message}");
        }
    }

    private void OnClearHistory(object sender, RoutedEventArgs e)
    {
        _settings.RecentInteractions?.Clear();
        RefreshInteractionHistory();
    }

    private void OnQuitClick(object sender, RoutedEventArgs e)
    {
        System.Windows.Application.Current.Shutdown();
    }

    private void OnWatchOnboarding(object sender, RoutedEventArgs e)
    {
        _companionManager?.PerformOnboardingDemoAsync();
    }
}
