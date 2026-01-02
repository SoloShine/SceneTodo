using System;
using System.Globalization;
using System.Windows.Data;

namespace SceneTodo.Converters
{
    /// <summary>
    /// 将注入状态转换为显示文本
    /// </summary>
    public class InjectedToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isInjected)
            {
                return isInjected ? "关闭注入(_I)" : "开启注入(_I)";
            }
            return "注入";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
