using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SceneTodo.Models;
using SceneTodo.ViewModels;

namespace SceneTodo.Views
{
    /// <summary>
    /// ShortcutManagerWindow.xaml interaction logic
    /// </summary>
    public partial class ShortcutManagerWindow
    {
        private readonly MainWindowViewModel? _viewModel;
        private readonly ShortcutSettings _workingCopy;

        public ShortcutManagerWindow()
        {
            InitializeComponent();

            _viewModel = DataContext as MainWindowViewModel;
            
            if (_viewModel != null)
            {
                // Create a working copy of shortcuts
                _workingCopy = new ShortcutSettings();
                foreach (var kvp in _viewModel.AppSettings.Shortcuts.Shortcuts)
                {
                    _workingCopy.Shortcuts[kvp.Key] = new ShortcutInfo(
                        kvp.Value.Name,
                        kvp.Value.Key,
                        kvp.Value.Modifiers,
                        kvp.Value.Description
                    );
                }

                // Bind to UI
                ShortcutsItemsControl.ItemsSource = _workingCopy.Shortcuts;
            }
        }

        /// <summary>
        /// Edit shortcut button clicked
        /// </summary>
        private void EditShortcut_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string action)
            {
                var dialog = new ShortcutEditDialog(action, _workingCopy)
                {
                    Owner = this
                };

                if (dialog.ShowDialog() == true)
                {
                    // Refresh the display
                    ShortcutsItemsControl.ItemsSource = null;
                    ShortcutsItemsControl.ItemsSource = _workingCopy.Shortcuts;
                }
            }
        }

        /// <summary>
        /// Reset button clicked
        /// </summary>
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            var result = HandyControl.Controls.MessageBox.Show(
                "Are you sure you want to reset all keyboard shortcuts to defaults?",
                "Confirm Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _workingCopy.ResetToDefaults();
                
                // Refresh the display
                ShortcutsItemsControl.ItemsSource = null;
                ShortcutsItemsControl.ItemsSource = _workingCopy.Shortcuts;
                
                HandyControl.Controls.Growl.Success("Shortcuts reset to defaults");
            }
        }

        /// <summary>
        /// Save button clicked
        /// </summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                // Copy working shortcuts back to main settings
                foreach (var kvp in _workingCopy.Shortcuts)
                {
                    _viewModel.AppSettings.Shortcuts.Shortcuts[kvp.Key] = kvp.Value;
                }

                _viewModel.AppSettings.Save();
                HandyControl.Controls.Growl.Success("Keyboard shortcuts saved");
            }

            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Cancel button clicked
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    /// <summary>
    /// Dialog for editing a single shortcut
    /// </summary>
    public class ShortcutEditDialog : Window
    {
        private readonly string _action;
        private readonly ShortcutSettings _settings;
        private readonly TextBox _shortcutTextBox;
        private Key _capturedKey = Key.None;
        private ModifierKeys _capturedModifiers = ModifierKeys.None;

        public ShortcutEditDialog(string action, ShortcutSettings settings)
        {
            _action = action;
            _settings = settings;

            Title = "Edit Shortcut";
            Width = 400;
            Height = 200;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;

            var shortcut = settings.GetShortcut(action);
            if (shortcut != null)
            {
                _capturedKey = shortcut.Key;
                _capturedModifiers = shortcut.Modifiers;
            }

            // Create UI
            var grid = new Grid { Margin = new Thickness(20) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Action name
            var actionLabel = new TextBlock
            {
                Text = shortcut?.Name ?? action,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 8)
            };
            Grid.SetRow(actionLabel, 0);
            grid.Children.Add(actionLabel);

            // Description
            var descLabel = new TextBlock
            {
                Text = shortcut?.Description ?? "",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 16)
            };
            Grid.SetRow(descLabel, 1);
            grid.Children.Add(descLabel);

            // Instructions
            var instructionLabel = new TextBlock
            {
                Text = "Press the keys you want to use for this shortcut:",
                Margin = new Thickness(0, 0, 0, 8)
            };
            Grid.SetRow(instructionLabel, 2);
            grid.Children.Add(instructionLabel);

            // Shortcut input
            _shortcutTextBox = new TextBox
            {
                IsReadOnly = true,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                FontSize = 14,
                Padding = new Thickness(8),
                Text = shortcut?.DisplayText ?? "Press keys..."
            };
            _shortcutTextBox.PreviewKeyDown += ShortcutTextBox_PreviewKeyDown;
            Grid.SetRow(_shortcutTextBox, 3);
            grid.Children.Add(_shortcutTextBox);

            // Buttons
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 16, 0, 0)
            };

            var okButton = new Button
            {
                Content = "OK",
                Width = 80,
                Margin = new Thickness(0, 0, 8, 0)
            };
            okButton.Click += OkButton_Click;
            buttonPanel.Children.Add(okButton);

            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 80
            };
            cancelButton.Click += (s, e) => { DialogResult = false; Close(); };
            buttonPanel.Children.Add(cancelButton);

            Grid.SetRow(buttonPanel, 4);
            grid.Children.Add(buttonPanel);

            Content = grid;

            // Focus the textbox
            Loaded += (s, e) => _shortcutTextBox.Focus();
        }

        private void ShortcutTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            // Get the key and modifiers
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            
            // Ignore modifier keys alone
            if (key == Key.LeftCtrl || key == Key.RightCtrl ||
                key == Key.LeftAlt || key == Key.RightAlt ||
                key == Key.LeftShift || key == Key.RightShift ||
                key == Key.LWin || key == Key.RWin)
            {
                return;
            }

            _capturedKey = key;
            _capturedModifiers = Keyboard.Modifiers;

            // Update display
            var text = "";
            if ((_capturedModifiers & ModifierKeys.Control) != 0)
                text += "Ctrl+";
            if ((_capturedModifiers & ModifierKeys.Alt) != 0)
                text += "Alt+";
            if ((_capturedModifiers & ModifierKeys.Shift) != 0)
                text += "Shift+";
            if ((_capturedModifiers & ModifierKeys.Windows) != 0)
                text += "Win+";
            text += _capturedKey.ToString();

            _shortcutTextBox.Text = text;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (_capturedKey == Key.None)
            {
                HandyControl.Controls.MessageBox.Error("Please press a key combination", "Error");
                return;
            }

            // Check for conflicts
            if (_settings.HasConflict(_action, _capturedKey, _capturedModifiers))
            {
                var result = HandyControl.Controls.MessageBox.Show(
                    "This shortcut is already in use. Do you want to replace it?",
                    "Conflict",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;
            }

            // Update the shortcut
            if (_settings.UpdateShortcut(_action, _capturedKey, _capturedModifiers))
            {
                DialogResult = true;
                Close();
            }
            else
            {
                HandyControl.Controls.MessageBox.Error("Failed to update shortcut", "Error");
            }
        }
    }
}
