# Session-12 快速启动指南

> **创建时间**: 2025-01-02  
> **会话主题**: 数据备份恢复系统  
> **预计工时**: 2-3小时

---

## ?? 快速开始

### 前置条件
- ? Session-11 已完成并归档
- ? 标签系统已集成
- ? 数据库已稳定运行
- ? 开发环境已准备就绪

### 开发目标
实现完整的数据备份和恢复功能，保护用户数据安全。

---

## ?? 开发检查清单

### Step 1: 数据模型 (30分钟)

- [ ] 创建 `Models/BackupInfo.cs`
- [ ] 创建 `Models/BackupSettings.cs`
- [ ] 创建 `Converters/FileSizeConverter.cs`
- [ ] 创建 `Converters/FileNameConverter.cs`
- [ ] 测试模型序列化

### Step 2: 备份服务 (1小时)

- [ ] 创建 `Services/BackupService.cs`
- [ ] 实现 `CreateBackupAsync()` 方法
- [ ] 实现 `RestoreFromBackupAsync()` 方法
- [ ] 实现 `GetBackupList()` 方法
- [ ] 实现 `DeleteBackup()` 方法
- [ ] 实现 `CleanupOldBackups()` 方法
- [ ] 在 `App.xaml.cs` 注册服务
- [ ] 测试备份功能

### Step 3: 管理界面 (30分钟)

- [ ] 创建 `Views/BackupManagementWindow.xaml`
- [ ] 创建 `Views/BackupManagementWindow.xaml.cs`
- [ ] 实现备份列表显示
- [ ] 实现创建备份功能
- [ ] 实现恢复备份功能
- [ ] 实现导入备份功能
- [ ] 实现删除备份功能
- [ ] 添加进度显示
- [ ] 测试UI交互

### Step 4: 主窗口集成 (15分钟)

- [ ] 在 `MainWindow.xaml` 添加备份管理菜单
- [ ] 在 `MainWindowViewModel.cs` 添加命令
- [ ] 测试菜单功能

### Step 5: 自动备份 (30分钟)

- [ ] 在 `MainWindowModel.cs` 添加备份设置
- [ ] 实现定时备份逻辑
- [ ] 实现保留策略
- [ ] 测试自动备份

### Step 6: 测试和文档 (30分钟)

- [ ] 完整功能测试
- [ ] 性能测试
- [ ] 边界情况测试
- [ ] 编写使用文档
- [ ] 编写技术文档

---

## ?? 快速命令

### 创建备份目录结构
```powershell
# 在 SceneTodo 目录下执行
mkdir Models -ErrorAction SilentlyContinue
mkdir Services -ErrorAction SilentlyContinue
mkdir Views -ErrorAction SilentlyContinue
mkdir Converters -ErrorAction SilentlyContinue
```

### 测试备份功能
```csharp
// 在 BackupManagementWindow.xaml.cs 中测试
var backupService = new BackupService(App.DbContext);
var backupPath = await backupService.CreateBackupAsync(BackupType.Manual);
MessageBox.Show($"Backup created: {backupPath}");
```

### 验证备份文件
```powershell
# 查看备份目录
$backupDir = "$env:LOCALAPPDATA\SceneTodo\Backups"
Get-ChildItem $backupDir -Filter "*.zip"
```

---

## ? 常见问题

### Q1: 备份时数据库被锁定？
**A**: 确保在备份前关闭数据库连接：
```csharp
await dbContext.Database.CloseConnectionAsync();
// 执行备份操作
await dbContext.Database.OpenConnectionAsync();
```

### Q2: 如何处理大文件备份？
**A**: 使用异步操作和进度报告：
```csharp
var progress = new Progress<int>(percent => 
{
    ProgressBar.Value = percent;
});
await backupService.CreateBackupAsync(BackupType.Manual, progress);
```

