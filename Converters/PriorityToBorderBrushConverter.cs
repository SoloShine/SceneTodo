using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using SceneTodo.Models;

namespace SceneTodo.Converters
{
    /// <summary>
    /// 优先级到边框颜色的转换器
    /// VeryHigh=深红色，High=红色，Medium=橙色，Low=灰色，VeryLow=浅灰色
    /// </summary>
    public class PriorityToBorderBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Priority priority)
            {
                return priority switch
                {
                    Priority.VeryHigh => new SolidColorBrush(Color.FromRgb(139, 0, 0)), // 深红色
                    Priority.High => new SolidColorBrush(Colors.Red),
                    Priority.Medium => new SolidColorBrush(Colors.Orange),
                    Priority.Low => new SolidColorBrush(Colors.Gray),
                    Priority.VeryLow => new SolidColorBrush(Colors.LightGray),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
