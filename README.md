# Clicky for Windows 🪟

**AI buddy that lives next to your cursor.** See your screen, talk to it, and it points at things.

## 📸 What It Does

| Step | What Happens |
|---|---|
| 1. **Press & hold hotkey** 🎤 | Transparent overlay appears with blue cursor buddy, waveform animates to your voice |
| 2. **Speak your question** 🗣️ | Microphone captures your voice (16kHz) |
| 3. **Release hotkey** ✋ | Clicky captures your screen, transcribes your voice, sends both to AI |
| 4. **AI thinks** 🤖 | OpenRouter vision model analyzes your screen + question (SSE streaming) |
| 5. **Clicky responds** 🔊 | Text bubble appears + TTS speaks the answer |
| 6. **Clicky points** 🎯 | Blue cursor flies to relevant UI elements using [POINT] tags |

## 🚀 One-Click Install

### Option 1: PowerShell (easiest)

```
Right-click setup/install.ps1 → "Run with PowerShell"
```

Or in PowerShell:
```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\setup\install.ps1
```

This **automatically**:
* Installs Clicky.exe to `%LOCALAPPDATA%\Clicky\`
* Creates **Start Menu** and **Desktop** shortcuts
* Adds to **Windows startup** (optional)
* Creates **uninstaller**
* Adds **firewall rule** for network access
* Launches Clicky when done!

### Option 2: Portable (no install)

Just run `setup\Clicky.exe` directly — works immediately.

## 📋 Requirements

* **Windows 10 or Windows 11** (64-bit)
* **Microphone** for voice input
* **Internet connection** for AI features (TTS works offline)

## 🔑 Free API Keys Needed

| Service | What it does | Cost | How to get |
|---|---|---|---|
| **OpenRouter** | AI vision + transcription | **Free credits** | [openrouter.ai](https://openrouter.ai) → Sign up → Create key |
| **Windows TTS** | Text-to-speech | **FREE** (built-in) | No key needed! Works offline |
| **ElevenLabs** | Premium TTS (optional) | Free tier: 10k chars/mo | elevenlabs.io |

## 🎮 Features

- **Configurable hotkey** — Ctrl+Alt (default), Ctrl+Shift, Alt+Shift, or Ctrl+Alt+Shift
- **Text-only mode** — Disable screen capture for privacy
- **Audio feedback** — Chime on record start/stop (can be disabled)
- **Auto-start** — Launch Clicky when you log in
- **Interaction history** — Last 20 conversations saved
- **Model fallback** — Auto-tries backup model if primary fails
- **Multi-monitor** — Full support for multiple displays
- **System tray** — Blue triangle icon with right-click menu

## ⚙️ Settings

Click the tray icon → settings panel where you can configure:
- OpenRouter API key
- AI model selection (Claude, GPT-4, Gemini, etc.)
- Push-to-talk shortcut
- TTS provider (Windows built-in or ElevenLabs)
- Audio feedback, text-only mode, auto-start
- View recent interactions

## 🎬 Watch It In Action

| Video | Description |
|---|---|
| [Clicky: The AI That Physically Points at Your Screen](https://www.youtube.com/watch?v=aXgKA4J4na0) | Best overview of the pointing mechanic |
| [Clicky: AI Screen-Aware Tutor Walkthrough](https://www.youtube.com/watch?v=izK2eSGF6vY) | Detailed walkthrough of all features |
| [Clicky - Open Source AI Teacher](https://www.youtube.com/watch?v=ZX9A31WoBEs) | Setup + full demo |
| [Farza makes crazy tools!](https://www.youtube.com/watch?v=O_kWKAriT24) | Short demo of blue cursor overlay |

## 🏗️ Project Structure

```
ClickyWindows/
├── setup/                  ← One-click download & run
│   ├── Clicky.exe          ← Self-contained 71MB executable
│   ├── install.ps1         ← PowerShell installer
│   ├── installer.iss       ← Inno Setup script
│   └── first_run.ps1       ← API key wizard
├── Core/                   ← State machine, hotkeys, tray, overlay
├── Services/               ← AI, screen capture, TTS, transcription, audio feedback
├── UI/                     ← WPF windows, overlay controls, onboarding
├── Models/                 ← Data types, settings, interaction history
├── WindowsAPI/             ← Win32 P/Invoke (DPI-aware)
├── Animations/             ← Bezier flight arc for pointing
└── App.xaml.cs             ← Entry point, wires everything
```

## ⚙️ Tech Stack

* **.NET 8** WPF + Win32 interop (DPI-aware)
* **NAudio** — microphone capture + audio playback + chimes
* **OpenRouter API** — vision AI + transcription (SSE streaming with model fallback)
* **Windows SAPI** — free built-in TTS (works offline)
* **GDI BitBlt** — multi-monitor screen capture
* **Low-level keyboard hook** — global push-to-talk

## 📄 License

Open source. Built by the community.
