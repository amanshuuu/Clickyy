using System.Windows;

namespace ClickyWindows.Animations;

/// <summary>
/// Provides a Bezier-arc flight animation for the blue cursor triangle.
/// When Claude identifies an element to point at, the cursor smoothly
/// arcs from its current position to the target coordinate.
/// </summary>
public sealed class FlightAnimation
{
    private readonly System.Windows.Threading.DispatcherTimer _timer;
    private readonly Action<System.Windows.Point> _onPositionUpdate;
    private readonly Action _onComplete;

    private System.Windows.Point _start;
    private System.Windows.Point _end;
    private System.Windows.Point _controlPoint;
    private double _progress;
    private const double Duration = 0.6;
    private const double FlightArcHeight = 80;

    public FlightAnimation(Action<System.Windows.Point> onPositionUpdate, Action onComplete)
    {
        _onPositionUpdate = onPositionUpdate;
        _onComplete = onComplete;

        _timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _timer.Tick += OnTick;
    }

    public void Start(System.Windows.Point startPoint, System.Windows.Point endPoint)
    {
        _start = startPoint;
        _end = endPoint;
        _progress = 0;

        var mid = new System.Windows.Point(
            (startPoint.X + endPoint.X) / 2,
            (startPoint.Y + endPoint.Y) / 2);

        double dx = endPoint.X - startPoint.X;
        double dy = endPoint.Y - startPoint.Y;
        double len = Math.Sqrt(dx * dx + dy * dy);
        if (len > 0)
        {
            double nx = -dy / len;
            double ny = dx / len;
            _controlPoint = new System.Windows.Point(
                mid.X + nx * FlightArcHeight,
                mid.Y + ny * FlightArcHeight);
        }
        else
        {
            _controlPoint = mid;
        }

        _timer.Start();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        _progress += 1.0 / (Duration * 60);
        if (_progress >= 1.0)
        {
            _progress = 1.0;
            _timer.Stop();
            _onPositionUpdate(_end);
            _onComplete();
            return;
        }

        double t = _progress;
        double oneMinusT = 1 - t;

        var pos = new System.Windows.Point(
            oneMinusT * oneMinusT * _start.X + 2 * oneMinusT * t * _controlPoint.X + t * t * _end.X,
            oneMinusT * oneMinusT * _start.Y + 2 * oneMinusT * t * _controlPoint.Y + t * t * _end.Y);

        _onPositionUpdate(pos);
    }

    public void Stop()
    {
        _timer.Stop();
    }
}
