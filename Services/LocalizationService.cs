using SceneTodo.Models;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Windows;
using System.Windows.Data;

namespace SceneTodo.Services
{
    /// <summary>
    /// 本地化服务
    /// </summary>
    public class LocalizationService : INotifyPropertyChanged
    {
        private static LocalizationService? _instance;
        private static readonly object _lock = new object();
        private readonly ResourceManager _resourceManager;
        private CultureInfo _currentCulture;

        public event PropertyChangedEventHandler? PropertyChanged;

        private LocalizationService()
        {
            _resourceManager = new ResourceManager("SceneTodo.Resources.Strings", typeof(LocalizationService).Assembly);
            _currentCulture = CultureInfo.CurrentCulture;
        }

        /// <summary>
        /// 获取单例实例
        /// </summary>
        public static LocalizationService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new LocalizationService();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 当前文化信息
        /// </summary>
        public CultureInfo CurrentCulture
        {
            get => _currentCulture;
            private set
            {
                if (_currentCulture != value)
                {
                    _currentCulture = value;
                    OnPropertyChanged(nameof(CurrentCulture));
                    OnPropertyChanged("Item[]"); // 通知所有索引器更新
                }
            }
        }

        /// <summary>
        /// 索引器，用于获取本地化字符串
        /// </summary>
        public string this[string key]
        {
            get
            {
                try
                {
                    var value = _resourceManager.GetString(key, CurrentCulture);
                    return value ?? $"[{key}]";
                }
                catch
                {
                    return $"[{key}]";
                }
            }
        }

        /// <summary>
        /// 获取本地化字符串
        /// </summary>
        public string GetString(string key)
        {
            return this[key];
        }

        /// <summary>
        /// 获取格式化的本地化字符串
        /// </summary>
        public string GetString(string key, params object[] args)
        {
            try
            {
                var format = GetString(key);
                return string.Format(format, args);
            }
            catch
            {
                return $"[{key}]";
            }
        }

        /// <summary>
        /// 更改当前语言
        /// </summary>
        public void ChangeLanguage(SupportedLanguage language)
        {
            CultureInfo newCulture;

            switch (language)
            {
                case SupportedLanguage.English:
                    newCulture = new CultureInfo("en");
                    break;
                case SupportedLanguage.ChineseSimplified:
                default:
                    newCulture = new CultureInfo("zh-CN");
                    break;
            }

            ChangeLanguage(newCulture);
        }

        /// <summary>
        /// 更改当前语言
        /// </summary>
        public void ChangeLanguage(CultureInfo culture)
        {
            if (_currentCulture.Name == culture.Name)
            {
                return; // 语言没有变化，无需更新
            }

            _currentCulture = culture;
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            // 更新 WPF 的语言设置
            Application.Current.Dispatcher.Invoke(() =>
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = culture;
                System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

                // 通知所有绑定更新
                OnPropertyChanged(nameof(CurrentCulture));
                OnPropertyChanged("Item[]"); // 索引器属性

                System.Diagnostics.Debug.WriteLine($"Language changed to: {culture.Name}");
            });
        }

        /// <summary>
        /// 根据系统语言自动检测
        /// </summary>
        public void AutoDetectLanguage()
        {
            var systemCulture = CultureInfo.CurrentUICulture;

            // 如果系统语言是中文，使用中文，否则使用英文
            if (systemCulture.TwoLetterISOLanguageName.Equals("zh", StringComparison.OrdinalIgnoreCase))
            {
                ChangeLanguage(SupportedLanguage.ChineseSimplified);
            }
            else
            {
                ChangeLanguage(SupportedLanguage.English);
            }
        }

        /// <summary>
        /// 获取当前语言枚举
        /// </summary>
        public SupportedLanguage GetCurrentLanguage()
        {
            if (CurrentCulture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase))
            {
                return SupportedLanguage.English;
            }
            return SupportedLanguage.ChineseSimplified;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// 本地化扩展，用于 XAML 中的绑定
    /// </summary>
    public class LocalizationExtension : Binding
    {
        public LocalizationExtension(string key) : base($"[{key}]")
        {
            Source = LocalizationService.Instance;
            Mode = BindingMode.OneWay;
        }
    }

    /// <summary>
    /// 本地化转换器，用于动态语言切换
    /// </summary>
    public class LocalizationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string key)
            {
                return LocalizationService.Instance[key];
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