### Q3: 恢复失败如何回滚？
**A**: 在恢复前创建快照：
```csharp
var snapshotPath = await CreateBackupAsync(BackupType.Snapshot);
try 
{
    await RestoreFromBackupAsync(backupPath, RestoreMode.Replace);
}
catch 
{
    await RestoreFromBackupAsync(snapshotPath, RestoreMode.Replace);
}
```

---

## ?? 关键代码片段

### 创建备份
```csharp
public async Task<string> CreateBackupAsync(BackupType type, IProgress<int>? progress = null)
{
    progress?.Report(0);
    
    // 生成文件名
    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
    var backupFileName = $"SceneTodo_Backup_{timestamp}.zip";
    var backupFilePath = Path.Combine(backupDir, backupFileName);
    
    // 关闭数据库
    await dbContext.Database.CloseConnectionAsync();
    
    try
    {
        // 复制文件到临时目录
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        
        // 复制数据库文件
        File.Copy(dbPath, Path.Combine(tempDir, "todo.db"));
        
        // 压缩为ZIP
        ZipFile.CreateFromDirectory(tempDir, backupFilePath);
        
        progress?.Report(100);
        return backupFilePath;
    }
    finally
    {
        await dbContext.Database.OpenConnectionAsync();
    }
}
```

### 恢复备份
```csharp
public async Task RestoreFromBackupAsync(string backupFilePath, RestoreMode mode)
{
    // 创建快照
    var snapshotPath = await CreateBackupAsync(BackupType.Snapshot);
    
    try
    {
        // 解压备份文件
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        ZipFile.ExtractToDirectory(backupFilePath, tempDir);
        
        // 关闭数据库
        await dbContext.Database.CloseConnectionAsync();
        
        // 替换数据库文件
        File.Copy(Path.Combine(tempDir, "todo.db"), dbPath, true);
        
        // 重新打开数据库
        await dbContext.Database.OpenConnectionAsync();
        
        // 删除快照
        File.Delete(snapshotPath);
    }
    catch
    {
        // 从快照恢复
        await RestoreFromBackupAsync(snapshotPath, RestoreMode.Replace);
        throw;
    }
}
```

---

## ?? 进度追踪

### 时间分配
- **数据模型**: 0.5小时 ??
- **备份服务**: 1小时 ??
- **管理界面**: 0.5小时 ??
- **自动备份**: 0.5小时 ??
- **测试文档**: 0.5小时 ??

### 里程碑
- ? Step 1 完成 - 数据模型就绪
- ? Step 2 完成 - 备份服务可用
- ? Step 3 完成 - UI集成完成
- ? Step 4 完成 - 自动备份运行
- ? Session-12 完成 - 功能上线

---

## ?? 参考资料

### 技术文档
- [System.IO.Compression.ZipFile](https://docs.microsoft.com/en-us/dotnet/api/system.io.compression.zipfile)
- [IProgress<T>](https://docs.microsoft.com/en-us/dotnet/api/system.iprogress-1)
- [EF Core Connection Management](https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-management)

### 项目文档
- [Session-12 详细规划](Session-12-数据备份恢复规划.md)
- [Session-11 归档报告](../../01-会话记录/Session-11/Session-11归档报告.md)
- [开发路线图](开发路线图-v1.0.md)

---

## ? 完成标准

### 功能标准
- [ ] 可以创建手动备份
- [ ] 可以恢复备份数据
- [ ] 可以管理备份列表
- [ ] 自动备份正常工作
- [ ] 备份保留策略生效

### 质量标准
- [ ] 代码编译通过
- [ ] 无明显Bug
- [ ] 性能表现良好
- [ ] 用户体验友好
- [ ] 文档完整清晰

### 交付标准
- [ ] 所有代码已提交
- [ ] 功能测试通过
- [ ] 文档已完善
- [ ] README已更新
- [ ] 归档报告已创建

---

**快速启动指南版本**: 1.0  
**创建时间**: 2025-01-02  
**适用于**: Session-12 开发

**准备就绪，开始开发！** ??
