using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using ClickyWindows.Core;

namespace ClickyWindows.UI.Overlay;

/// <summary>
/// A simple onboarding overlay that shows the user how to use Clicky.
/// Displayed on first launch or when the user clicks "Onboarding" in the settings panel.
/// </summary>
public class OnboardingOverlay : Window
{
    private readonly bool _hasApiKey;

    public OnboardingOverlay(bool hasApiKey)
    {
        _hasApiKey = hasApiKey;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Title = "Welcome to Clicky";
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = new SolidColorBrush(Color.FromArgb(200, 16, 18, 17));
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Width = 480;
        Height = 480;
        ShowInTaskbar = false;
        Topmost = true;
        ResizeMode = ResizeMode.NoResize;
        WindowStyle = WindowStyle.None;

        // Main container
        var border = new Border
        {
            Background = new SolidColorBrush(DesignSystem.Colors.Surface1),
            BorderBrush = new SolidColorBrush(DesignSystem.Colors.BorderSubtle),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(16),
            Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                Opacity = 0.5,
                BlurRadius = 24,
                ShadowDepth = 12,
            }
        };

        var stack = new StackPanel
        {
            Margin = new Thickness(32, 24, 32, 24),
        };

        // Title
        stack.Children.Add(new TextBlock
        {
            Text = "👋 Welcome to Clicky!",
            FontSize = 22,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(DesignSystem.Colors.TextPrimary),
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 6),
        });

        stack.Children.Add(new TextBlock
        {
            Text = "Your AI buddy that lives next to your cursor",
            FontSize = 13,
            Foreground = new SolidColorBrush(DesignSystem.Colors.TextSecondary),
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 24),
        });

        // Divider
        stack.Children.Add(new Rectangle
        {
            Height = 1,
            Fill = new SolidColorBrush(DesignSystem.Colors.BorderSubtle),
            Margin = new Thickness(0, 0, 0, 24),
        });

        // Instructions
        AddInstruction(stack, "🎤", "Hold Ctrl+Alt to talk",
            "Press and hold both keys, then speak naturally into your mic");
        AddInstruction(stack, "🖱️", "Release to send",
            "Clicky captures your screen and transcripts your voice");
        AddInstruction(stack, "🤖", "AI analyzes your screen",
            "OpenRouter vision model sees what you're looking at");
        AddInstruction(stack, "🎯", "Clicky points at things",
            "The blue cursor flies to elements on your screen");
        AddInstruction(stack, "🔊", "Clicky speaks back",
            "Hear the response through your speakers (free, built-in TTS)");

        // API key warning
        if (!_hasApiKey)
        {
            stack.Children.Add(new Rectangle
            {
                Height = 1,
                Fill = new SolidColorBrush(DesignSystem.Colors.BorderSubtle),
                Margin = new Thickness(0, 8, 0, 8),
            });

            var warningBox = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(40, 245, 158, 11)), // Warning yellow at 15%
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(0, 0, 0, 12),
            };
            warningBox.Child = new TextBlock
            {
                Text = "⚠️ You need an OpenRouter API key to use Clicky.\nGet free credits at: https://openrouter.ai\nEnter it in the Clicky settings panel (tray icon).",
                FontSize = 12,
                Foreground = new SolidColorBrush(DesignSystem.Colors.Warning),
                TextWrapping = TextWrapping.Wrap,
            };
            stack.Children.Add(warningBox);
        }

        // Got it button
        var button = new Button
        {
            Content = new TextBlock
            {
                Text = "Got it!",
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Center,
            },
            Background = new SolidColorBrush(DesignSystem.Colors.Blue500),
            Foreground = new SolidColorBrush(Colors.White),
            Height = 40,
            Width = 160,
            Cursor = System.Windows.Input.Cursors.Hand,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 8, 0, 0),
        };
        button.Click += (_, _) => Close();

        // Button styling
        var buttonBorder = new Border
        {
            Child = button,
            CornerRadius = new CornerRadius(10),
            Background = new SolidColorBrush(DesignSystem.Colors.Blue500),
            ClipToBounds = true,
        };
        button.Background = Brushes.Transparent;
        stack.Children.Add(buttonBorder);

        border.Child = stack;
        Content = border;

        // Wire up close on Escape
        KeyDown += (_, e) =>
        {
            if (e.Key == System.Windows.Input.Key.Escape) Close();
        };
    }

    private static void AddInstruction(StackPanel parent, string icon, string title, string description)
    {
        var row = new Grid
        {
            Margin = new Thickness(0, 0, 0, 16),
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(40) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            }
        };

        var iconText = new TextBlock
        {
            Text = icon,
            FontSize = 24,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        Grid.SetColumn(iconText, 0);
        row.Children.Add(iconText);

        var textStack = new StackPanel();
        Grid.SetColumn(textStack, 1);
        textStack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(DesignSystem.Colors.TextPrimary),
        });
        textStack.Children.Add(new TextBlock
        {
            Text = description,
            FontSize = 12,
            Foreground = new SolidColorBrush(DesignSystem.Colors.TextTertiary),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 2, 0, 0),
        });

        row.Children.Add(textStack);
        parent.Children.Add(row);
    }
}
