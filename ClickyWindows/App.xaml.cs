using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using ClickyWindows.Core;
using ClickyWindows.Models;
using ClickyWindows.Services;
using ClickyWindows.UI.Overlay;
using ClickyWindows.UI.Panel;
using ClickyWindows.WindowsAPI;

namespace ClickyWindows;

public partial class App : System.Windows.Application
{
    private SystemTrayManager? _trayManager;
    private HotkeyManager? _hotkeyManager;
    private OverlayManager? _overlayManager;
    private CompanionManager? _companionManager;
    private SettingsService? _settingsService;
    private AppSettings? _settings;
    private ScreenCaptureService? _screenCaptureService;
    private TranscriptionService? _transcriptionService;
    private LlmVisionService? _llmVisionService;
    private TtsService? _ttsService;
    private AudioFeedbackService? _audioFeedbackService;
    private Window? _messageWindow;
    private CompanionPanel? _companionPanel;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Debug.WriteLine("🎯 Clicky Windows: Starting...");

        // 1. Load settings
        _settingsService = new SettingsService();
        _settings = await _settingsService.LoadAsync();

        // 2. Create a hidden message-only window for Win32 messaging
        _messageWindow = new Window
        {
            Width = 0, Height = 0,
            WindowStyle = WindowStyle.None,
            ShowInTaskbar = false,
            ResizeMode = ResizeMode.NoResize,
            AllowsTransparency = true,
            Background = System.Windows.Media.Brushes.Transparent,
            ShowActivated = false,
        };
        _messageWindow.Show();
        _messageWindow.Hide();

        var hWnd = new WindowInteropHelper(_messageWindow).Handle;

        // 3. Initialize services
        _trayManager = new SystemTrayManager(_messageWindow);
        _trayManager.CreateTrayIcon();

        _overlayManager = new OverlayManager();
        _hotkeyManager = new HotkeyManager();
        _screenCaptureService = new ScreenCaptureService();
        _transcriptionService = new TranscriptionService(_settings);
        _llmVisionService = new LlmVisionService(_settings);
        _ttsService = new TtsService(_settings);
        _audioFeedbackService = new AudioFeedbackService(
            _settings?.AudioFeedbackEnabled ?? true);

        // Apply saved hotkey preference
        bool ctrl = _settings?.UseCtrlAlt == true || _settings?.UseCtrlAltShift == true || _settings?.UseCtrlShift == true;
        bool alt = _settings?.UseCtrlAlt == true || _settings?.UseCtrlAltShift == true || _settings?.UseAltShift == true;
        bool shift = _settings?.UseCtrlShift == true || _settings?.UseCtrlAltShift == true || _settings?.UseAltShift == true;
        _hotkeyManager.SetHotkeyCombo(ctrl, alt, shift);

        // 4. Create companion manager (wires up all components)
        _companionManager = new CompanionManager(
            _hotkeyManager, _overlayManager, _screenCaptureService,
            _transcriptionService, _llmVisionService, _ttsService,
            _trayManager, _audioFeedbackService, _settings, _settingsService);

        _companionManager.VoiceStateChanged += (_, state) => _overlayManager?.SetVoiceState(state);
        _companionManager.StreamingResponseTextChanged += (_, text) => _overlayManager?.SetResponseText(text);
        _companionManager.AudioPowerLevelChanged += (_, level) => _overlayManager?.SetAudioLevel(level);

        // 5. Wire up tray icon events
        _trayManager.TrayIconClicked += OnTrayIconClicked;
        _trayManager.QuitRequested += OnQuitRequested;
        _trayManager.OnboardingRequested += OnOnboardingRequested;

        Debug.WriteLine("🎯 Clicky Windows: Ready! Press hotkey to talk.");

        // 6. Onboarding or first-run experience
        bool hasApiKey = !string.IsNullOrEmpty(_settings?.OpenRouterApiKey);

        // Apply startup registry if enabled
        if (_settings?.LaunchAtStartup == true)
            EnsureStartupRegistry();

        if (!_settings.HasCompletedOnboarding)
        {
            ShowOnboarding(hasApiKey);
        }
        else if (!hasApiKey)
        {
            _trayManager.ShowNotification("Clicky",
                "No API key configured! Click the tray icon to open settings.");
        }
        else
        {
            _trayManager.ShowNotification("Clicky",
                "Ready! Press your hotkey to start talking.");
        }
    }

    private static void EnsureStartupRegistry()
    {
        try
        {
            const string key = @"Software\Microsoft\Windows\CurrentVersion\Run";
            const string valueName = "Clicky";
            using var regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(key, true);
            if (regKey == null) return;
            var exePath = Process.GetCurrentProcess().MainModule?.FileName;
            if (!string.IsNullOrEmpty(exePath) && regKey.GetValue(valueName) == null)
            {
                regKey.SetValue(valueName, $"\"{exePath}\"");
                Debug.WriteLine("✅ Added Clicky to startup registry");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"⚠️ Startup registry check error: {ex.Message}");
        }
    }

    private void OnTrayIconClicked(object? sender, EventArgs e)
    {
        ShowCompanionPanel();
    }

    private void ShowCompanionPanel()
    {
        if (_companionPanel == null || !_companionPanel.IsVisible)
        {
            _companionPanel?.Close();

            var primaryScreen = OverlayManager.GetMonitorInfos()
                .FirstOrDefault(m => m.IsPrimary);
            double left = 0, top = 0;
            if (primaryScreen != null)
            {
                left = primaryScreen.Left + primaryScreen.Width - 380;
                top = primaryScreen.Top + primaryScreen.Height - 620;
            }

            _companionPanel = new CompanionPanel(
                _companionManager, _settings, _settingsService, _hotkeyManager);
            _companionPanel.Left = left;
            _companionPanel.Top = top;
            _companionPanel.Show();
        }
        else
        {
            _companionPanel.Activate();
        }
    }

    private void OnQuitRequested(object? sender, EventArgs e)
    {
        if (_settings != null && _settingsService != null)
            _ = _settingsService.SaveAsync(_settings);
        System.Windows.Application.Current.Shutdown();
    }

    private void OnOnboardingRequested(object? sender, EventArgs e)
    {
        _companionManager?.PerformOnboardingDemoAsync();
    }

    private async void ShowOnboarding(bool hasApiKey)
    {
        await Task.Delay(500);

        var onboarding = new OnboardingOverlay(hasApiKey);
        onboarding.ShowDialog();

        if (_settings != null)
        {
            _settings.HasCompletedOnboarding = true;
            if (_settingsService != null)
                await _settingsService.SaveAsync(_settings);
        }

        _companionManager?.PerformOnboardingDemoAsync();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_settings != null && _settingsService != null)
            _ = _settingsService.SaveAsync(_settings);

        _companionManager?.Dispose();
        _hotkeyManager?.Dispose();
        _overlayManager?.Dispose();
        _trayManager?.Dispose();
        _transcriptionService?.Dispose();
        _screenCaptureService?.Dispose();
        _llmVisionService?.Dispose();
        _ttsService?.Dispose();
        _audioFeedbackService?.Dispose();
        _messageWindow?.Close();
        base.OnExit(e);
    }
}
