using System;
using System.IO;
using System.Text.Json;
using System.Diagnostics;

namespace SceneTodo.Models
{
    /// <summary>
    /// Application settings manager
    /// </summary>
    public class AppSettings
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SceneTodo",
            "settings.json"
        );

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Appearance settings
        /// </summary>
        public AppearanceSettings Appearance { get; set; } = new AppearanceSettings();

        /// <summary>
        /// Behavior settings
        /// </summary>
        public BehaviorSettings Behavior { get; set; } = new BehaviorSettings();

        /// <summary>
        /// Keyboard shortcut settings
        /// </summary>
        public ShortcutSettings Shortcuts { get; set; } = new ShortcutSettings();

        /// <summary>
        /// Save settings to file
        /// </summary>
        public void Save()
        {
            try
            {
                var directory = Path.GetDirectoryName(SettingsPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(this, JsonOptions);
                File.WriteAllText(SettingsPath, json);
                Debug.WriteLine($"Settings saved to: {SettingsPath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Load settings from file
        /// </summary>
        /// <returns>Loaded settings or default settings</returns>
        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                    if (settings != null)
                    {
                        Debug.WriteLine($"Settings loaded from: {SettingsPath}");
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load settings: {ex.Message}");
            }

            Debug.WriteLine("Using default settings");
            return new AppSettings();
        }

        /// <summary>
        /// Reset to default settings
        /// </summary>
        public void Reset()
        {
            Appearance = new AppearanceSettings();
            Behavior = new BehaviorSettings();
            Shortcuts = new ShortcutSettings();
            Save();
        }
    }
}
