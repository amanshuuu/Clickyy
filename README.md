# Clicky for Windows 🪟

**AI buddy that lives next to your cursor.** See your screen, talk to it, and it points at things.

> 🪟 **Windows 10/11 only** — This is a Windows desktop app (WPF/.NET 8). Does NOT run on Linux/Mac.

---

## 🚀 Quick Start (from ZIP)

### Step 1: Install .NET 8 Runtime (one time only)

Clicky needs **.NET 8 Desktop Runtime**. Install it from Microsoft:

➡️ **[Download .NET 8 Desktop Runtime](https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe)**

Run the installer → click Next → done. (Takes 30 seconds.)

### Step 2: Run Clicky

1. Download **`Clickyy-v1.0.0.zip`** from [Releases](https://github.com/amanshuuu/Clickyy/releases)
2. **Extract** anywhere on your PC
3. **Double-click** `Clicky.exe` — blue triangle icon appears in system tray

### Step 3: Get a Free API Key

4. Go to **[openrouter.ai](https://openrouter.ai)** → Sign up → Create API key
5. Click the blue triangle in system tray → Settings → Paste API key → Save

### Step 4: Start Talking

6. Press & hold **`Ctrl+Alt`** → blue overlay appears
7. Speak your question → release **`Ctrl+Alt`**
8. Clicky sees your screen, answers, and points at things!

---

## ⚠️ If Clicky Doesn't Start

| Problem | Solution |
|---|---|
| **"Side-by-side configuration error"** | Install [.NET 8 Desktop Runtime](https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe) |
| **Nothing happens when double-clicking** | Right-click `Clicky.exe` → **Run as Administrator** |
| **Hotkey doesn't work** | Run Clicky as Administrator |
| **Blue triangle not in tray** | Click the `^` arrow near clock to show hidden icons |

---

## 🎬 Watch It In Action

- [Clicky: The AI That Physically Points at Your Screen](https://www.youtube.com/watch?v=aXgKA4J4na0)
- [Clicky: AI Screen-Aware Tutor Walkthrough](https://www.youtube.com/watch?v=izK2eSGF6vY)
- [Clicky - Open Source AI Teacher](https://www.youtube.com/watch?v=ZX9A31WoBEs)

---

## ✨ Features

- **Configurable hotkey** — Ctrl+Alt, Ctrl+Shift, Alt+Shift, or all three
- **Multi-monitor support**
- **Text-only privacy mode** — Disable screen capture
- **Interaction history** — Last 20 conversations saved
- **Model fallback** — Auto-retries with backup if primary fails
- **Auto-start on login** — Toggle in settings

---

## 🏗️ Project Structure

```
📁 Clickyy/
├── 🚀 Clicky.exe          ← The app (double-click to run)
├── 🚀 Clicky.bat          ← Launcher script
├── 🚀 Clicky.ps1          ← PowerShell launcher
├── 📁 ClickyWindows/      ← Full C# source code (27 files)
├── 📁 setup/              ← Install scripts + settings
└── 📁 demo/               ← Interactive HTML demo
```

**Built with:** .NET 8 / WPF / Win32 interop / NAudio / OpenRouter API
