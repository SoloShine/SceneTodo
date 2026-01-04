using System;
using System.Windows;
using SceneTodo.ViewModels;

namespace SceneTodo.Views
{
    /// <summary>
    /// AppearanceSettingsWindow.xaml interaction logic
    /// </summary>
    public partial class AppearanceSettingsWindow
    {
        private readonly MainWindowViewModel? _viewModel;
        private readonly string _originalTheme;
        private readonly double _originalTransparency;
        private readonly bool _originalAnimations;

        public AppearanceSettingsWindow()
        {
            InitializeComponent();

            // Get ViewModel from DataContext
            _viewModel = DataContext as MainWindowViewModel;

            if (_viewModel != null)
            {
                // Save original values for cancel operation
                _originalTheme = _viewModel.AppSettings.Appearance.Theme;
                _originalTransparency = _viewModel.OverlayTransparency;
                _originalAnimations = _viewModel.EnableAnimations;

                // Set initial theme selection
                ThemeComboBox.SelectedIndex = _originalTheme == "Dark" ? 1 : 0;
            }
        }

        /// <summary>
        /// Theme ComboBox selection changed
        /// </summary>
        private void ThemeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_viewModel == null || ThemeComboBox.SelectedIndex < 0) return;

            string newTheme = ThemeComboBox.SelectedIndex == 1 ? "Dark" : "Light";
            
            if (_viewModel.AppSettings.Appearance.Theme != newTheme)
            {
                _viewModel.AppSettings.Appearance.Theme = newTheme;
                _viewModel.AppSettings.Save();

                // Apply theme immediately
                ApplyTheme(newTheme);
            }
        }

        /// <summary>
        /// Apply theme to the application
        /// </summary>
        private void ApplyTheme(string theme)
        {
            try
            {
                var mergedDicts = Application.Current.Resources.MergedDictionaries;

                // Remove existing theme dictionaries
                var themeDicts = new System.Collections.Generic.List<System.Windows.ResourceDictionary>();
                foreach (var dict in mergedDicts)
                {
                    if (dict.Source != null &&
                        (dict.Source.ToString().Contains("Theme") ||
                         dict.Source.ToString().Contains("Skin")))
                    {
                        themeDicts.Add(dict);
                    }
                }

                foreach (var dict in themeDicts)
                {
                    mergedDicts.Remove(dict);
                }

                // Add new theme
                string skinPath = theme == "Dark"
                    ? "pack://application:,,,/HandyControl;component/Themes/SkinDark.xaml"
                    : "pack://application:,,,/HandyControl;component/Themes/SkinDefault.xaml";

                mergedDicts.Add(new System.Windows.ResourceDictionary
                {
                    Source = new Uri(skinPath)
                });

                mergedDicts.Add(new System.Windows.ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/HandyControl;component/Themes/Theme.xaml")
                });

                HandyControl.Controls.Growl.Success($"Theme changed to {theme}");
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Error($"Failed to apply theme: {ex.Message}", "Error");
            }
        }

        /// <summary>
        /// Reset button clicked
        /// </summary>
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            var result = HandyControl.Controls.MessageBox.Show(
                "Are you sure you want to reset all settings to defaults?",
                "Confirm Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes && _viewModel != null)
            {
                _viewModel.ResetAllSettings();
                
                // Update UI
                ThemeComboBox.SelectedIndex = 0;
                
                HandyControl.Controls.Growl.Success("Settings reset to defaults");
            }
        }

        /// <summary>
        /// Save button clicked
        /// </summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.AppSettings.Save();
                HandyControl.Controls.Growl.Success("Settings saved successfully");
            }
            
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Cancel button clicked
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                // Restore original values
                _viewModel.AppSettings.Appearance.Theme = _originalTheme;
                _viewModel.OverlayTransparency = _originalTransparency;
                _viewModel.EnableAnimations = _originalAnimations;

                // Reapply original theme
                ApplyTheme(_originalTheme);
            }

            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Test sound button clicked
        /// </summary>
        private void TestSoundButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Play system sound for testing
                System.Media.SystemSounds.Asterisk.Play();
                HandyControl.Controls.Growl.Info("Playing test sound");
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Error($"Failed to play sound: {ex.Message}", "Error");
            }
        }

        /// <summary>
        /// Customize shortcuts button clicked
        /// </summary>
        private void CustomizeShortcutsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var shortcutWindow = new ShortcutManagerWindow
                {
                    DataContext = _viewModel,
                    Owner = this
                };
                shortcutWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Error($"Failed to open shortcuts window: {ex.Message}", "Error");
            }
        }
    }
}
