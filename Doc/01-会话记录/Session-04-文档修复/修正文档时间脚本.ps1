# SceneTodo 文档时间修正脚本
# 用途: 修正文档中的占位符时间（2025-01-XX）
# 使用方法: 在PowerShell中执行此脚本

Write-Host "=== SceneTodo 文档时间修正脚本 ===" -ForegroundColor Cyan
Write-Host ""

# 获取当前真实时间
$currentTime = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
Write-Host "当前系统时间: $currentTime" -ForegroundColor Green
Write-Host ""

# 设置项目根目录
$ProjectRoot = "D:\Documents\GitHub\SceneTodo"
$DocRoot = Join-Path $ProjectRoot "Doc"

# 检查目录是否存在
if (-not (Test-Path $DocRoot)) {
    Write-Host "错误: 找不到Doc目录: $DocRoot" -ForegroundColor Red
    exit 1
}

Set-Location $DocRoot
Write-Host "当前目录: $DocRoot" -ForegroundColor Green
Write-Host ""

# ====================================
# 时间修正说明
# ====================================
Write-Host "?? 重要说明:" -ForegroundColor Yellow
Write-Host "本脚本将把文档中的占位符时间（2025-01-XX）替换为真实时间。" -ForegroundColor Gray
Write-Host "由于这些文档是在2026-01-02创建的，我们将使用该日期。" -ForegroundColor Gray
Write-Host ""

Write-Host "时间修正规则:" -ForegroundColor Yellow
Write-Host "  1. Session-01文档: 使用 2026-01-01 (估算)" -ForegroundColor Gray
Write-Host "  2. Session-02文档: 使用 2026-01-01 (估算)" -ForegroundColor Gray
Write-Host "  3. Session-03文档: 使用 2026-01-02 (估算)" -ForegroundColor Gray
Write-Host "  4. 今天创建的文档: 使用 2026-01-02 18:16:05" -ForegroundColor Gray
Write-Host ""

# 询问用户是否继续
$confirmation = Read-Host "是否继续修正时间？(Y/N)"
if ($confirmation -ne 'Y' -and $confirmation -ne 'y') {
    Write-Host "操作已取消" -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "开始修正文档时间..." -ForegroundColor Yellow
Write-Host ""

# ====================================
# 函数: 替换文件中的占位符时间
# ====================================
function Replace-PlaceholderTime {
    param(
        [string]$FilePath,
        [string]$ReplaceTime
    )
    
    if (Test-Path $FilePath) {
        $content = Get-Content $FilePath -Raw -Encoding UTF8
        $originalContent = $content
        
        # 替换各种占位符格式
        $content = $content -replace '2025-01-XX', $ReplaceTime
        $content = $content -replace 'YYYY-MM-DD HH:mm:ss', "$ReplaceTime"
        $content = $content -replace 'YYYY-MM-DD HH:MM:SS', "$ReplaceTime"
        
        # 如果内容有变化，则写回文件
        if ($content -ne $originalContent) {
            Set-Content $FilePath $content -Encoding UTF8
            return $true
        }
    }
    return $false
}

# ====================================
# 修正核心文档（今天创建）
# ====================================
$todayTime = "2026-01-02 18:16:05"
$coreFiles = @(
    "00-必读\README.md",
    "00-必读\项目状态总览.md",
    "00-必读\交接文档-最新版.md",
    "00-必读\快速上手指南.md",
    "00-必读\文档整理完成报告.md",
    "本次文档整理会话总结.md",
    "文档整理与归档方案.md"
)

Write-Host "修正核心文档（使用: $todayTime）..." -ForegroundColor Yellow
$count = 0
foreach ($file in $coreFiles) {
    if (Replace-PlaceholderTime -FilePath $file -ReplaceTime $todayTime) {
        Write-Host "  ? $file" -ForegroundColor Green
        $count++
    }
}
Write-Host "完成! 修正了 $count 个文件" -ForegroundColor Green
Write-Host ""

# ====================================
# 添加估算时间标注
# ====================================
Write-Host "为历史会话文档添加估算标注..." -ForegroundColor Yellow

# Session-01文档
$session01Files = Get-ChildItem "01-会话记录\Session-01-P0核心功能实现" -Filter "*.md" -File
foreach ($file in $session01Files) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    
    # 如果还有占位符，替换为估算时间
    if ($content -match '2025-01-XX') {
        $content = $content -replace '2025-01-XX', '2026-01-01 10:00:00 [估算]'
        Set-Content $file.FullName $content -Encoding UTF8
        Write-Host "  ? Session-01/$($file.Name)" -ForegroundColor Gray
    }
}

# Session-02文档
$session02Files = Get-ChildItem "01-会话记录\Session-02-UI优化" -Filter "*.md" -File
foreach ($file in $session02Files) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    
    if ($content -match '2025-01-XX') {
        $content = $content -replace '2025-01-XX', '2026-01-01 14:00:00 [估算]'
        Set-Content $file.FullName $content -Encoding UTF8
        Write-Host "  ? Session-02/$($file.Name)" -ForegroundColor Gray
    }
}

# Session-03文档
$session03Files = Get-ChildItem "01-会话记录\Session-03-右键菜单改进" -Filter "*.md" -File
foreach ($file in $session03Files) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    
    if ($content -match '2025-01-XX') {
        $content = $content -replace '2025-01-XX', '2026-01-02 10:00:00 [估算]'
        Set-Content $file.FullName $content -Encoding UTF8
        Write-Host "  ? Session-03/$($file.Name)" -ForegroundColor Gray
    }
}

Write-Host "完成!" -ForegroundColor Green
Write-Host ""

# ====================================
# 统计修正结果
# ====================================
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "?? 时间修正完成！" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "修正说明:" -ForegroundColor Yellow
Write-Host "  ? 核心文档: 使用真实创建时间（$todayTime）" -ForegroundColor Gray
Write-Host "  ?? 历史文档: 使用估算时间并标注 [估算]" -ForegroundColor Gray
Write-Host ""

Write-Host "?? 提示:" -ForegroundColor Yellow
Write-Host "  - 今后创建文档时，请使用真实系统时间" -ForegroundColor Gray
Write-Host "  - 执行命令: Get-Date -Format 'yyyy-MM-dd HH:mm:ss'" -ForegroundColor Gray
Write-Host "  - 参考文档: Doc/00-必读/文档编写规范.md" -ForegroundColor Gray
Write-Host ""

Write-Host "?? 时间修正脚本执行完成！" -ForegroundColor Cyan
Write-Host ""
