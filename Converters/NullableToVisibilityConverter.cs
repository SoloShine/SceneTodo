using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SceneTodo.Converters
{
    /// <summary>
    /// Nullable值转可见性转换器，有值则可见
    /// </summary>
    public class NullableToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            // 处理可空类型
            if (value is DateTime dateTime && dateTime == default(DateTime))
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
