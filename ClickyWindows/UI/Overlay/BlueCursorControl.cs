using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ClickyWindows.Core;
using ClickyWindows.Models;

namespace ClickyWindows.UI.Overlay;

/// <summary>
/// The visual representation of the buddy cursor on a single monitor.
/// Displays a blue triangle that follows the mouse, with state-dependent
/// overlays: waveform bars (listening), spinner (processing), text bubble (responding).
/// 
/// Equivalent to macOS BlueCursorView (~700 lines in OverlayWindow.swift).
/// Every visual element is drawn using WPF shapes and animations.
/// </summary>
public class BlueCursorControl : Canvas
{
    private readonly MonitorInfo _monitor;
    private readonly Canvas _cursorTriangle;
    private readonly Canvas _waveformContainer;
    private readonly Canvas _spinnerContainer;
    private readonly Border _textBubble;
    private readonly TextBlock _textBlock;

    private readonly DispatcherTimer _animationTimer;
    private System.Windows.Point _cursorPosition;
    private VoiceState _currentState = VoiceState.Idle;
    private string _responseText = "";
    private float _audioLevel;

    private readonly System.Windows.Shapes.Rectangle[] _waveformBars;
    private const int WaveformBarCount = 24;

    public BlueCursorControl(MonitorInfo monitor)
    {
        _monitor = monitor;
        Width = monitor.Width;
        Height = monitor.Height;
        Background = new SolidColorBrush(System.Windows.Media.Colors.Transparent);
        IsHitTestVisible = false;

        // 1. Cursor triangle
        _cursorTriangle = CreateCursorTriangle();
        Children.Add(_cursorTriangle);

        // 2. Waveform container (hidden by default)
        _waveformContainer = new Canvas { IsHitTestVisible = false, Opacity = 0 };
        _waveformBars = new System.Windows.Shapes.Rectangle[WaveformBarCount];
        for (int i = 0; i < WaveformBarCount; i++)
        {
            _waveformBars[i] = new System.Windows.Shapes.Rectangle
            {
                Width = 3,
                Fill = new SolidColorBrush(DesignSystem.Colors.OverlayCursorBlue),
                RadiusX = 1.5,
                RadiusY = 1.5,
                IsHitTestVisible = false,
            };
            _waveformContainer.Children.Add(_waveformBars[i]);
        }
        Children.Add(_waveformContainer);

        // 3. Spinner (hidden by default)
        _spinnerContainer = CreateSpinner();
        _spinnerContainer.Opacity = 0;
        Children.Add(_spinnerContainer);

        // 4. Text bubble (hidden by default)
        _textBlock = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            FontSize = DesignSystem.FontSizes.T13,
            Foreground = new SolidColorBrush(DesignSystem.Colors.TextPrimary),
            LineHeight = 18,
            MaxWidth = 300,
            IsHitTestVisible = false,
        };

