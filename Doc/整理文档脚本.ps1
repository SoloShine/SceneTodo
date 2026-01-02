# SceneTodo 文档整理脚本
# 用途: 将现有文档按会话归档到新的目录结构
# 使用方法: 在PowerShell中执行此脚本

Write-Host "=== SceneTodo 文档整理脚本 ===" -ForegroundColor Cyan
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
# 第1步: 创建会话目录
# ====================================
Write-Host "第1步: 创建会话目录..." -ForegroundColor Yellow

$Session01 = "01-会话记录\Session-01-P0核心功能实现"
$Session02 = "01-会话记录\Session-02-UI优化"
$Session03 = "01-会话记录\Session-03-右键菜单改进"

New-Item -Path $Session01 -ItemType Directory -Force | Out-Null
New-Item -Path $Session02 -ItemType Directory -Force | Out-Null
New-Item -Path $Session03 -ItemType Directory -Force | Out-Null
New-Item -Path "02-功能文档\历史记录" -ItemType Directory -Force | Out-Null
New-Item -Path "Archive" -ItemType Directory -Force | Out-Null

Write-Host "? 会话目录创建完成" -ForegroundColor Green
Write-Host ""

# ====================================
# 第2步: 移动Session-01文档
# ====================================
Write-Host "第2步: 移动Session-01文档（P0核心功能实现）..." -ForegroundColor Yellow

$Session01Files = @(
    "会话交接文档.md",
    "开发进度日志.md",
    "开发任务总览.md",
    "P0功能实现状态分析.md",
    "P0核心功能实现指南.md",
    "数据库Schema变更清单.md"
)

$moved = 0
foreach ($file in $Session01Files) {
    if (Test-Path $file) {
        if ($file -eq "会话交接文档.md") {
            Move-Item $file "$Session01\会话交接文档-v1.0.md" -Force
        } else {
            Move-Item $file $Session01 -Force
        }
        $moved++
        Write-Host "  ? $file" -ForegroundColor Gray
    } else {
        Write-Host "  ? 未找到: $file" -ForegroundColor DarkGray
    }
}

Write-Host "? Session-01: 移动了 $moved 个文件" -ForegroundColor Green
Write-Host ""

# ====================================
# 第3步: 移动Session-02文档
# ====================================
Write-Host "第3步: 移动Session-02文档（UI优化）..." -ForegroundColor Yellow

$Session02Files = @(
    "UI优化说明-待办项和编辑窗口.md",
    "UI优化快速测试指南.md",
    "UI优化任务完成总结.md",
    "功能说明-主页显示优化和应用绑定继承.md",
    "测试指南-主页显示优化和应用绑定继承.md",
    "会话完成总结-显示优化和绑定继承.md"
)

$moved = 0
foreach ($file in $Session02Files) {
    if (Test-Path $file) {
        Move-Item $file $Session02 -Force
        $moved++
        Write-Host "  ? $file" -ForegroundColor Gray
    } else {
        Write-Host "  ? 未找到: $file" -ForegroundColor DarkGray
    }
}

Write-Host "? Session-02: 移动了 $moved 个文件" -ForegroundColor Green
Write-Host ""

# ====================================
# 第4步: 移动Session-03文档
# ====================================
Write-Host "第4步: 移动Session-03文档（右键菜单改进）..." -ForegroundColor Yellow

$Session03Files = @(
    "会话交接文档-v2.0-右键菜单改进.md",
    "本次会话修改记录清单.md",
    "本次会话总结报告.md",
    "本次会话文档索引.md",
    "重大改进-右键菜单替代悬停按钮.md",
    "Bug修复说明-操作按钮悬停消失问题.md",
    "修复完成摘要-操作按钮悬停问题.md",
    "测试指南-右键菜单功能.md",
    "测试清单-操作按钮悬停修复.md",
    "完成摘要-右键菜单替代悬停按钮.md",
    "快速参考卡片-右键菜单改进.md",
    "用户使用指南-右键菜单新功能.md"
)

$moved = 0
foreach ($file in $Session03Files) {
    if (Test-Path $file) {
        Move-Item $file $Session03 -Force
        $moved++
        Write-Host "  ? $file" -ForegroundColor Gray
    } else {
        Write-Host "  ? 未找到: $file" -ForegroundColor DarkGray
    }
}

Write-Host "? Session-03: 移动了 $moved 个文件" -ForegroundColor Green
Write-Host ""

# ====================================
# 第5步: 移动独立文档
# ====================================
Write-Host "第5步: 移动独立文档..." -ForegroundColor Yellow

