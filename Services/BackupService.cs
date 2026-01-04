using Microsoft.EntityFrameworkCore;
using SceneTodo.Models;
using SceneTodo.Services.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SceneTodo.Services
{
    /// <summary>
    /// 备份服务
    /// </summary>
    public class BackupService
    {
        private readonly TodoDbContext dbContext;
        private readonly string dataDir;
        private readonly string backupDir;

        public BackupService(TodoDbContext dbContext)
        {
            this.dbContext = dbContext;
            this.dataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SceneTodo");
            this.backupDir = Path.Combine(dataDir, "Backups");

            // 确保备份目录存在
            if (!Directory.Exists(backupDir))
            {
                Directory.CreateDirectory(backupDir);
            }
        }

        /// <summary>
        /// 创建备份
        /// </summary>
        public async Task<string> CreateBackupAsync(BackupType type, IProgress<int>? progress = null)
        {
            progress?.Report(0);

            // 1. 生成备份文件名
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var typeName = type == BackupType.Snapshot ? "Snapshot" : "Backup";
            var backupFileName = $"SceneTodo_{typeName}_{timestamp}.zip";
            var backupFilePath = Path.Combine(backupDir, backupFileName);

            progress?.Report(10);

            // 2. 关闭数据库连接（确保文件可以复制）
            await dbContext.Database.CloseConnectionAsync();

            progress?.Report(20);

            // 3. 创建临时目录
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                progress?.Report(30);

                // 4. 复制数据库文件
                var dbPath = Path.Combine(dataDir, "todo.db");
                var tempDbPath = Path.Combine(tempDir, "todo.db");
                
                if (File.Exists(dbPath))
                {
                    File.Copy(dbPath, tempDbPath, true);
                }
                else
                {
                    throw new FileNotFoundException("Database file not found", dbPath);
                }

                progress?.Report(50);

                // 5. 复制关联应用配置文件（如果存在）
                var appAssociationsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app_associations.json");
                if (File.Exists(appAssociationsPath))
                {
                    var tempAssociationsPath = Path.Combine(tempDir, "app_associations.json");
                    File.Copy(appAssociationsPath, tempAssociationsPath, true);
                }

                progress?.Report(70);

                // 6. 创建备份信息文件
                var backupInfo = new BackupInfo
                {
                    Type = type,
                    CreatedAt = DateTime.Now,
                    DbVersion = GetDbVersion(),
                    AppVersion = GetAppVersion(),
                    Description = $"{type} backup created at {DateTime.Now}"
                };

                var backupInfoPath = Path.Combine(tempDir, "backup-info.json");
                var json = JsonSerializer.Serialize(backupInfo, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(backupInfoPath, json);

                progress?.Report(80);

                // 7. 压缩为ZIP文件
                ZipFile.CreateFromDirectory(tempDir, backupFilePath);

                progress?.Report(95);

                // 8. 获取文件大小并更新备份信息
                var fileInfo = new FileInfo(backupFilePath);
                backupInfo.FilePath = backupFilePath;
                backupInfo.FileSize = fileInfo.Length;

                progress?.Report(100);

                return backupFilePath;
            }
            finally
            {
                // 清理临时目录
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }

                // 重新打开数据库连接
                await dbContext.Database.OpenConnectionAsync();
            }
        }

        /// <summary>
        /// 从备份恢复
        /// </summary>
        public async Task RestoreFromBackupAsync(string backupFilePath, RestoreMode mode, IProgress<int>? progress = null)
        {
            progress?.Report(0);

            // 1. 验证备份文件
            if (!File.Exists(backupFilePath))
            {
                throw new FileNotFoundException("Backup file not found", backupFilePath);
            }

            if (!backupFilePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Invalid backup file format. Only ZIP files are supported.", nameof(backupFilePath));
            }

            progress?.Report(10);

            // 2. 创建当前数据快照（以防恢复失败）
            string? snapshotPath = null;
            try
            {
                snapshotPath = await CreateBackupAsync(BackupType.Snapshot);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to create snapshot: {ex.Message}");
                // 继续恢复，但没有快照保护
            }

            progress?.Report(20);

            // 3. 创建临时目录解压
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                progress?.Report(30);

                // 4. 解压备份文件
                ZipFile.ExtractToDirectory(backupFilePath, tempDir);

                progress?.Report(50);

                // 5. 验证备份信息
                var backupInfoPath = Path.Combine(tempDir, "backup-info.json");
                if (File.Exists(backupInfoPath))
                {
                    var json = await File.ReadAllTextAsync(backupInfoPath);
                    var backupInfo = JsonSerializer.Deserialize<BackupInfo>(json);

                    System.Diagnostics.Debug.WriteLine($"Restoring backup from {backupInfo?.CreatedAt}, version {backupInfo?.AppVersion}");
                }

                progress?.Report(60);

                // 6. 关闭数据库连接
                await dbContext.Database.CloseConnectionAsync();

                progress?.Report(70);

                // 7. 根据模式处理数据
                var tempDbPath = Path.Combine(tempDir, "todo.db");
                var dbPath = Path.Combine(dataDir, "todo.db");

                if (!File.Exists(tempDbPath))
                {
                    throw new FileNotFoundException("Database file not found in backup", tempDbPath);
                }

                switch (mode)
                {
                    case RestoreMode.Replace:
                        // 完全替换现有数据
                        File.Copy(tempDbPath, dbPath, true);
                        break;

                    case RestoreMode.Merge:
                    case RestoreMode.Skip:
                        // 暂时使用替换模式
                        // TODO: 实现真正的合并和跳过逻辑
                        File.Copy(tempDbPath, dbPath, true);
                        break;
                }

                progress?.Report(90);

                // 8. 恢复关联应用配置（如果存在）
                var tempAssociationsPath = Path.Combine(tempDir, "app_associations.json");
                if (File.Exists(tempAssociationsPath))
                {
                    var appAssociationsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app_associations.json");
                    File.Copy(tempAssociationsPath, appAssociationsPath, true);
                }

                progress?.Report(95);

                // 9. 重新打开数据库连接
                await dbContext.Database.OpenConnectionAsync();

                // 10. 删除快照（恢复成功）
                if (!string.IsNullOrEmpty(snapshotPath) && File.Exists(snapshotPath))
                {
                    File.Delete(snapshotPath);
                }

                progress?.Report(100);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Restore failed: {ex.Message}");

                // 尝试恢复快照
                if (!string.IsNullOrEmpty(snapshotPath) && File.Exists(snapshotPath))
                {
                    try
                    {
                        await RestoreFromBackupAsync(snapshotPath, RestoreMode.Replace, null);
                        System.Diagnostics.Debug.WriteLine("Rolled back to snapshot successfully");
                    }
                    catch (Exception rollbackEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to rollback: {rollbackEx.Message}");
                    }
                }

                throw;
            }
            finally
            {
                // 清理临时目录
                if (Directory.Exists(tempDir))
                {
                    try
                    {
                        Directory.Delete(tempDir, true);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to cleanup temp directory: {ex.Message}");
                    }
                }

                // 确保数据库连接已打开
                if (dbContext.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
                {
                    await dbContext.Database.OpenConnectionAsync();
                }
            }
        }

        /// <summary>
        /// 获取所有备份列表
        /// </summary>
        public List<BackupInfo> GetBackupList()
        {
            var backups = new List<BackupInfo>();

            if (!Directory.Exists(backupDir))
            {
                return backups;
            }

            var backupFiles = Directory.GetFiles(backupDir, "*.zip");

            foreach (var filePath in backupFiles)
            {
                try
                {
                    var fileInfo = new FileInfo(filePath);
                    var backupInfo = new BackupInfo
                    {
                        FilePath = filePath,
                        FileSize = fileInfo.Length,
                        CreatedAt = fileInfo.CreationTime,
                        Type = filePath.Contains("Snapshot") ? BackupType.Snapshot : 
                               filePath.Contains("Auto") ? BackupType.Automatic : BackupType.Manual
                    };

                    // 尝试读取备份信息
                    try
                    {
                        using (var archive = ZipFile.OpenRead(filePath))
                        {
                            var infoEntry = archive.GetEntry("backup-info.json");
                            if (infoEntry != null)
                            {
                                using (var stream = infoEntry.Open())
                                using (var reader = new StreamReader(stream))
                                {
                                    var json = reader.ReadToEnd();
                                    var info = JsonSerializer.Deserialize<BackupInfo>(json);
                                    if (info != null)
                                    {
                                        backupInfo.DbVersion = info.DbVersion;
                                        backupInfo.AppVersion = info.AppVersion;
                                        backupInfo.Type = info.Type;
                                        backupInfo.Description = info.Description;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to read backup metadata: {ex.Message}");
                    }

                    backups.Add(backupInfo);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to process backup file {filePath}: {ex.Message}");
                }
            }

            return backups.OrderByDescending(b => b.CreatedAt).ToList();
        }

        /// <summary>
        /// 删除备份
        /// </summary>
        public void DeleteBackup(string backupFilePath)
        {
            if (File.Exists(backupFilePath))
            {
                File.Delete(backupFilePath);
            }
        }

        /// <summary>
        /// 清理旧备份（保留最近N个）
        /// </summary>
        public void CleanupOldBackups(int retentionCount)
        {
            var backups = GetBackupList()
                .Where(b => b.Type != BackupType.Snapshot) // 不删除快照
                .OrderByDescending(b => b.CreatedAt)
                .ToList();

            // 删除超过保留数量的备份
            for (int i = retentionCount; i < backups.Count; i++)
            {
                try
                {
                    DeleteBackup(backups[i].FilePath);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to delete backup: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 获取备份目录
        /// </summary>
        public string GetBackupDirectory()
        {
            return backupDir;
        }

        // 辅助方法
        private string GetDbVersion()
        {
            return "1.4";
        }

        private string GetAppVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly()
                .GetName().Version?.ToString() ?? "1.4.0";
        }
    }
}