        _textBubble = new Border
        {
            Child = _textBlock,
            Padding = new Thickness(14, 10, 14, 10),
            CornerRadius = new System.Windows.CornerRadius(10),
            Opacity = 0,
            IsHitTestVisible = false,
            Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(242, 23, 25, 24)),
            BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 55, 59, 57)),
            BorderThickness = new Thickness(0.8),
        };
        _textBubble.Effect = new System.Windows.Media.Effects.DropShadowEffect
        {
            Color = Colors.Black,
            Opacity = 0.35,
            BlurRadius = 16,
            ShadowDepth = 8,
        };
        Children.Add(_textBubble);

        // 5. Animation timer (60fps for waveform)
        _animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _animationTimer.Tick += OnAnimationTick;

        _cursorPosition = new System.Windows.Point(monitor.Width / 2.0, monitor.Height / 2.0);
        PositionElements();
    }

    private static Canvas CreateCursorTriangle()
    {
        double size = DesignSystem.CursorTriangle.Width;
        double cx = size / 2.0;
        double cy = size / 2.0;
        double height = size * Math.Sqrt(3.0) / 2.0;

        var polygon = new System.Windows.Shapes.Polygon
        {
            Points = new PointCollection(new[]
            {
                new System.Windows.Point(cx, cy - height / 1.5),
                new System.Windows.Point(cx - size / 2, cy + height / 3),
                new System.Windows.Point(cx + size / 2, cy + height / 3),
            }),
            Fill = new SolidColorBrush(DesignSystem.Colors.OverlayCursorBlue),
            IsHitTestVisible = false,
        };
        polygon.Effect = new System.Windows.Media.Effects.DropShadowEffect
        {
            Color = DesignSystem.Colors.OverlayCursorBlue,
            Opacity = 0.6,
            BlurRadius = 6,
            ShadowDepth = 0,
        };

        var container = new Canvas
        {
            Width = DesignSystem.CursorTriangle.Width * 2,
            Height = DesignSystem.CursorTriangle.Height * 2,
            IsHitTestVisible = false,
        };
        container.RenderTransform = new RotateTransform(DesignSystem.CursorTriangle.RotationAngle,
            DesignSystem.CursorTriangle.Width / 2,
            DesignSystem.CursorTriangle.Height / 2);

        Canvas.SetLeft(polygon, DesignSystem.CursorTriangle.Width * 0.15);
        Canvas.SetTop(polygon, DesignSystem.CursorTriangle.Height * 0.15);
        container.Children.Add(polygon);
        return container;
    }

    private static Canvas CreateSpinner()
    {
        var arc = new System.Windows.Shapes.Path
        {
            Width = 14,
            Height = 14,
            Stroke = new SolidColorBrush(DesignSystem.Colors.OverlayCursorBlue),
            StrokeThickness = 2.5,
            StrokeEndLineCap = PenLineCap.Round,
            IsHitTestVisible = false,
        };

        var geometry = new PathGeometry();
        var figure = new PathFigure { StartPoint = new System.Windows.Point(7, 0), IsClosed = false };
        figure.Segments.Add(new ArcSegment
        {
            Point = new System.Windows.Point(12, 12),
            Size = new System.Windows.Size(7, 7),
            SweepDirection = SweepDirection.Clockwise,
            IsLargeArc = false,
        });
        geometry.Figures.Add(figure);
        arc.Data = geometry;

        arc.Effect = new System.Windows.Media.Effects.DropShadowEffect
        {
            Color = DesignSystem.Colors.OverlayCursorBlue,
            Opacity = 0.6,
            BlurRadius = 6,
            ShadowDepth = 0,
        };

        var container = new Canvas { Width = 20, Height = 20, IsHitTestVisible = false };
        Canvas.SetLeft(arc, 3);
        Canvas.SetTop(arc, 3);
        container.Children.Add(arc);
        return container;
    }

    #region Public API

    public void UpdateCursorPosition(System.Windows.Point position)
    {
        _cursorPosition = new System.Windows.Point(
            position.X - _monitor.Left + DesignSystem.CursorTriangle.OffsetX,
            _monitor.Height - (position.Y - _monitor.Top - DesignSystem.CursorTriangle.OffsetY));
        PositionElements();
    }

    public void SetVoiceState(VoiceState state)
    {
        if (_currentState == state) return;
        _currentState = state;
        var duration = TimeSpan.FromMilliseconds(200);

        switch (state)
        {
            case VoiceState.Idle:
                FadeElement(_cursorTriangle, 1.0, duration);
                FadeElement(_waveformContainer, 0, duration);
                FadeElement(_spinnerContainer, 0, duration);
                FadeElement(_textBubble, 0, duration);
                _animationTimer.Stop();
                break;
            case VoiceState.Listening:
                FadeElement(_cursorTriangle, 0, duration);
                FadeElement(_waveformContainer, 1.0, duration);
                FadeElement(_spinnerContainer, 0, duration);
                FadeElement(_textBubble, 0, duration);
                _animationTimer.Start();
                break;
            case VoiceState.Processing:
                FadeElement(_cursorTriangle, 0, duration);
                FadeElement(_waveformContainer, 0, duration);
                FadeElement(_spinnerContainer, 1.0, duration);
                FadeElement(_textBubble, 0, duration);
                _animationTimer.Stop();
                break;
            case VoiceState.Responding:
            case VoiceState.Pointing:
                FadeElement(_cursorTriangle, 0, duration);
                FadeElement(_waveformContainer, 0, duration);
                FadeElement(_spinnerContainer, 0, duration);
                FadeElement(_textBubble, 1.0, duration);
                _animationTimer.Stop();
                break;
        }
    }

    public void SetResponseText(string text)
    {
        _responseText = text;
        _textBlock.Text = string.IsNullOrEmpty(text) ? "..." : text;
    }

    public void SetAudioLevel(float level)
    {
        _audioLevel = level;
    }

    #endregion

    private void PositionElements()
    {
        Canvas.SetLeft(_cursorTriangle, _cursorPosition.X);
        Canvas.SetTop(_cursorTriangle, _cursorPosition.Y);
        Canvas.SetLeft(_waveformContainer, _cursorPosition.X - 30);
        Canvas.SetTop(_waveformContainer, _cursorPosition.Y - 20);
        Canvas.SetLeft(_spinnerContainer, _cursorPosition.X + 10);
        Canvas.SetTop(_spinnerContainer, _cursorPosition.Y + 5);
        Canvas.SetLeft(_textBubble, _cursorPosition.X + 20);
        Canvas.SetTop(_textBubble, _cursorPosition.Y - 10);
    }

    private void OnAnimationTick(object? sender, EventArgs e)
    {
        double time = DateTime.Now.TimeOfDay.TotalSeconds * 3.6;
        double normalizedLevel = Math.Max(0, _audioLevel - 0.008f);
        double easedLevel = Math.Pow(Math.Min(normalizedLevel * 2.85, 1), 0.76);

        for (int i = 0; i < WaveformBarCount; i++)
        {
            double phase = time + i * 0.35;
            double idlePulse = (Math.Sin(phase) + 1) / 2 * 1.5;
            double reactiveHeight = easedLevel * 10 * GetBarProfile(i);
            double height = 3 + reactiveHeight + idlePulse;

            _waveformBars[i].Height = height;
            Canvas.SetLeft(_waveformBars[i], i * 5.0);
            Canvas.SetTop(_waveformBars[i], 15 - height);
        }
    }

    private static double GetBarProfile(int index)
    {
        double midpoint = WaveformBarCount / 2.0;
        double distance = Math.Abs(index - midpoint) / midpoint;
        return 1.0 - distance * 0.5;
    }

    private static void FadeElement(FrameworkElement element, double targetOpacity, TimeSpan duration)
    {
        var anim = new DoubleAnimation(targetOpacity, duration) { EasingFunction = new QuadraticEase() };
        element.BeginAnimation(OpacityProperty, anim);
    }
}
