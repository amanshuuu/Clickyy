╔══════════════════════════════════════════════════╗
║         Clicky for Windows - Quick Start         ║
╚══════════════════════════════════════════════════╝

🔷 WHAT IS CLICKY?
   An AI buddy that lives next to your cursor.
   It can see your screen, talk to you, and point at things.

   Customize hotkey, toggle text-only privacy mode,
   view interaction history — all from the settings panel.

🔷 ONE-CLICK INSTALL

   Option A — PowerShell Script (recommended):
     Right-click install.ps1 → "Run with PowerShell"
     
     Or open PowerShell as Admin and run:
       Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
       .\install.ps1

   Option B — Inno Setup Installer (if you have Inno Setup):
     Open installer.iss in Inno Setup → Compile → Run Clicky-Setup.exe

   Option C — Portable:
     Just run Clicky.exe directly! No installation needed.

🔷 WHAT GETS INSTALLED
   • Clicky.exe → %LOCALAPPDATA%\Clicky\
   • Start Menu shortcut
   • Desktop shortcut (optional)
   • Auto-start on login (optional)
   • Uninstaller

🔷 FIRST TIME SETUP
   1. Launch Clicky (blue triangle icon in system tray)
   2. Get a free API key from https://openrouter.ai
   3. Click the tray icon → Settings → Enter your API key → Save
   4. Press your hotkey (default: Ctrl+Alt) to start talking!

🔷 REQUIREMENTS
   • Windows 10 or Windows 11 (64-bit)
   • Microphone (for voice input)
   • Internet connection (for AI features)

🔷 UNINSTALL
   • Run "%LOCALAPPDATA%\Clicky\uninstall.ps1"
   • Or use Windows Settings → Apps → Clicky

🔷 FREE API KEYS NEEDED
   OpenRouter (for AI vision + transcription):
     Sign up: https://openrouter.ai
     Free credits on signup, no credit card required

   Windows TTS (for voice output):
     Built into Windows, FREE, no API key needed, works offline!

🔷 FEATURES
   • Configurable hotkey (Ctrl+Alt, Ctrl+Shift, Alt+Shift, Ctrl+Alt+Shift)
   • Text-only privacy mode (disables screen capture)
   • Audio feedback chimes for recording start/stop
   • Auto-start on login
   • Interaction history (saves last 20 conversations)
   • Model fallback (auto-retries with backup if primary fails)
   • Multi-monitor support

🔷 TROUBLESHOOTING
   • App doesn't start? Run as Administrator
   • Hotkey not working? Try running as Administrator
   • No sound? Check Windows Volume Mixer
   • Need help? Open an issue on GitHub
