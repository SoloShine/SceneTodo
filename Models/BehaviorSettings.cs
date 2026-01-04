using System.ComponentModel;

namespace SceneTodo.Models
{
    /// <summary>
    /// Behavior settings for the application
    /// </summary>
    public class BehaviorSettings : INotifyPropertyChanged
    {
        private bool _rememberCollapseState = true;
        private bool _autoExpandOnSearch = true;
        private bool _showCompletedTodos = true;
        private bool _enableSoundReminders = true;
        private bool _enableDesktopNotifications = true;
        private bool _enableFlashWindow = false;
        private string _reminderSoundPath = "SystemDefault";
        private int _reminderAdvanceMinutes = 15;
        private int _snoozeMinutes = 10;

        /// <summary>
        /// Remember collapse/expand state of todo items
        /// </summary>
        public bool RememberCollapseState
        {
            get => _rememberCollapseState;
            set
            {
                if (_rememberCollapseState != value)
                {
                    _rememberCollapseState = value;
                    OnPropertyChanged(nameof(RememberCollapseState));
                }
            }
        }

        /// <summary>
        /// Auto expand todos when searching
        /// </summary>
        public bool AutoExpandOnSearch
        {
            get => _autoExpandOnSearch;
            set
            {
                if (_autoExpandOnSearch != value)
                {
                    _autoExpandOnSearch = value;
                    OnPropertyChanged(nameof(AutoExpandOnSearch));
                }
            }
        }

        /// <summary>
        /// Show completed todos in the list
        /// </summary>
        public bool ShowCompletedTodos
        {
            get => _showCompletedTodos;
            set
            {
                if (_showCompletedTodos != value)
                {
                    _showCompletedTodos = value;
                    OnPropertyChanged(nameof(ShowCompletedTodos));
                }
            }
        }

        /// <summary>
        /// Enable sound reminders
        /// </summary>
        public bool EnableSoundReminders
        {
            get => _enableSoundReminders;
            set
            {
                if (_enableSoundReminders != value)
                {
                    _enableSoundReminders = value;
                    OnPropertyChanged(nameof(EnableSoundReminders));
                }
            }
        }

        /// <summary>
        /// Enable desktop notifications
        /// </summary>
        public bool EnableDesktopNotifications
        {
            get => _enableDesktopNotifications;
            set
            {
                if (_enableDesktopNotifications != value)
                {
                    _enableDesktopNotifications = value;
                    OnPropertyChanged(nameof(EnableDesktopNotifications));
                }
            }
        }

        /// <summary>
        /// Flash window to get attention
        /// </summary>
        public bool EnableFlashWindow
        {
            get => _enableFlashWindow;
            set
            {
                if (_enableFlashWindow != value)
                {
                    _enableFlashWindow = value;
                    OnPropertyChanged(nameof(EnableFlashWindow));
                }
            }
        }

        /// <summary>
        /// Path to reminder sound file (or "SystemDefault")
        /// </summary>
        public string ReminderSoundPath
        {
            get => _reminderSoundPath;
            set
            {
                if (_reminderSoundPath != value)
                {
                    _reminderSoundPath = value;
                    OnPropertyChanged(nameof(ReminderSoundPath));
                }
            }
        }

        /// <summary>
        /// Minutes before due date to show reminder
        /// </summary>
        public int ReminderAdvanceMinutes
        {
            get => _reminderAdvanceMinutes;
            set
            {
                if (_reminderAdvanceMinutes != value)
                {
                    _reminderAdvanceMinutes = value;
                    OnPropertyChanged(nameof(ReminderAdvanceMinutes));
                }
            }
        }

        /// <summary>
        /// Snooze duration in minutes
        /// </summary>
        public int SnoozeMinutes
        {
            get => _snoozeMinutes;
            set
            {
                if (_snoozeMinutes != value)
                {
                    _snoozeMinutes = value;
                    OnPropertyChanged(nameof(SnoozeMinutes));
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
