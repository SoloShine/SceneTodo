using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace SceneTodo.Converters
{
    /// <summary>
    /// 枚举值转换为Description特性的转换器
    /// </summary>
    public class EnumToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            var enumType = value.GetType();
            if (!enumType.IsEnum)
                return value.ToString() ?? string.Empty;

            var memberInfo = enumType.GetMember(value.ToString() ?? string.Empty);
            if (memberInfo.Length == 0)
                return value.ToString() ?? string.Empty;

            var attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length == 0)
                return value.ToString() ?? string.Empty;

            return ((DescriptionAttribute)attributes[0]).Description;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
