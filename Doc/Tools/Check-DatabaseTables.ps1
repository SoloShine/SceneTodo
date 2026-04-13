# 数据库检查和修复脚本
# 用于检查 Tags 表是否存在，如果不存在则创建

$dbPath = "$env:LOCALAPPDATA\SceneTodo\todo.db"

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "数据库检查和修复工具" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# 检查数据库文件是否存在
if (Test-Path $dbPath) {
    Write-Host "? 数据库文件存在: $dbPath" -ForegroundColor Green
    
    # 显示数据库大小
    $size = (Get-Item $dbPath).Length / 1KB
    Write-Host "  大小: $([math]::Round($size, 2)) KB" -ForegroundColor Gray
} else {
    Write-Host "? 数据库文件不存在: $dbPath" -ForegroundColor Red
    Write-Host "  请先运行应用程序创建数据库" -ForegroundColor Yellow
    exit
}

Write-Host ""
Write-Host "正在检查数据库表结构..." -ForegroundColor Cyan

# 使用 System.Data.SQLite 检查表
Add-Type -Path "System.Data.SQLite.dll" -ErrorAction SilentlyContinue

try {
    # 创建数据库连接
    $connectionString = "Data Source=$dbPath;Version=3;"
    $connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)
    $connection.Open()
    
    # 检查 Tags 表是否存在
    $checkTableCmd = $connection.CreateCommand()
    $checkTableCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Tags';"
    $result = $checkTableCmd.ExecuteScalar()
    
    if ($result) {
        Write-Host "? Tags 表存在" -ForegroundColor Green
        
        # 检查表结构
        $schemaCmd = $connection.CreateCommand()
        $schemaCmd.CommandText = "PRAGMA table_info(Tags);"
        $reader = $schemaCmd.ExecuteReader()
        
        Write-Host "  表结构:" -ForegroundColor Gray
        while ($reader.Read()) {
            $name = $reader["name"]
            $type = $reader["type"]
            $notnull = $reader["notnull"]
            $pk = $reader["pk"]
            
            $info = "    - $name ($type)"
            if ($notnull -eq 1) { $info += " NOT NULL" }
            if ($pk -eq 1) { $info += " PRIMARY KEY" }
            
            Write-Host $info -ForegroundColor Gray
        }
        $reader.Close()
        
        # 统计标签数量
        $countCmd = $connection.CreateCommand()
        $countCmd.CommandText = "SELECT COUNT(*) FROM Tags;"
        $count = $countCmd.ExecuteScalar()
        Write-Host "  当前标签数量: $count" -ForegroundColor Gray
        
    } else {
        Write-Host "? Tags 表不存在" -ForegroundColor Red
        Write-Host "  正在创建 Tags 表..." -ForegroundColor Yellow
        
        # 创建 Tags 表
        $createTableCmd = $connection.CreateCommand()
        $createTableCmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Tags (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Color TEXT DEFAULT '#2196F3',
    CreatedAt TEXT
);
"@
        $createTableCmd.ExecuteNonQuery()
        Write-Host "? Tags 表创建成功" -ForegroundColor Green
    }
    
    # 检查 TodoItemTags 表
    $checkTableCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='TodoItemTags';"
    $result = $checkTableCmd.ExecuteScalar()
    
    if ($result) {
        Write-Host "? TodoItemTags 表存在" -ForegroundColor Green
    } else {
        Write-Host "? TodoItemTags 表不存在" -ForegroundColor Red
        Write-Host "  正在创建 TodoItemTags 表..." -ForegroundColor Yellow
        
        $createTableCmd = $connection.CreateCommand()
        $createTableCmd.CommandText = @"
CREATE TABLE IF NOT EXISTS TodoItemTags (
    Id TEXT PRIMARY KEY,
    TodoItemId TEXT NOT NULL,
    TagId TEXT NOT NULL,
    UNIQUE(TodoItemId, TagId)
);
"@
        $createTableCmd.ExecuteNonQuery()
        Write-Host "? TodoItemTags 表创建成功" -ForegroundColor Green
    }
    
    $connection.Close()
    
    Write-Host ""
    Write-Host "======================================" -ForegroundColor Cyan
    Write-Host "检查完成！" -ForegroundColor Green
    Write-Host "======================================" -ForegroundColor Cyan
    
} catch {
    Write-Host "? 检查失败: $_" -ForegroundColor Red
    Write-Host "  请确保应用程序未在运行" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "按任意键退出..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
