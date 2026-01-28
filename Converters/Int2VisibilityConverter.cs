using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SceneTodo.Converters
{
    /// <summary>
    /// 整数转可见性转换器，大于0则可见
    /// </summary>
    public class Int2VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count && count > 0)
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