# PRD文档
if (Test-Path "桌面应用待办软件产品需求规格说明书（PRD）初稿.md") {
    Move-Item "桌面应用待办软件产品需求规格说明书（PRD）初稿.md" "06-规划文档\PRD-初稿.md" -Force
    Write-Host "  ? PRD文档" -ForegroundColor Gray
}

# 历史记录文档
if (Test-Path "测试说明-历史记录功能.md") {
    Move-Item "测试说明-历史记录功能.md" "02-功能文档\历史记录\" -Force
    Write-Host "  ? 历史记录测试文档" -ForegroundColor Gray
}

# 文档整理方案（保留在根目录）
Write-Host "  ? 文档整理方案保留在根目录" -ForegroundColor Gray

Write-Host "? 独立文档移动完成" -ForegroundColor Green
Write-Host ""

# ====================================
# 第6步: 在旧文档中添加过时提示
# ====================================
Write-Host "第6步: 添加过时提示到旧文档..." -ForegroundColor Yellow

# Session-01的P0功能实现状态分析
$oldStatusFile = "$Session01\P0功能实现状态分析.md"
if (Test-Path $oldStatusFile) {
    $content = Get-Content $oldStatusFile -Raw
    $warning = @"
> ?? **[已过时 - 归档文档]**  
> **创建日期**: Session-01  
> **状态**: 本文档标记的很多功能已在后续会话中实现  
> **请参考**: ``Doc/00-必读/项目状态总览.md`` 获取最新状态  
> **用途**: 仅供了解Session-01时的项目状态  

---

"@
    $newContent = $warning + $content
    Set-Content $oldStatusFile $newContent -Encoding UTF8
    Write-Host "  ? P0功能实现状态分析.md" -ForegroundColor Gray
}

Write-Host "? 过时提示添加完成" -ForegroundColor Green
Write-Host ""

# ====================================
# 第7步: 显示结果
# ====================================
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "?? 文档整理完成！" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "新的文档结构:" -ForegroundColor Yellow
Write-Host ""
Write-Host "Doc/" -ForegroundColor White
Write-Host "├── 00-必读/                    ← 从这里开始！" -ForegroundColor Green
Write-Host "│   ├── README.md" -ForegroundColor Gray
Write-Host "│   ├── 项目状态总览.md" -ForegroundColor Gray
Write-Host "│   ├── 交接文档-最新版.md" -ForegroundColor Gray
Write-Host "│   └── 快速上手指南.md" -ForegroundColor Gray
Write-Host "├── 01-会话记录/" -ForegroundColor White
Write-Host "│   ├── Session-01-P0核心功能实现/" -ForegroundColor Gray
Write-Host "│   ├── Session-02-UI优化/" -ForegroundColor Gray
Write-Host "│   └── Session-03-右键菜单改进/" -ForegroundColor Gray
Write-Host "├── 02-功能文档/" -ForegroundColor White
Write-Host "├── 03-测试文档/" -ForegroundColor White
Write-Host "├── 04-技术文档/" -ForegroundColor White
Write-Host "├── 05-用户文档/" -ForegroundColor White
Write-Host "└── 06-规划文档/" -ForegroundColor White
Write-Host ""

Write-Host "?? 查看归档的文档:" -ForegroundColor Yellow
Write-Host "  Session-01: 共 $($Session01Files.Count) 个文件" -ForegroundColor Gray
Write-Host "  Session-02: 共 $($Session02Files.Count) 个文件" -ForegroundColor Gray
Write-Host "  Session-03: 共 $($Session03Files.Count) 个文件" -ForegroundColor Gray
Write-Host ""

Write-Host "? 下一步:" -ForegroundColor Green
Write-Host "  1. 查看 Doc/00-必读/README.md" -ForegroundColor White
Write-Host "  2. 阅读 Doc/00-必读/项目状态总览.md" -ForegroundColor White
Write-Host "  3. 阅读 Doc/00-必读/交接文档-最新版.md" -ForegroundColor White
Write-Host ""

Write-Host "?? 文档整理脚本执行完成！" -ForegroundColor Cyan
Write-Host ""

# 显示统计
Write-Host "?? 文件统计:" -ForegroundColor Yellow
$totalFiles = (Get-ChildItem -Path $DocRoot -Recurse -File -Filter "*.md").Count
Write-Host "  总文档数: $totalFiles 个" -ForegroundColor Gray
Write-Host ""

# 提示Git操作
Write-Host "?? 建议执行Git操作:" -ForegroundColor Yellow
Write-Host "  git add Doc/" -ForegroundColor White
Write-Host "  git commit -m `"docs: 完成文档结构化整理和归档`"" -ForegroundColor White
Write-Host "  git push" -ForegroundColor White
Write-Host ""
