# Clicky for Windows 🪟

**AI buddy that lives next to your cursor.** See your screen, talk to it, and it points at things.

> 🪟 **Windows Only** — This is a Windows desktop app (WPF/.NET 8). Does NOT run on Linux/Mac.

---

## 🚀 How to Install & Run (Windows Only)

### ✅ Option 1: Download ZIP → Extract → Run (EASIEST)

1. Go to **[GitHub Releases](https://github.com/amanshuuu/Clickyy/releases)**
2. Download **`Clickyy-v1.0.0.zip`** (66MB, includes everything)
3. **Extract** the ZIP anywhere on your Windows PC
4. **Double-click** `Clicky.exe` — that's it!

> The ZIP already contains the EXE. No download, no install, no .NET needed.

### ✅ Option 2: Bootstrap Script (auto-downloads)

1. Download **`Clicky.bat`** or **`Clicky.ps1`** from GitHub Releases
2. Double-click it — it auto-downloads `Clicky.exe` and launches

### ✅ Option 3: Install to Start Menu

```powershell
Right-click setup/install.ps1 → "Run with PowerShell"
```

---

## 🔑 Get Your Free API Key

Clicky needs **one free API key** to work:

| Service | What | How |
|---|---|---|
| **OpenRouter** | AI vision + voice transcription | [openrouter.ai](https://openrouter.ai) — free credits on signup |

1. Go to [openrouter.ai](https://openrouter.ai)
2. Sign up → Create API key → Copy it
3. Launch Clicky → Click tray icon (blue triangle) → Settings → Paste key → Save

---

## 🎮 How to Use

| Action | What happens |
|---|---|
| **Press & hold `Ctrl+Alt`** | Blue overlay appears + waveform animates |
| **Speak your question** | Microphone captures your voice |
| **Release `Ctrl+Alt`** | Screen captured + voice transcribed → sent to AI |
| **Clicky responds** | Text bubble + voice speaks the answer |
| **Clicky points** 🎯 | Blue cursor flies to elements on your screen |

---

## ⚙️ Features

- **Configurable hotkey** — Ctrl+Alt, Ctrl+Shift, Alt+Shift, or all three
- **Multi-monitor** — Full support for multiple displays
- **Text-only mode** — Disable screen capture for privacy
- **Audio chimes** — Hear when recording starts/stops
- **Interaction history** — Last 20 conversations saved
- **Model fallback** — Auto-retries with backup if primary fails
- **Auto-start** — Launch on login (toggle in settings)

---

## 🏗️ Project Structure

```
📁 Clickyy/
├── 🚀 Clicky.exe          ← The app (just double-click to run!)
├── 🚀 Clicky.bat          ← Bootstrap launcher
├── 🚀 Clicky.ps1          ← PowerShell launcher
├── 📄 README.md           ← This file
├── 📁 ClickyWindows/      ← Full C# source code (27 files)
├── 📁 setup/              ← Install scripts + settings
└── 📁 demo/               ← Interactive HTML demo
```

**Built with:** .NET 8 / WPF / Win32 interop / NAudio / OpenRouter API

---

## 🎬 Watch It In Action

- [Clicky: The AI That Physically Points at Your Screen](https://www.youtube.com/watch?v=aXgKA4J4na0)
- [Clicky: AI Screen-Aware Tutor Walkthrough](https://www.youtube.com/watch?v=izK2eSGF6vY)
- [Clicky - Open Source AI Teacher](https://www.youtube.com/watch?v=ZX9A31WoBEs)

---

## ❓ Troubleshooting

- **"localhost refused to connect"?** You're running on Linux/Mac. This is Windows-only.
- **App doesn't start?** Run as Administrator (right-click → Run as admin)
- **Hotkey not working?** Try running as Administrator
- **No sound?** Check Windows Volume Mixer
- **Need help?** [Open an issue](https://github.com/amanshuuu/Clickyy/issues)

> 💡 **Questions?** Open an issue on GitHub!
