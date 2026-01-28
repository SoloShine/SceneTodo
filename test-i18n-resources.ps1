# 国际化资源测试脚本

Write-Host "=== 国际化资源文件测试 ===" -ForegroundColor Cyan

# 检查资源文件是否存在
Write-Host "`n1. 检查资源文件..." -ForegroundColor Yellow
$resxFiles = @(
    "Resources\Strings.resx",
    "Resources\Strings.en.resx"
)

foreach ($file in $resxFiles) {
    if (Test-Path $file) {
        Write-Host "? $file 存在" -ForegroundColor Green
        
        # 检查资源数量
        $content = Get-Content $file -Raw
        $dataCount = ([regex]::Matches($content, '<data name=')).Count
        Write-Host "  - 包含 $dataCount 个资源键" -ForegroundColor Gray
    } else {
        Write-Host "? $file 不存在" -ForegroundColor Red
    }
}

# 检查关键资源键
Write-Host "`n2. 检查关键资源键..." -ForegroundColor Yellow
$keyNames = @(
    "Menu_Settings",
    "Settings_Language",
    "Settings_Appearance",
    "Common_Save",
    "Common_Cancel"
)

foreach ($key in $keyNames) {
    $found = Select-String -Path "Resources\Strings.resx" -Pattern "<data name=`"$key`"" -Quiet
    if ($found) {
        Write-Host "? $key" -ForegroundColor Green
    } else {
        Write-Host "? $key 缺失" -ForegroundColor Red
    }
}

# 检查英文资源
Write-Host "`n3. 检查英文资源..." -ForegroundColor Yellow
foreach ($key in $keyNames) {
    $found = Select-String -Path "Resources\Strings.en.resx" -Pattern "<data name=`"$key`"" -Quiet
    if ($found) {
        Write-Host "? $key (EN)" -ForegroundColor Green
    } else {
        Write-Host "? $key (EN) 缺失" -ForegroundColor Red
    }
}

# 检查项目文件配置
Write-Host "`n4. 检查项目文件配置..." -ForegroundColor Yellow
$projContent = Get-Content "SceneTodo.csproj" -Raw
if ($projContent -match "EmbeddedResource.*Strings\.resx") {
    Write-Host "? 资源文件已配置为嵌入式资源" -ForegroundColor Green
} else {
    Write-Host "? 资源文件未配置为嵌入式资源" -ForegroundColor Red
}

# 检查编译输出
Write-Host "`n5. 检查编译输出..." -ForegroundColor Yellow
$dllPath = "bin\Debug\net8.0-windows\SceneTodo.dll"
if (Test-Path $dllPath) {
    Write-Host "? 主程序集存在: $dllPath" -ForegroundColor Green
    
    # 检查资源是否嵌入
    try {
        $assembly = [System.Reflection.Assembly]::LoadFile((Resolve-Path $dllPath))
        $resourceNames = $assembly.GetManifestResourceNames()
        
        Write-Host "`n  嵌入的资源:" -ForegroundColor Gray
        $resourceNames | Where-Object { $_ -like "*Strings*" } | ForEach-Object {
            Write-Host "  - $_" -ForegroundColor Cyan
        }
        
        if ($resourceNames -like "*Strings*") {
            Write-Host "`n? 资源文件已正确嵌入程序集" -ForegroundColor Green
        } else {
            Write-Host "`n? 资源文件未嵌入程序集" -ForegroundColor Red
        }
    } catch {
        Write-Host "? 无法加载程序集: $_" -ForegroundColor Red
    }
} else {
    Write-Host "? 主程序集不存在，请先编译项目" -ForegroundColor Red
}

Write-Host "`n=== 测试完成 ===" -ForegroundColor Cyan
