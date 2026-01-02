# 文档长度检查脚本
# 用途: 扫描项目中所有Markdown文档，检查是否需要分割
# 用法: .\Check-DocumentLength.ps1

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  SceneTodo 文档长度检查工具" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 定义限制标准
$limits = @{
    Safe = 600
    Warning = 800
    Danger = 1000
    Critical = 1200
}

# 定义颜色
$colors = @{
    Safe = "Green"
    Warning = "Yellow"
    Danger = "DarkYellow"
    Critical = "Red"
}

# 获取所有Markdown文档
$docPath = "Doc"
$mdFiles = Get-ChildItem -Path $docPath -Filter "*.md" -Recurse

Write-Host "?? 扫描目录: $docPath" -ForegroundColor Cyan
Write-Host "?? 找到文档: $($mdFiles.Count) 个" -ForegroundColor Cyan
Write-Host ""

# 分类统计
$stats = @{
    Safe = @()
    Warning = @()
    Danger = @()
    Critical = @()
}

# 检查每个文件
foreach ($file in $mdFiles) {
    $content = Get-Content $file.FullName
    $lines = $content.Count
    $chars = ($content | Measure-Object -Character).Characters
    $sizeKB = [Math]::Round($file.Length / 1KB, 2)
    
    $relativePath = $file.FullName.Replace((Get-Location).Path + "\", "")
    
    $fileInfo = @{
        Path = $relativePath
        Name = $file.Name
        Lines = $lines
        Chars = $chars
        SizeKB = $sizeKB
    }
    
    # 判断级别
    if ($lines -gt $limits.Critical) {
        $fileInfo.Level = "Critical"
        $fileInfo.Status = "?? 严重超限"
        $fileInfo.Action = "必须立即分割"
        $stats.Critical += $fileInfo
    }
    elseif ($lines -gt $limits.Danger) {
        $fileInfo.Level = "Danger"
        $fileInfo.Status = "?? 危险"
        $fileInfo.Action = "应该分割"
        $stats.Danger += $fileInfo
    }
    elseif ($lines -gt $limits.Warning) {
        $fileInfo.Level = "Warning"
        $fileInfo.Status = "?? 警告"
        $fileInfo.Action = "考虑分割"
        $stats.Warning += $fileInfo
    }
    else {
        $fileInfo.Level = "Safe"
        $fileInfo.Status = "?? 安全"
        $fileInfo.Action = "无需分割"
        $stats.Safe += $fileInfo
    }
}

# 显示统计结果
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  检查结果统计" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "?? 总体统计:" -ForegroundColor White
Write-Host "  ?? 安全 (<$($limits.Safe)行): $($stats.Safe.Count) 个" -ForegroundColor Green
Write-Host "  ?? 警告 ($($limits.Safe)-$($limits.Warning)行): $($stats.Warning.Count) 个" -ForegroundColor Yellow
Write-Host "  ?? 危险 ($($limits.Warning)-$($limits.Danger)行): $($stats.Danger.Count) 个" -ForegroundColor DarkYellow
Write-Host "  ?? 严重 (>$($limits.Danger)行): $($stats.Critical.Count) 个" -ForegroundColor Red
Write-Host ""

# 显示需要关注的文档
if ($stats.Critical.Count -gt 0) {
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "  ?? 严重超限文档（必须分割）" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    
    foreach ($file in $stats.Critical | Sort-Object Lines -Descending) {
        Write-Host "?? $($file.Name)" -ForegroundColor Red
        Write-Host "   路径: $($file.Path)" -ForegroundColor Gray
        Write-Host "   行数: $($file.Lines) 行 | 字符: $($file.Chars) | 大小: $($file.SizeKB) KB" -ForegroundColor Gray
        Write-Host "   操作: $($file.Action)" -ForegroundColor Red
        Write-Host ""
    }
}

if ($stats.Danger.Count -gt 0) {
    Write-Host "========================================" -ForegroundColor DarkYellow
    Write-Host "  ?? 危险文档（应该分割）" -ForegroundColor DarkYellow
    Write-Host "========================================" -ForegroundColor DarkYellow
    Write-Host ""
    
    foreach ($file in $stats.Danger | Sort-Object Lines -Descending) {
        Write-Host "?? $($file.Name)" -ForegroundColor DarkYellow
        Write-Host "   路径: $($file.Path)" -ForegroundColor Gray
        Write-Host "   行数: $($file.Lines) 行 | 字符: $($file.Chars) | 大小: $($file.SizeKB) KB" -ForegroundColor Gray
        Write-Host "   操作: $($file.Action)" -ForegroundColor DarkYellow
        Write-Host ""
    }
}

if ($stats.Warning.Count -gt 0) {
    Write-Host "========================================" -ForegroundColor Yellow
    Write-Host "  ?? 警告文档（考虑分割）" -ForegroundColor Yellow
    Write-Host "========================================" -ForegroundColor Yellow
    Write-Host ""
    
    foreach ($file in $stats.Warning | Sort-Object Lines -Descending) {
        Write-Host "?? $($file.Name)" -ForegroundColor Yellow
        Write-Host "   路径: $($file.Path)" -ForegroundColor Gray
        Write-Host "   行数: $($file.Lines) 行 | 字符: $($file.Chars) | 大小: $($file.SizeKB) KB" -ForegroundColor Gray
        Write-Host "   操作: $($file.Action)" -ForegroundColor Yellow
        Write-Host ""
    }
}

# 生成CSV报告
$reportPath = "Doc\文档长度检查报告.csv"
$allFiles = $stats.Critical + $stats.Danger + $stats.Warning + $stats.Safe
$allFiles | Select-Object Path, Lines, Chars, SizeKB, Status, Action | 
    Export-Csv -Path $reportPath -NoTypeInformation -Encoding UTF8

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  ? 检查完成" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "?? 详细报告已保存到: $reportPath" -ForegroundColor Green
Write-Host ""

# 显示建议
$needSplit = $stats.Critical.Count + $stats.Danger.Count
if ($needSplit -gt 0) {
    Write-Host "??  建议操作:" -ForegroundColor Yellow
    Write-Host "   1. 优先处理 ?? 严重超限 文档（$($stats.Critical.Count) 个）" -ForegroundColor Red
    Write-Host "   2. 其次处理 ?? 危险 文档（$($stats.Danger.Count) 个）" -ForegroundColor DarkYellow
    Write-Host "   3. 参考《文档编写规范.md》第10.4节进行分割" -ForegroundColor White
    Write-Host ""
}
else {
    Write-Host "? 所有文档长度正常，无需分割！" -ForegroundColor Green
    Write-Host ""
}

# 返回需要处理的文档数量
exit $needSplit
