using System;
using System.Globalization;
using System.Windows.Data;

namespace SceneTodo.Converters
{
    /// <summary>
    /// Converts percentage (0-100) to opacity (0-1)
    /// </summary>
    public class PercentToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percent)
            {
                return percent / 100.0;
            }
            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double opacity)
            {
                return opacity * 100.0;
            }
            return 100.0;
        }
    }
}
