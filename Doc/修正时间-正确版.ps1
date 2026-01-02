# 正确的时间修正脚本
# 只替换占位符，不改变文件编码

Write-Host "=== 修正文档占位符时间 ===" -ForegroundColor Cyan
Write-Host ""

$DocRoot = "D:\Documents\GitHub\SceneTodo\Doc"
Set-Location $DocRoot

# 定义替换时间
$todayTime = "2026-01-02 18:16:05"

# 要修正的文件列表
$files = @(
    "00-必读\README.md",
    "00-必读\项目状态总览.md",
    "00-必读\交接文档-最新版.md",
    "00-必读\快速上手指南.md"
)

Write-Host "将占位符 '2025-01-XX' 替换为: $todayTime" -ForegroundColor Yellow
Write-Host ""

foreach ($file in $files) {
    if (Test-Path $file) {
        Write-Host "处理: $file" -ForegroundColor Cyan
        
        # 读取文件（保留原始编码）
        $content = Get-Content $file -Raw -Encoding UTF8
        
        # 计算替换次数
        $count = ([regex]::Matches($content, "2025-01-XX")).Count
        
        if ($count -gt 0) {
            # 替换占位符
            $newContent = $content -replace '2025-01-XX', $todayTime
            
            # 写回文件（使用UTF-8无BOM）
            $utf8NoBom = New-Object System.Text.UTF8Encoding $false
            [System.IO.File]::WriteAllText((Resolve-Path $file).Path, $newContent, $utf8NoBom)
            
            Write-Host "  ? 替换了 $count 处" -ForegroundColor Green
        } else {
            Write-Host "  - 没有需要替换的内容" -ForegroundColor Gray
        }
    } else {
        Write-Host "  ? 文件不存在" -ForegroundColor Red
    }
    Write-Host ""
}

Write-Host "=== 完成 ===" -ForegroundColor Green
