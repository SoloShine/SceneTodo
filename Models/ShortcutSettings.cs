using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace SceneTodo.Models
{
    /// <summary>
    /// Keyboard shortcut settings
    /// </summary>
    public class ShortcutSettings : INotifyPropertyChanged
    {
        private Dictionary<string, ShortcutInfo> _shortcuts;

        public ShortcutSettings()
        {
            // Initialize default shortcuts
            _shortcuts = new Dictionary<string, ShortcutInfo>
            {
                ["NewTodo"] = new ShortcutInfo("New Todo", Key.N, ModifierKeys.Control, "Create a new todo item"),
                ["Search"] = new ShortcutInfo("Search", Key.F, ModifierKeys.Control, "Open search/filter panel"),
                ["ExpandCollapse"] = new ShortcutInfo("Expand/Collapse", Key.E, ModifierKeys.Control, "Toggle expand/collapse state"),
                ["Save"] = new ShortcutInfo("Save", Key.S, ModifierKeys.Control, "Save current changes"),
                ["Delete"] = new ShortcutInfo("Delete", Key.Delete, ModifierKeys.None, "Delete selected item"),
                ["Refresh"] = new ShortcutInfo("Refresh", Key.F5, ModifierKeys.None, "Refresh the list"),
                ["Undo"] = new ShortcutInfo("Undo", Key.Z, ModifierKeys.Control, "Undo last action"),
                ["Priority1"] = new ShortcutInfo("Priority: High", Key.D1, ModifierKeys.Alt, "Set priority to High"),
                ["Priority2"] = new ShortcutInfo("Priority: Medium", Key.D2, ModifierKeys.Alt, "Set priority to Medium"),
                ["Priority3"] = new ShortcutInfo("Priority: Low", Key.D3, ModifierKeys.Alt, "Set priority to Low"),
                ["ToggleComplete"] = new ShortcutInfo("Toggle Complete", Key.Space, ModifierKeys.Control, "Toggle completion status"),
                ["Settings"] = new ShortcutInfo("Settings", Key.OemComma, ModifierKeys.Control, "Open settings window"),
            };
        }

        /// <summary>
        /// Dictionary of shortcuts
        /// </summary>
        public Dictionary<string, ShortcutInfo> Shortcuts
        {
            get => _shortcuts;
            set
            {
                if (_shortcuts != value)
                {
                    _shortcuts = value;
                    OnPropertyChanged(nameof(Shortcuts));
                }
            }
        }

        /// <summary>
        /// Get shortcut by action name
        /// </summary>
        public ShortcutInfo? GetShortcut(string action)
        {
            return Shortcuts.TryGetValue(action, out var shortcut) ? shortcut : null;
        }

        /// <summary>
        /// Update shortcut for an action
        /// </summary>
        public bool UpdateShortcut(string action, Key key, ModifierKeys modifiers)
        {
            if (!Shortcuts.ContainsKey(action))
                return false;

            // Check for conflicts
            if (HasConflict(action, key, modifiers))
                return false;

            Shortcuts[action].Key = key;
            Shortcuts[action].Modifiers = modifiers;
            OnPropertyChanged(nameof(Shortcuts));
            return true;
        }

        /// <summary>
        /// Check if shortcut conflicts with existing shortcuts
        /// </summary>
        public bool HasConflict(string excludeAction, Key key, ModifierKeys modifiers)
        {
            foreach (var kvp in Shortcuts)
            {
                if (kvp.Key != excludeAction &&
                    kvp.Value.Key == key &&
                    kvp.Value.Modifiers == modifiers)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Reset to default shortcuts
        /// </summary>
        public void ResetToDefaults()
        {
            _shortcuts = new ShortcutSettings().Shortcuts;
            OnPropertyChanged(nameof(Shortcuts));
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    /// <summary>
    /// Information about a keyboard shortcut
    /// </summary>
    public class ShortcutInfo : INotifyPropertyChanged
    {
        private string _name;
        private Key _key;
        private ModifierKeys _modifiers;
        private string _description;

        public ShortcutInfo(string name, Key key, ModifierKeys modifiers, string description)
        {
            _name = name;
            _key = key;
            _modifiers = modifiers;
            _description = description;
        }

        /// <summary>
        /// Display name of the action
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        /// <summary>
        /// Key code
        /// </summary>
        public Key Key
        {
            get => _key;
            set
            {
                if (_key != value)
                {
                    _key = value;
                    OnPropertyChanged(nameof(Key));
                    OnPropertyChanged(nameof(DisplayText));
                }
            }
        }

        /// <summary>
        /// Modifier keys (Ctrl, Alt, Shift)
        /// </summary>
        public ModifierKeys Modifiers
        {
            get => _modifiers;
            set
            {
                if (_modifiers != value)
                {
                    _modifiers = value;
                    OnPropertyChanged(nameof(Modifiers));
                    OnPropertyChanged(nameof(DisplayText));
                }
            }
        }

        /// <summary>
        /// Description of what the shortcut does
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        /// <summary>
        /// Display text for the shortcut (e.g., "Ctrl+N")
        /// </summary>
        public string DisplayText
        {
            get
            {
                var text = "";
                if ((Modifiers & ModifierKeys.Control) != 0)
                    text += "Ctrl+";
                if ((Modifiers & ModifierKeys.Alt) != 0)
                    text += "Alt+";
                if ((Modifiers & ModifierKeys.Shift) != 0)
                    text += "Shift+";
                if ((Modifiers & ModifierKeys.Windows) != 0)
                    text += "Win+";
                text += Key.ToString();
                return text;
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
