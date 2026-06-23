using System.Diagnostics;
using System.Runtime.InteropServices;
using ClickyWindows.WindowsAPI;

namespace ClickyWindows.Core;

/// <summary>
/// Manages the global push-to-talk keyboard shortcut.
/// Uses a low-level keyboard hook (WH_KEYBOARD_LL) to detect
/// configurable modifier combinations (Ctrl+Alt, Ctrl+Shift, etc.).
/// Equivalent to macOS GlobalPushToTalkShortcutMonitor (CGEventTap).
/// </summary>
public sealed class HotkeyManager : IDisposable
{
    /// <summary>Fired when the user presses the push-to-talk shortcut.</summary>
    public event EventHandler? ShortcutPressed;

    /// <summary>Fired when the user releases the push-to-talk shortcut.</summary>
    public event EventHandler? ShortcutReleased;

    private readonly HashSet<int> _currentlyDown = new();
    private readonly object _lock = new();

    // Modifier key virtual codes
    private const int VK_LCONTROL = 0xA2;
    private const int VK_RCONTROL = 0xA3;
    private const int VK_LMENU = 0xA4;   // Left Alt
    private const int VK_RMENU = 0xA5;   // Right Alt
    private const int VK_LSHIFT = 0xA0;
    private const int VK_RSHIFT = 0xA1;

    // The low-level hook handle
    private IntPtr _hookId = IntPtr.Zero;
    private NativeMethods.LowLevelKeyboardProc? _hookProc;

    // Current active modifier combo
    private bool _requireCtrl;
    private bool _requireAlt;
    private bool _requireShift;

    public bool IsPressed { get; private set; }

    public HotkeyManager()
    {
        // Default: Ctrl+Alt
        _requireCtrl = true;
        _requireAlt = true;
        _requireShift = false;
        InstallLowLevelHook();
    }

    ~HotkeyManager()
    {
        Unhook();
    }

    /// <summary>
    /// Updates which modifier keys are required for the push-to-talk shortcut.
    /// Call this when the user changes their hotkey preference.
    /// </summary>
    public void SetHotkeyCombo(bool ctrl, bool alt, bool shift)
    {
        lock (_lock)
        {
            _requireCtrl = ctrl;
            _requireAlt = alt;
            _requireShift = shift;
            _currentlyDown.Clear();
            IsPressed = false;
            Debug.WriteLine($"Hotkey combo updated: Ctrl={ctrl}, Alt={alt}, Shift={shift}");
        }
    }

    /// <summary>
    /// Installs a low-level keyboard hook for hotkey detection.
    /// </summary>
    private void InstallLowLevelHook()
    {
        if (_hookId != IntPtr.Zero) return;

        _hookProc = KeyboardHookCallback;

        using var curProcess = Process.GetCurrentProcess();
        using var mainModule = curProcess.MainModule;
        if (mainModule != null)
        {
            _hookId = NativeMethods.SetWindowsHookEx(
                NativeMethods.WH_KEYBOARD_LL,
                _hookProc,
                NativeMethods.GetModuleHandle(mainModule.ModuleName),
                0);

            if (_hookId != IntPtr.Zero)
            {
                Debug.WriteLine("Low-level keyboard hook installed successfully.");
            }
            else
            {
                int error = Marshal.GetLastWin32Error();
                Debug.WriteLine($"Failed to install low-level keyboard hook. Error: {error}");
            }
        }
    }

    /// <summary>
    /// Callback for low-level keyboard events.
    /// Tracks the state of modifier keys and fires press/release events.
    /// </summary>
    private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            var kbStruct = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);
            if (kbStruct == null) return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);

            int vkCode = (int)kbStruct.vkCode;

            bool isDown = wParam == (IntPtr)NativeMethods.WM_KEYDOWN || wParam == (IntPtr)NativeMethods.WM_SYSKEYDOWN;
            bool isUp = wParam == (IntPtr)NativeMethods.WM_KEYUP || wParam == (IntPtr)NativeMethods.WM_SYSKEYUP;

            if (!isDown && !isUp) return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);

            lock (_lock)
            {
                if (isDown) _currentlyDown.Add(vkCode);
                else if (isUp) _currentlyDown.Remove(vkCode);

                bool ctrlDown = _currentlyDown.Contains(VK_LCONTROL) || _currentlyDown.Contains(VK_RCONTROL);
                bool altDown = _currentlyDown.Contains(VK_LMENU) || _currentlyDown.Contains(VK_RMENU);
                bool shiftDown = _currentlyDown.Contains(VK_LSHIFT) || _currentlyDown.Contains(VK_RSHIFT);

                // Check if required modifiers are held (and only those)
                bool modifiersMet = true;
                if (_requireCtrl) modifiersMet &= ctrlDown;
                if (_requireAlt) modifiersMet &= altDown;
                if (_requireShift) modifiersMet &= shiftDown;

                // Pressed when all required modifiers are held
                if (modifiersMet && !IsPressed)
                {
                    IsPressed = true;
                    Task.Run(() => ShortcutPressed?.Invoke(this, EventArgs.Empty));
                }
                // Released when ANY required modifier goes up
                else if (IsPressed)
                {
                    bool stillMet = true;
                    if (_requireCtrl) stillMet &= ctrlDown;
                    if (_requireAlt) stillMet &= altDown;
                    if (_requireShift) stillMet &= shiftDown;

                    if (!stillMet)
                    {
                        IsPressed = false;
                        Task.Run(() => ShortcutReleased?.Invoke(this, EventArgs.Empty));
                    }
                }
            }
        }

        return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    /// <summary>
    /// Unregisters the hotkey and unhooks the keyboard hook.
    /// </summary>
    public void Unhook()
    {
        lock (_lock)
        {
            if (_hookId != IntPtr.Zero)
            {
                NativeMethods.UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
                Debug.WriteLine("Low-level keyboard hook removed.");
            }
            _currentlyDown.Clear();
            IsPressed = false;
        }
    }

    public void Dispose()
    {
        Unhook();
        _hookProc = null;
        GC.SuppressFinalize(this);
    }
}
