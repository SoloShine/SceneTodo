# 标签数据库诊断和修复工具
# 用于检查和修复标签数据问题

param(
    [switch]$Fix = $false
)

$dbPath = "$env:LOCALAPPDATA\SceneTodo\todo.db"

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "标签数据库诊断工具" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# 检查数据库文件
if (-not (Test-Path $dbPath)) {
    Write-Host "? 数据库文件不存在: $dbPath" -ForegroundColor Red
    exit
}

Write-Host "? 数据库文件存在" -ForegroundColor Green
Write-Host ""

# 加载 SQLite
$sqliteDll = Get-ChildItem -Path "." -Filter "System.Data.SQLite.dll" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1

if ($sqliteDll) {
    Add-Type -Path $sqliteDll.FullName
} else {
    Write-Host "?? 无法找到 System.Data.SQLite.dll，尝试使用系统路径" -ForegroundColor Yellow
    try {
        Add-Type -AssemblyName System.Data.SQLite
    } catch {
        Write-Host "? 无法加载 SQLite，请确保应用程序已安装" -ForegroundColor Red
        exit
    }
}

# 连接数据库
$connectionString = "Data Source=$dbPath;Version=3;"
$connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)

try {
    $connection.Open()
    Write-Host "? 数据库连接成功" -ForegroundColor Green
    Write-Host ""
    
    # 1. 检查 Tags 表
    Write-Host "【1】检查 Tags 表" -ForegroundColor Cyan
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Tags';"
    $result = $cmd.ExecuteScalar()
    
    if ($result) {
        Write-Host "  ? Tags 表存在" -ForegroundColor Green
        
        # 显示表结构
        $cmd.CommandText = "PRAGMA table_info(Tags);"
        $reader = $cmd.ExecuteReader()
        
        Write-Host "  表结构:" -ForegroundColor Gray
        while ($reader.Read()) {
            $name = $reader["name"]
            $type = $reader["type"]
            Write-Host "    - $name ($type)" -ForegroundColor Gray
        }
        $reader.Close()
    } else {
        Write-Host "  ? Tags 表不存在" -ForegroundColor Red
        
        if ($Fix) {
            Write-Host "  正在创建 Tags 表..." -ForegroundColor Yellow
            $cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Tags (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Color TEXT DEFAULT '#2196F3',
    CreatedAt TEXT
);
"@
            $cmd.ExecuteNonQuery() | Out-Null
            Write-Host "  ? Tags 表创建成功" -ForegroundColor Green
        }
    }
    Write-Host ""
    
    # 2. 列出所有标签
    Write-Host "【2】标签列表" -ForegroundColor Cyan
    $cmd.CommandText = "SELECT Id, Name, Color, CreatedAt FROM Tags ORDER BY CreatedAt DESC;"
    $reader = $cmd.ExecuteReader()
    
    $tags = @()
    $index = 1
    while ($reader.Read()) {
        $tag = @{
            Index = $index
            Id = $reader["Id"]
            Name = $reader["Name"]
            Color = $reader["Color"]
            CreatedAt = $reader["CreatedAt"]
        }
        $tags += $tag
        
        Write-Host ("  [{0}] ID: {1}" -f $index, $tag.Id) -ForegroundColor White
        Write-Host ("      Name: {0}" -f $tag.Name) -ForegroundColor Gray
        Write-Host ("      Color: {0}" -f $tag.Color) -ForegroundColor Gray
        Write-Host ("      Created: {0}" -f $tag.CreatedAt) -ForegroundColor Gray
        Write-Host ""
        $index++
    }
    $reader.Close()
    
    if ($tags.Count -eq 0) {
        Write-Host "  ?? 数据库中没有标签" -ForegroundColor Yellow
    } else {
        Write-Host ("  总计: {0} 个标签" -f $tags.Count) -ForegroundColor Green
    }
    Write-Host ""
    
    # 3. 检查 TodoItemTags 关联
    Write-Host "【3】检查标签关联" -ForegroundColor Cyan
    $cmd.CommandText = @"
SELECT t.Name, COUNT(tit.Id) as UsageCount
FROM Tags t
LEFT JOIN TodoItemTags tit ON t.Id = tit.TagId
GROUP BY t.Id, t.Name
ORDER BY UsageCount DESC;
"@
    $reader = $cmd.ExecuteReader()
    
    while ($reader.Read()) {
        $name = $reader["Name"]
        $count = $reader["UsageCount"]
        Write-Host ("  {0}: {1} 个待办项" -f $name, $count) -ForegroundColor Gray
    }
    $reader.Close()
    Write-Host ""
    
    # 4. 检查孤立的关联记录
    Write-Host "【4】检查数据完整性" -ForegroundColor Cyan
    $cmd.CommandText = @"
SELECT COUNT(*) FROM TodoItemTags
WHERE TagId NOT IN (SELECT Id FROM Tags);
"@
    $orphanCount = $cmd.ExecuteScalar()
    
    if ($orphanCount -gt 0) {
        Write-Host ("  ?? 发现 {0} 个孤立的标签关联记录" -f $orphanCount) -ForegroundColor Yellow
        
        if ($Fix) {
            Write-Host "  正在清理孤立记录..." -ForegroundColor Yellow
            $cmd.CommandText = "DELETE FROM TodoItemTags WHERE TagId NOT IN (SELECT Id FROM Tags);"
            $deleted = $cmd.ExecuteNonQuery()
            Write-Host ("  ? 已删除 {0} 个孤立记录" -f $deleted) -ForegroundColor Green
        }
    } else {
        Write-Host "  ? 没有孤立的关联记录" -ForegroundColor Green
    }
    Write-Host ""
    
    # 5. 诊断建议
    Write-Host "【5】诊断建议" -ForegroundColor Cyan
    
    if ($tags.Count -eq 0) {
        Write-Host "  ?? 数据库中没有标签，请在应用中创建标签" -ForegroundColor Yellow
    }
    
    if ($orphanCount -gt 0 -and -not $Fix) {
        Write-Host "  ?? 发现数据不一致，建议运行: .\Diagnose-Tags.ps1 -Fix" -ForegroundColor Yellow
    }
    
    if ($tags.Count -gt 0) {
        Write-Host "  ? 标签数据正常" -ForegroundColor Green
        Write-Host ""
        Write-Host "  如果编辑标签时提示'数据库中未找到该标签'，可能的原因:" -ForegroundColor Yellow
        Write-Host "  1. 标签是从旧数据加载的，但未保存到数据库" -ForegroundColor Gray
        Write-Host "  2. 应用在启动时加载了缓存数据，但数据库中没有" -ForegroundColor Gray
        Write-Host "  3. 标签的 ID 与数据库不匹配" -ForegroundColor Gray
        Write-Host ""
        Write-Host "  解决方案:" -ForegroundColor Yellow
        Write-Host "  1. 关闭应用程序" -ForegroundColor Gray
        Write-Host "  2. 重新启动应用程序" -ForegroundColor Gray
        Write-Host "  3. 删除旧标签，重新创建" -ForegroundColor Gray
    }
    
    Write-Host ""
    
} catch {
    Write-Host "? 错误: $_" -ForegroundColor Red
} finally {
    $connection.Close()
}

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "诊断完成！" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

if (-not $Fix -and $orphanCount -gt 0) {
    Write-Host "提示: 要自动修复问题，请运行: .\Diagnose-Tags.ps1 -Fix" -ForegroundColor Yellow
    Write-Host ""
}

Write-Host "按任意键退出..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
