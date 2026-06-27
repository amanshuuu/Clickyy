╔══════════════════════════════════════════════════╗
║         Clicky for Windows - Quick Start         ║
╚══════════════════════════════════════════════════╝

🔷 WHAT IS CLICKY?
   An AI buddy that lives next to your cursor.
   It can see your screen, talk to you, and point at things.

🔷 QUICK START (From ZIP)

   Just double-click Clicky.bat (or Clicky.ps1) — it will:
   1. ✅ Check if .NET 8 Runtime is installed (auto-installs if not)
   2. ✅ Check if Clicky.exe exists (downloads if not)
   3. ✅ Launch Clicky

   That's it! No manual steps needed.

🔷 ONE-CLICK INSTALL (to Start Menu)
   
   Right-click setup/install.ps1 → "Run with PowerShell"
   
   Or open PowerShell as Admin:
     Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
     .\setup\install.ps1

🔷 WHAT GETS INSTALLED
   • Clicky.exe → %LOCALAPPDATA%\Clicky\
   • Start Menu shortcut
   • Desktop shortcut
   • Auto-start on login
   • Uninstaller

🔷 FIRST TIME SETUP
   1. Launch Clicky (blue triangle icon in system tray)
   2. Get a free API key from https://openrouter.ai
   3. Click tray icon → Settings → Enter API key → Save
   4. Press Ctrl+Alt to start talking!

🔷 REQUIREMENTS
   • Windows 10 or 11 (64-bit)
   • Microphone
   • Internet connection
   • .NET 8 Runtime (auto-installed by Clicky.bat)

🔷 TROUBLESHOOTING
   • "Side-by-side configuration error" → Run Clicky.bat, it installs .NET 8
   • App doesn't start? → Right-click → Run as Administrator
   • Hotkey not working? → Run as Administrator
   • Need help? → Open an issue on GitHub
