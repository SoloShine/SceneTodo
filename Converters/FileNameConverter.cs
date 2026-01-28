using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace SceneTodo.Converters
{
    /// <summary>
    /// 文件名转换器，从完整路径提取文件名
    /// </summary>
    public class FileNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string filePath && !string.IsNullOrEmpty(filePath))
            {
                return Path.GetFileName(filePath);
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
