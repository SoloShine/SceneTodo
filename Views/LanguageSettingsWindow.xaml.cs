using SceneTodo.Models;
using SceneTodo.Services;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using MessageBox = System.Windows.MessageBox;

namespace SceneTodo.Views
{
    /// <summary>
    /// 语言设置窗口
    /// </summary>
    public partial class LanguageSettingsWindow : HandyControl.Controls.Window
    {
        private readonly AppSettings _settings;
        private bool _settingsChanged = false;

        public LanguageSettingsWindow(AppSettings settings)
        {
            InitializeComponent();
            _settings = settings;
            LoadSettings();
        }

        /// <summary>
        /// 加载设置
        /// </summary>
        private void LoadSettings()
        {
            // 填充语言下拉框
            var languages = Enum.GetValues(typeof(SupportedLanguage))
                .Cast<SupportedLanguage>()
                .Select(lang => new
                {
                    Value = lang,
                    Description = GetEnumDescription(lang)
                })
                .ToList();

            LanguageComboBox.ItemsSource = languages;
            LanguageComboBox.SelectedValue = _settings.Language.CurrentLanguage;

            // 设置自动检测
            AutoDetectCheckBox.IsChecked = _settings.Language.AutoDetectLanguage;
        }

        /// <summary>
        /// 获取枚举描述
        /// </summary>
        private string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                .FirstOrDefault() as DescriptionAttribute;
            return attribute?.Description ?? value.ToString();
        }

        /// <summary>
        /// 语言选择变化
        /// </summary>
        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedValue is SupportedLanguage selectedLanguage)
            {
                if (_settings.Language.CurrentLanguage != selectedLanguage)
                {
                    _settingsChanged = true;
                }
            }
        }

        /// <summary>
        /// 自动检测变化
        /// </summary>
        private void AutoDetectCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            _settingsChanged = true;

            if (AutoDetectCheckBox.IsChecked == true)
            {
                LanguageComboBox.IsEnabled = false;

                // 根据系统语言设置
                var systemCulture = System.Globalization.CultureInfo.CurrentUICulture;
                var detectedLanguage = systemCulture.TwoLetterISOLanguageName.Equals("zh", StringComparison.OrdinalIgnoreCase)
                    ? SupportedLanguage.ChineseSimplified
                    : SupportedLanguage.English;

                LanguageComboBox.SelectedValue = detectedLanguage;
            }
            else
            {
                LanguageComboBox.IsEnabled = true;
            }
        }

        /// <summary>
        /// 保存按钮点击
        /// </summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 保存语言设置
                _settings.Language.CurrentLanguage = (SupportedLanguage)LanguageComboBox.SelectedValue;
                _settings.Language.AutoDetectLanguage = AutoDetectCheckBox.IsChecked == true;

                // 应用语言设置
                if (_settings.Language.AutoDetectLanguage)
                {
                    LocalizationService.Instance.AutoDetectLanguage();
                }
                else
                {
                    LocalizationService.Instance.ChangeLanguage(_settings.Language.CurrentLanguage);
                }

                // 保存到文件
                _settings.Save();

                if (_settingsChanged)
                {
                    MessageBox.Show(
                        LocalizationService.Instance["Message_SaveSuccess"] + "\n\n" +
                        "语言更改将在重启应用后生效 / Language change will take effect after restarting the application",
                        LocalizationService.Instance["Message_Info"],
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{LocalizationService.Instance["Message_SaveFailed"]}: {ex.Message}",
                    LocalizationService.Instance["Message_Error"],
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 取消按钮点击
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
