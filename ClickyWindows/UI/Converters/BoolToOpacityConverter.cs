using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ClickyWindows.UI.Converters;

/// <summary>
/// Converts a boolean to an opacity value (true → 1.0, false → 0.0).
/// Used for fading UI elements based on visibility state.
/// </summary>
[ValueConversion(typeof(bool), typeof(double))]
public class BoolToOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && b ? 1.0 : 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is double d && d > 0.5;
    }
}

/// <summary>
/// Inverts a boolean value for binding.
/// </summary>
[ValueConversion(typeof(bool), typeof(bool))]
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b ? !b : false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b ? !b : false;
    }
}
