using System;
using System.ComponentModel;

namespace SceneTodo.Models
{
    /// <summary>
    /// Appearance settings for the application
    /// </summary>
    public class AppearanceSettings : INotifyPropertyChanged
    {
        private double _transparency = 100;
        private string _theme = "Light";
        private bool _enableAnimations = true;
        private string _accentColor = "#007ACC";
        private string _overlayBackground = "#D3D3D3";
        private double _overlayOpacity = 0.3;

        /// <summary>
        /// Overlay transparency (0-100%)
        /// </summary>
        public double Transparency
        {
            get => _transparency;
            set
            {
                if (_transparency != value)
                {
                    _transparency = Math.Clamp(value, 0, 100);
                    OnPropertyChanged(nameof(Transparency));
                }
            }
        }

        /// <summary>
        /// Application theme (Light/Dark)
        /// </summary>
        public string Theme
        {
            get => _theme;
            set
            {
                if (_theme != value)
                {
                    _theme = value;
                    OnPropertyChanged(nameof(Theme));
                }
            }
        }

        /// <summary>
        /// Enable animations
        /// </summary>
        public bool EnableAnimations
        {
            get => _enableAnimations;
            set
            {
                if (_enableAnimations != value)
                {
                    _enableAnimations = value;
                    OnPropertyChanged(nameof(EnableAnimations));
                }
            }
        }

        /// <summary>
        /// Accent color (hex color code)
        /// </summary>
        public string AccentColor
        {
            get => _accentColor;
            set
            {
                if (_accentColor != value)
                {
                    _accentColor = value;
                    OnPropertyChanged(nameof(AccentColor));
                }
            }
        }

        /// <summary>
        /// Overlay background color (hex color code)
        /// </summary>
        public string OverlayBackground
        {
            get => _overlayBackground;
            set
            {
                if (_overlayBackground != value)
                {
                    _overlayBackground = value;
                    OnPropertyChanged(nameof(OverlayBackground));
                }
            }
        }

        /// <summary>
        /// Overlay opacity (0.0-1.0)
        /// </summary>
        public double OverlayOpacity
        {
            get => _overlayOpacity;
            set
            {
                if (_overlayOpacity != value)
                {
                    _overlayOpacity = Math.Clamp(value, 0.1, 1.0);
                    OnPropertyChanged(nameof(OverlayOpacity));
                }
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
