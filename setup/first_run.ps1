# Clicky First Run Wizard
Write-Host "╔══════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║         Welcome to Clicky for Windows!          ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

Write-Host "Before you start, you need one free API key:" -ForegroundColor Yellow
Write-Host ""

Write-Host "🔑 OpenRouter API Key (FREE credits on signup)" -ForegroundColor Green
Write-Host "   Go to: https://openrouter.ai" -ForegroundColor White
Write-Host "   Sign up → Create a key → Copy it" -ForegroundColor White
Write-Host ""

$key = Read-Host "Paste your OpenRouter API key here (or press Enter to skip)"
if ($key) {
    $settingsPath = "$env:APPDATA\Clicky\settings.json"
    if (Test-Path $settingsPath) {
        $settings = Get-Content $settingsPath -Raw | ConvertFrom-Json
        $settings.OpenRouterApiKey = $key
        $settings | ConvertTo-Json | Set-Content $settingsPath
        Write-Host "✅ API key saved!" -ForegroundColor Green
        Write-Host ""
        Write-Host "🚀 Press Ctrl+Alt to start talking to your AI buddy!" -ForegroundColor Cyan
    }
} else {
    Write-Host ""
    Write-Host "⚠️  You can add your API key later by editing:" -ForegroundColor Yellow
    Write-Host "   %APPDATA%\Clicky\settings.json" -ForegroundColor White
}

Read-Host "Press Enter to launch Clicky"
