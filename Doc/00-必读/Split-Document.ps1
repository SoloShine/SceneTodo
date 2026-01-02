# 文档分割脚本
# 用途: 将文档编写规范-完整版-v1.2-备份.md 分割为4个Part文件

$backupFile = "文档编写规范-完整版-v1.2-备份.md"
$content = Get-Content $backupFile -Raw

# 定义分割点（基于章节标题）
$parts = @{
    "Part1" = @{
        Name = "Part1-基础规范"
        Title = "Part 1: 基础规范"
        StartPattern = "## 1\. 文档命名规范"
        EndPattern = "## 6\. 内容编写规范"
        Chapters = "第1-5章"
        Description = @"
- 第1章: 文档命名规范
- 第2章: 文档结构规范
- 第3章: 时间处理规范 ??????（强制性规范）
- 第4章: 文档版本管理
- 第5章: Markdown格式规范
"@
    }
    "Part2" = @{
        Name = "Part2-高级规范"
        Title = "Part 2: 高级规范"
        StartPattern = "## 6\. 内容编写规范"
        EndPattern = "## 11\. 文档归档规范"
        Chapters = "第6-10章"
        Description = @"
- 第6章: 内容编写规范
- 第7章: 文档分类与存放
- 第8章: 文档更新规范
- 第9章: 质量检查清单
- 第10章: 特殊场景处理（包含10.4超长文档分割规范）
"@
    }
    "Part3" = @{
        Name = "Part3-归档与模板"
        Title = "Part 3: 归档与模板"
        StartPattern = "## 11\. 文档归档规范"
        EndPattern = "## 13\. 常见问题"
        Chapters = "第11-12章"
        Description = @"
- 第11章: 文档归档规范 ??????（强制性规范）
- 第12章: 示例模板
  - 12.1 功能文档模板
  - 12.2 会话交接文档模板
"@
    }
    "Part4" = @{
        Name = "Part4-FAQ与执行"
        Title = "Part 4: FAQ与执行"
        StartPattern = "## 13\. 常见问题"
        EndPattern = "维护者: SceneTodo Team"
        Chapters = "第13-15章"
        Description = @"
- 第13章: 常见问题（8个Q&A）
- 第14章: 规范执行
  - 14.1 强制性规范（必须遵守）
  - 14.2 建议性规范（推荐遵守）
  - 14.3 违反规范的处理
- 第15章: 规范更新（包含完整版本历史）
"@
    }
}

# 提取文档头部（元数据）
$headerPattern = "(?s)^(.*?)(?=## 1\.)"
$headerMatch = [regex]::Match($content, $headerPattern)
$originalHeader = $headerMatch.Value

Write-Host "开始分割文档..." -ForegroundColor Cyan
Write-Host ""

$partIndex = 1
foreach ($partKey in @("Part1", "Part2", "Part3", "Part4")) {
    $part = $parts[$partKey]
    
    Write-Host "创建 $($part.Title)..." -ForegroundColor Yellow
    
    # 提取内容
    $startIdx = $content.IndexOf($part.StartPattern)
    if ($part.EndPattern -match "维护者") {
        $endIdx = $content.Length
    } else {
        $endIdx = $content.IndexOf($part.EndPattern)
    }
    
    if ($startIdx -lt 0 -or $endIdx -lt 0) {
        Write-Host "  ? 错误: 无法找到章节边界" -ForegroundColor Red
        continue
    }
    
    $partContent = $content.Substring($startIdx, $endIdx - $startIdx)
    
    # 构建导航
    $prevLink = if ($partIndex -eq 1) { "" } else { " | [上一部分: Part $($partIndex-1) ←](文档编写规范-Part$($partIndex-1)-*.md)" }
    $nextLink = if ($partIndex -eq 4) { "" } else { " | [下一部分: Part $($partIndex+1) →](文档编写规范-Part$($partIndex+1)-*.md)" }
    
    # 生成Part文件头部
    $partHeader = @"
# ?? SceneTodo 文档编写规范 - $($part.Title)

> **创建日期**: 2026-01-02 18:16:05  
> **最后更新**: 2026-01-02 22:04:47  
> **版本**: 1.3  
> **状态**: 正式发布  
> **所属文档**: 文档编写规范  
> **部分编号**: Part $partIndex / 4

---

## ?? 导航

?? [返回总索引](文档编写规范.md)$prevLink$nextLink

---

## ?? 本部分内容

本部分包含$($part.Chapters)：
$($part.Description)

---


"@
    
    # 组合完整内容
    $fullContent = $partHeader + $partContent
    
    # 添加底部导航
    if ($partIndex -lt 4) {
        $fullContent += "`n`n---`n`n**?? 继续阅读**: [下一部分: Part $($partIndex+1) →](文档编写规范-Part$($partIndex+1)-*.md)`n"
    } else {
        $fullContent += "`n`n---`n`n**? 文档完成！** 返回 [总索引](文档编写规范.md)`n"
    }
    
    # 保存文件
    $outputFile = "文档编写规范-$($part.Name).md"
    Set-Content -Path $outputFile -Value $fullContent -Encoding UTF8
    
    $lines = ($fullContent -split "`n").Count
    Write-Host "  ? 已创建: $outputFile ($lines 行)" -ForegroundColor Green
    
    $partIndex++
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "? 文档分割完成！" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "已创建文件:" -ForegroundColor White
Get-ChildItem "." -Filter "文档编写规范-Part*.md" | ForEach-Object {
    $lines = (Get-Content $_.FullName).Count
    Write-Host "  - $($_.Name) ($lines 行)" -ForegroundColor Gray
}
