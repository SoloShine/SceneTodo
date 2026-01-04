using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using SceneTodo.Models;
using SceneTodo.Services;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SceneTodo.Views
{
    public partial class BackupManagementWindow
    {
        private readonly BackupService _backupService;

        public BackupManagementWindow()
        {
            InitializeComponent();
            _backupService = App.BackupService;
            Loaded += BackupManagementWindow_Loaded;
        }

        private void BackupManagementWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshBackupList();
        }

        /// <summary>
        /// Refresh backup list
        /// </summary>
        private void RefreshBackupList()
        {
            try
            {
                var backups = _backupService.GetBackupList();
                BackupsDataGrid.ItemsSource = backups;

                // Update statistics
                TotalBackupsText.Text = backups.Count.ToString();
                var totalSize = backups.Sum(b => b.FileSize);
                TotalSizeText.Text = FormatFileSize(totalSize);
            }
            catch (Exception ex)
            {
                MessageBox.Error($"Failed to load backup list: {ex.Message}", "Error");
            }
        }

        /// <summary>
        /// Create backup button click
        /// </summary>
        private async void CreateBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var progress = new Progress<int>(percent =>
                {
                    // TODO: Show progress dialog
                    System.Diagnostics.Debug.WriteLine($"Backup progress: {percent}%");
                });

                var backupPath = await _backupService.CreateBackupAsync(BackupType.Manual, progress);
                
                MessageBox.Success($"Backup created successfully!\n\nFile: {Path.GetFileName(backupPath)}", "Success");
                RefreshBackupList();
            }
            catch (Exception ex)
            {
                MessageBox.Error($"Failed to create backup: {ex.Message}", "Error");
            }
        }

        /// <summary>
        /// Import backup button click
        /// </summary>
        private void ImportBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Title = "Select Backup File",
                    Filter = "Backup Files (*.zip)|*.zip|All Files (*.*)|*.*",
                    Multiselect = false
                };

                if (dialog.ShowDialog() == true)
                {
                    var backupDir = _backupService.GetBackupDirectory();
                    var fileName = Path.GetFileName(dialog.FileName);
                    var targetPath = Path.Combine(backupDir, fileName);

                    // Copy to backup directory
                    File.Copy(dialog.FileName, targetPath, true);

                    MessageBox.Success($"Backup imported successfully!\n\nFile: {fileName}", "Success");
                    RefreshBackupList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Error($"Failed to import backup: {ex.Message}", "Error");
            }
        }

        /// <summary>
        /// Open backup folder button click
        /// </summary>
        private void OpenBackupFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var backupDir = _backupService.GetBackupDirectory();
                if (Directory.Exists(backupDir))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = backupDir,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Warning("Backup directory does not exist.", "Warning");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Error($"Failed to open backup folder: {ex.Message}", "Error");
            }
        }

        /// <summary>
        /// Refresh list button click
        /// </summary>
        private void RefreshList_Click(object sender, RoutedEventArgs e)
        {
            RefreshBackupList();
        }

        /// <summary>
        /// Restore selected backup button click
        /// </summary>
        private async void RestoreSelected_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button || button.Tag is not BackupInfo backupInfo)
                return;

            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to restore from this backup?\n\n" +
                    $"File: {Path.GetFileName(backupInfo.FilePath)}\n" +
                    $"Created: {backupInfo.CreatedAt:yyyy-MM-dd HH:mm:ss}\n\n" +
                    $"Warning: This will replace all current data!\n" +
                    $"A snapshot will be created automatically before restore.",
                    "Confirm Restore",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    var progress = new Progress<int>(percent =>
                    {
                        System.Diagnostics.Debug.WriteLine($"Restore progress: {percent}%");
                    });

                    await _backupService.RestoreFromBackupAsync(backupInfo.FilePath, RestoreMode.Replace, progress);

                    MessageBox.Success(
                        "Restore completed successfully!\n\n" +
                        "The application will restart now.",
                        "Success");

                    // Restart application
                    var exePath = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName;
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = exePath,
                            UseShellExecute = true
                        });
                    }
                    Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Error($"Failed to restore backup: {ex.Message}", "Error");
            }
        }

        /// <summary>
        /// Export selected backup button click
        /// </summary>
        private void ExportSelected_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button || button.Tag is not BackupInfo backupInfo)
                return;

            try
            {
                var dialog = new SaveFileDialog
                {
                    Title = "Export Backup",
                    Filter = "Backup Files (*.zip)|*.zip",
                    FileName = Path.GetFileName(backupInfo.FilePath)
                };

                if (dialog.ShowDialog() == true)
                {
                    File.Copy(backupInfo.FilePath, dialog.FileName, true);
                    MessageBox.Success($"Backup exported successfully to:\n\n{dialog.FileName}", "Success");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Error($"Failed to export backup: {ex.Message}", "Error");
            }
        }

        /// <summary>
        /// Delete selected backup button click
        /// </summary>
        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button || button.Tag is not BackupInfo backupInfo)
                return;

            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete this backup?\n\n" +
                    $"File: {Path.GetFileName(backupInfo.FilePath)}\n" +
                    $"Created: {backupInfo.CreatedAt:yyyy-MM-dd HH:mm:ss}\n\n" +
                    $"This action cannot be undone!",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _backupService.DeleteBackup(backupInfo.FilePath);
                    MessageBox.Success("Backup deleted successfully!", "Success");
                    RefreshBackupList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Error($"Failed to delete backup: {ex.Message}", "Error");
            }
        }

        /// <summary>
        /// Close button click
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Format file size for display
        /// </summary>
        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
