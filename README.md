# Clicky for Windows 🪟

**AI buddy that lives next to your cursor.** See your screen, talk to it, and it points at things.

---

## 🚀 Zero Setup — Just Download & Run

### Option 1: One-click (recommended)

1. **Download** the ZIP from GitHub → **Extract** anywhere
2. **Double-click** `Clicky.ps1` (or `Clicky.bat`)
3. It automatically downloads Clicky.exe (69MB, one-time)
4. **Get a free API key** at [openrouter.ai](https://openrouter.ai)
5. **Press Ctrl+Alt** and start talking!

> ⚡ Everything is automatic — no .NET, no installers, no dependencies.

### Option 2: Direct download (if you already have the exe)

1. Download `Clicky.exe` from [Releases](https://github.com/amanshuuu/Clickyy/releases)
2. Save it anywhere and double-click to run

### Option 3: Install to Start Menu

```powershell
# Right-click setup/install.ps1 → "Run with PowerShell"
.\setup\install.ps1
```

---

## 📖 How It Works

| Step | What happens |
|---|---|
| **Press & hold hotkey** 🎤 | Blue overlay appears + waveform animates to your voice |
| **Speak** 🗣️ | Microphone captures your voice |
| **Release hotkey** ✋ | Screen is captured + voice transcribed → sent to AI |
| **AI responds** 🤖 | Text bubble + voice speaks the answer |
| **Clicky points** 🎯 | Blue cursor flies to elements on your screen |

**Default hotkey:** `Ctrl+Alt` (configurable in settings)

---

## 🔑 Get Your Free API Key

| Service | What | Cost |
|---|---|---|
| **OpenRouter** | AI vision + transcription | Free credits on signup |
| **Windows TTS** | Voice output | FREE (built-in, works offline) |

1. Go to [openrouter.ai](https://openrouter.ai)
2. Sign up → Create a key → Copy it
3. Open Clicky → Click tray icon → Enter API key → Save

---

## ✨ Features

- **Configurable hotkey** — Ctrl+Alt, Ctrl+Shift, Alt+Shift, or all three
- **Multi-monitor** — Full support for multiple displays
- **Text-only mode** — Disable screen capture for privacy
- **Audio chimes** — Hear when recording starts/stops
- **Interaction history** — Last 20 conversations saved
- **Model fallback** — Auto-retries with backup if primary fails
- **Auto-start** — Launch on login (toggle in settings)

---

## 🎬 Watch It In Action

- [Clicky: The AI That Physically Points at Your Screen](https://www.youtube.com/watch?v=aXgKA4J4na0)
- [Clicky: AI Screen-Aware Tutor Walkthrough](https://www.youtube.com/watch?v=izK2eSGF6vY)
- [Clicky - Open Source AI Teacher](https://www.youtube.com/watch?v=ZX9A31WoBEs)

---

## 🏗️ Project Structure

```
📁 Clickyy/
├── 🚀 Clicky.bat          ← Bootstrap: auto-downloads + launches
├── 🚀 Clicky.ps1          ← Same as above (PowerShell, prettier)
├── 📁 ClickyWindows/      ← Full C# source code (27 files, 4,099 lines)
├── 📁 setup/              ← Install scripts + settings template
├── 📁 demo/               ← Interactive HTML demo
└── 📄 README.md           ← This file
```

**Built with:** .NET 8, WPF, Win32 interop, NAudio, OpenRouter API

---

> 💡 **Questions?** Open an issue on GitHub!
