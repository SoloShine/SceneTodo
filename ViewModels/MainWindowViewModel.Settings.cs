using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using SceneTodo.Models;

namespace SceneTodo.ViewModels
{
    /// <summary>
    /// MainWindowViewModel - Settings Management
    /// Handles application settings, appearance, and behavior configuration
    /// </summary>
    public partial class MainWindowViewModel
    {
        #region Settings Properties

        private AppSettings _appSettings = AppSettings.Load();

        /// <summary>
        /// Application settings
        /// </summary>
        public AppSettings AppSettings
        {
            get => _appSettings;
            set
            {
                if (_appSettings != value)
                {
                    _appSettings = value;
                    OnPropertyChanged(nameof(AppSettings));
                }
            }
        }

        /// <summary>
        /// Overlay transparency (0-100%)
        /// </summary>
        public double OverlayTransparency
        {
            get => AppSettings.Appearance.Transparency;
            set
            {
                if (AppSettings.Appearance.Transparency != value)
                {
                    AppSettings.Appearance.Transparency = value;
                    OnPropertyChanged(nameof(OverlayTransparency));
                    ApplyOverlayTransparency();
                    AppSettings.Save();
                }
            }
        }

        /// <summary>
        /// Enable animations
        /// </summary>
        public bool EnableAnimations
        {
            get => AppSettings.Appearance.EnableAnimations;
            set
            {
                if (AppSettings.Appearance.EnableAnimations != value)
                {
                    AppSettings.Appearance.EnableAnimations = value;
                    OnPropertyChanged(nameof(EnableAnimations));
                    AppSettings.Save();
                }
            }
        }

        #endregion

        #region Settings Commands

        public ICommand SetTransparencyCommand { get; private set; }
        public ICommand ToggleAnimationsCommand { get; private set; }
        public ICommand OpenAppearanceSettingsCommand { get; private set; }

        #endregion

        #region Settings Initialization

        /// <summary>
        /// Initialize settings-related commands
        /// </summary>
        private void InitializeSettingsCommands()
        {
            SetTransparencyCommand = new RelayCommand(param =>
            {
                if (param is double transparency)
                {
                    OverlayTransparency = transparency;
                }
                else if (param is string transparencyStr && double.TryParse(transparencyStr, out var parsed))
                {
                    OverlayTransparency = parsed;
                }
            });

            ToggleAnimationsCommand = new RelayCommand(_ =>
            {
                EnableAnimations = !EnableAnimations;
            });

            OpenAppearanceSettingsCommand = new RelayCommand(_ => OpenAppearanceSettings());
        }

        /// <summary>
        /// Initialize settings from loaded configuration
        /// </summary>
        private void InitializeSettings()
        {
            // Apply loaded settings
            ApplyAppearanceSettings();
            ApplyOverlayTransparency();
        }

        #endregion

        #region Settings Methods

        /// <summary>
        /// Apply appearance settings to the application
        /// </summary>
        private void ApplyAppearanceSettings()
        {
            try
            {
                var appearance = AppSettings.Appearance;

                // Apply theme
                if (!string.IsNullOrEmpty(appearance.Theme))
                {
                    ApplyTheme(appearance.Theme);
                }

                // Apply accent color
                if (!string.IsNullOrEmpty(appearance.AccentColor))
                {
                    ApplyAccentColor(appearance.AccentColor);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to apply appearance settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply theme to the application
        /// </summary>
        /// <param name="theme">Theme name (Light/Dark)</param>
        private void ApplyTheme(string theme)
        {
            try
            {
                var mergedDicts = Application.Current.Resources.MergedDictionaries;

                // Remove existing theme dictionaries
                var themeDicts = new System.Collections.Generic.List<ResourceDictionary>();
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

                mergedDicts.Add(new ResourceDictionary
                {
                    Source = new Uri(skinPath)
                });

                mergedDicts.Add(new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/HandyControl;component/Themes/Theme.xaml")
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to apply theme: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply accent color to the application
        /// </summary>
        /// <param name="colorHex">Hex color code</param>
        private void ApplyAccentColor(string colorHex)
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(colorHex);
                Application.Current.Resources["PrimaryBrush"] = new SolidColorBrush(color);
                Application.Current.Resources["DarkPrimaryBrush"] = new SolidColorBrush(
                    Color.FromRgb(
                        (byte)Math.Max(0, color.R - 40),
                        (byte)Math.Max(0, color.G - 40),
                        (byte)Math.Max(0, color.B - 40)
                    ));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to apply accent color: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply transparency to all overlay windows
        /// </summary>
        private void ApplyOverlayTransparency()
        {
            try
            {
                // Convert percentage (0-100) to opacity (0-1)
                var opacity = OverlayTransparency / 100.0;

                // Apply to all existing overlay windows
                foreach (var window in overlayWindows.Values)
                {
                    if (window != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            window.Opacity = opacity;
                        });
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Applied transparency: {OverlayTransparency}% (opacity: {opacity})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to apply overlay transparency: {ex.Message}");
            }
        }

        /// <summary>
        /// Open appearance settings window
        /// </summary>
        private void OpenAppearanceSettings()
        {
            try
            {
                var settingsWindow = new Views.AppearanceSettingsWindow
                {
                    DataContext = this,
                    Owner = Application.Current.MainWindow
                };
                settingsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Error($"Failed to open settings window: {ex.Message}", "Error");
            }
        }

        /// <summary>
        /// Reset all settings to defaults
        /// </summary>
        public void ResetAllSettings()
        {
            try
            {
                AppSettings.Reset();
                OnPropertyChanged(nameof(AppSettings));
                OnPropertyChanged(nameof(OverlayTransparency));
                OnPropertyChanged(nameof(EnableAnimations));
                ApplyAppearanceSettings();
                ApplyOverlayTransparency();
                HandyControl.Controls.Growl.Success("Settings reset to defaults");
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Error($"Failed to reset settings: {ex.Message}", "Error");
            }
        }

        #endregion
    }
}
