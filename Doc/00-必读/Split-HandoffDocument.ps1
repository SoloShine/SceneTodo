# 交接文档分割脚本
# 将交接文档-最新版-v3.1-备份.md分割为5个Part

$backupFile = "Doc\00-必读\交接文档-最新版-v3.1-备份.md"
$content = Get-Content $backupFile -Raw

Write-Host "开始分割交接文档..." -ForegroundColor Cyan
Write-Host "总行数: $((Get-Content $backupFile).Count)" -ForegroundColor Yellow
Write-Host ""

# 定义分割点
$parts = @(
    @{
        Name = "Part1-项目概览与进度"
        Title = "Part 1: 项目概览与进度"
        StartLine = 1
        EndLine = 170
        Chapters = "1-3章"
        Description = "项目基本信息、核心特性、开发进度"
    },
    @{
        Name = "Part2-技术架构与结构"
        Title = "Part 2: 技术架构与结构"
        StartLine = 171
        EndLine = 365
        Chapters = "4-5章"
        Description = "开发会话历史、技术架构、数据库设计"
    },
    @{
        Name = "Part3-核心功能详解"
        Title = "Part 3: 核心功能详解"
        StartLine = 366
        EndLine = 608
        Chapters = "6-7章"
        Description = "项目结构详解、核心功能说明"
    },
    @{
        Name = "Part4-问题与规划"
        Title = "Part 4: 问题与规划"
        StartLine = 609
        EndLine = 856
        Chapters = "8-12章"
        Description = "已知问题、下一步工作、文档体系、开发环境"
    },
    @{
        Name = "Part5-开发指南与资源"
        Title = "Part 5: 开发指南与资源"
        StartLine = 857
        EndLine = 1002
        Chapters = "13-20章"
        Description = "问题排查、项目统计、交接清单、新手建议"
    }
)

$allLines = Get-Content $backupFile

$partIndex = 1
foreach ($part in $parts) {
    Write-Host "创建 $($part.Title)..." -ForegroundColor Yellow
    
    # 提取内容
    $startIdx = $part.StartLine - 1
    $endIdx = $part.EndLine - 1
    $partLines = $allLines[$startIdx..$endIdx]
    
    # 构建导航
    $prevLink = if ($partIndex -eq 1) { "" } else { " | [上一部分: Part $($partIndex-1) ←](交接文档-Part$($partIndex-1)-*.md)" }
    $nextLink = if ($partIndex -eq 5) { "" } else { " | [下一部分: Part $($partIndex+1) →](交接文档-Part$($partIndex+1)-*.md)" }
    
    # 生成Part文件头部
    $partHeader = @"
# ?? SceneTodo 项目交接文档 - $($part.Title)

> **创建日期**: 2026-01-02 18:43:00  
> **最后更新**: 2026-01-02 22:16:53  
> **版本**: v3.2  
> **文档状态**: 正式发布  
> **所属文档**: 项目交接文档  
> **部分编号**: Part $partIndex / 5

---

## ?? 导航

?? [返回总索引](交接文档-最新版.md)$prevLink$nextLink

---

## ?? 本部分内容

本部分包含第$($part.Chapters)：
$($part.Description)

---


"@
    
    # 组合完整内容
    $fullContent = $partHeader + ($partLines -join "`n")
    
    # 添加底部导航
    if ($partIndex -lt 5) {
        $fullContent += "`n`n---`n`n**?? 继续阅读**: [下一部分: Part $($partIndex+1) →](交接文档-Part$($partIndex+1)-*.md)`n"
    } else {
        $fullContent += "`n`n---`n`n**? 文档完成！** 返回 [总索引](交接文档-最新版.md)`n"
    }
    
    # 保存文件
    $outputFile = "Doc\00-必读\交接文档-$($part.Name).md"
    Set-Content -Path $outputFile -Value $fullContent -Encoding UTF8
    
    $lines = ($fullContent -split "`n").Count
    Write-Host "  ? 已创建: 交接文档-$($part.Name).md ($lines 行)" -ForegroundColor Green
    
    $partIndex++
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "? 交接文档分割完成！" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "已创建文件:" -ForegroundColor White
Get-ChildItem "Doc\00-必读" -Filter "交接文档-Part*.md" | ForEach-Object {
    $lines = (Get-Content $_.FullName).Count
    Write-Host "  - $($_.Name) ($lines 行)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "验证文档长度..." -ForegroundColor Cyan
$allParts = Get-ChildItem "Doc\00-必读" -Filter "交接文档-Part*.md"
$maxLines = ($allParts | ForEach-Object { (Get-Content $_.FullName).Count } | Measure-Object -Maximum).Maximum
if ($maxLines -lt 800) {
    Write-Host "? 所有Part文档 < 800行（安全）" -ForegroundColor Green
    Write-Host "   最大文档: $maxLines 行" -ForegroundColor Gray
} else {
    Write-Host "?? 警告: 存在 >800行的文档" -ForegroundColor Yellow
    Write-Host "   最大文档: $maxLines 行" -ForegroundColor Yellow
}
