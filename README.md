# Clicky for Windows 🪟

**AI buddy that lives next to your cursor.** See your screen, talk to it, and it points at things.

> 🪟 **Windows 10/11 only**

---

## 🚀 Option 1: Quick Start (if EXE works)

1. Install [.NET 8 Desktop Runtime](https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe)
2. Download **`Clicky.exe`** from [Releases](https://github.com/amanshuuu/Clickyy/releases)
3. Double-click `Clicky.exe`
4. Get API key at [openrouter.ai](https://openrouter.ai) → Settings → Save
5. Press **Ctrl+Alt** → talk!

## 🔧 Option 2: Build from Source (100% reliable)

If the pre-built EXE gives errors, build it on your own PC:

1. Install [.NET 8 SDK](https://aka.ms/dotnet/8.0/dotnet-sdk-win-x64.exe)
2. Clone the repo or download source ZIP:
   ```
   git clone https://github.com/amanshuuu/Clickyy.git
   ```
3. Run the build script:
   ```
   cd Clickyy
   build.bat
   ```
4. `Clicky.exe` will be in the same folder — double-click to run!

## 🔧 Option 3: Build & Launch (PowerShell)

```
Right-click BUILD.ps1 → Run with PowerShell
```
It auto-installs .NET 8 SDK if missing, builds Clicky, and launches it.

---

## 🎬 YouTube Videos

- [Clicky: The AI That Physically Points at Your Screen](https://www.youtube.com/watch?v=aXgKA4J4na0)
- [Clicky: AI Screen-Aware Tutor Walkthrough](https://www.youtube.com/watch?v=izK2eSGF6vY)
- [Clicky - Open Source AI Teacher](https://www.youtube.com/watch?v=ZX9A31WoBEs)

## 🏗️ Project Structure

```
📁 Clickyy/
├── 🚀 Clicky.exe          ← Built app (try this first)
├── 🚀 build.bat           ← Build script (Windows batch)
├── 🚀 BUILD.ps1           ← Build script (PowerShell)
├── 📁 ClickyWindows/      ← Full C# source code (27 files)
├── 📁 setup/              ← Install scripts + settings
└── 📁 demo/               ← Interactive HTML demo
```

**Stack:** .NET 8 / WPF / Win32 / NAudio / OpenRouter API
