using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SceneTodo.Converters
{
    /// <summary>
    /// 过期状态到画刷颜色转换器
    /// </summary>
    public class OverdueToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isOverdue)
            {
                return isOverdue
                    ? new SolidColorBrush(Colors.Red)
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF888888"));
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
