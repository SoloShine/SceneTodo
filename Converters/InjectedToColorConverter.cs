using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SceneTodo.Converters
{
    /// <summary>
    /// 将注入状态转换为颜色
    /// </summary>
    public class InjectedToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isInjected)
            {
                // 如果已注入，返回红色（危险），否则返回绿色（成功）
                return isInjected 
                    ? new SolidColorBrush(Color.FromRgb(240, 84, 84))  // 红色
                    : new SolidColorBrush(Color.FromRgb(82, 196, 26));  // 绿色
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
