# 图标名称验证和修复脚本

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "  SceneTodo 图标名称验证工具" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# 定义项目根目录
$projectRoot = $PSScriptRoot

# 获取所有 XAML 文件
$xamlFiles = Get-ChildItem -Path $projectRoot -Filter "*.xaml" -Recurse -File | Where-Object { $_.FullName -notmatch "\\obj\\" -and $_.FullName -notmatch "\\bin\\" }

Write-Host "找到 $($xamlFiles.Count) 个 XAML 文件" -ForegroundColor Green
Write-Host ""

# MaterialDesign 图标的常见错误映射
$materialDesignFixes = @{
    "Plus" = "Plus"
    "Refresh" = "Refresh" 
    "PencilOutline" = "PencilOutline"
    "DeleteOutline" = "DeleteOutline"
    "FilterOutline" = "FilterOutline"
    "Settings" = "Settings"
    "Cog" = "Cogs"  # 常见错误
    "Cogs" = "Cogs"
    "Pencil" = "PencilOutline"  # 简化版本
    "Delete" = "DeleteOutline"  # 简化版本
    "Filter" = "FilterOutline"  # 简化版本
}

# FontAwesome 图标的常见错误映射
$fontAwesomeFixes = @{
    "PlusSolid" = "PlusRegular"  # FontAwesome 5+ 使用 Regular/Solid 后缀
    "EditSolid" = "EditRegular"
    "TrashAltSolid" = "TrashAltRegular"
    "RedoSolid" = "RedoRegular"
    "PlusCircleSolid" = "PlusCircleRegular"
}

$issuesFound = @()
$totalIssues = 0

foreach ($file in $xamlFiles) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $lineNumber = 0
    $lines = Get-Content $file.FullName -Encoding UTF8
    
    foreach ($line in $lines) {
        $lineNumber++
        
        # 检查 MaterialDesign 图标
        if ($line -match 'PackIconMaterialDesign\s+Kind="([^"]+)"') {
            $iconName = $Matches[1]
            
            # 检查是否是已知的错误图标
            if ($materialDesignFixes.ContainsKey($iconName) -and $materialDesignFixes[$iconName] -ne $iconName) {
                $correctName = $materialDesignFixes[$iconName]
                $issuesFound += [PSCustomObject]@{
                    File = $file.Name
                    Line = $lineNumber
                    Type = "MaterialDesign"
                    Current = $iconName
                    Suggested = $correctName
                    Content = $line.Trim()
                }
                $totalIssues++
            }
        }
        
        # 检查 FontAwesome 图标
        if ($line -match 'PackIconFontAwesome\s+Kind="([^"]+)"') {
            $iconName = $Matches[1]
            
            # 检查是否是已知的错误图标
            if ($fontAwesomeFixes.ContainsKey($iconName)) {
                $correctName = $fontAwesomeFixes[$iconName]
                $issuesFound += [PSCustomObject]@{
                    File = $file.Name
                    Line = $lineNumber
                    Type = "FontAwesome"
                    Current = $iconName
                    Suggested = $correctName
                    Content = $line.Trim()
                }
                $totalIssues++
            }
        }
    }
}

if ($totalIssues -eq 0) {
    Write-Host "? 未发现图标名称问题！" -ForegroundColor Green
} else {
    Write-Host "? 发现 $totalIssues 个图标名称问题：" -ForegroundColor Red
    Write-Host ""
    
    $issuesFound | Format-Table -AutoSize
    
    Write-Host ""
    Write-Host "建议修复方案：" -ForegroundColor Yellow
    Write-Host "1. MaterialDesign 图标统一使用 Outline 后缀版本" -ForegroundColor Yellow
    Write-Host "2. FontAwesome 图标改为使用 MaterialDesign 图标" -ForegroundColor Yellow
    Write-Host "3. 'Cog' 改为 'Cogs' 或 'Settings'" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "  验证完成" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
