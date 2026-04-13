# SceneTodo - Internationalization (i18n) Skill

> **Purpose**: 专门处理 SceneTodo 项目的国际化（多语言）实现
> **Context**: 基于 `Doc/00-必读/国际化使用指南.md`
> **适用场景**: 添加新功能、修改 UI 文本、支持新语言

---

## 🌍 项目国际化架构

### 资源文件结构
```
Resources/
├── Strings.resx          # 默认资源文件（中文）
└── Strings.en.resx       # 英文资源文件
```

### 核心组件
- **LocalizationService**: 文本本地化服务 (`Services/LocalizationService.cs`)
- **LocalizationExtension**: XAML 扩展（推荐使用）
- **SupportedLanguage**: 支持的语言枚举 (`Models/SupportedLanguage.cs`)
- **LanguageSettings**: 语言设置模型 (`Models/LanguageSettings.cs`)

---

## 📝 在 XAML 中使用国际化

### 方式 1: 使用 LocalizationExtension（推荐）✅

```xaml
<Window xmlns:services="clr-namespace:SceneTodo.Services">
    <Button Content="{services:Localization Common_Save}" />
    <TextBlock Text="{services:Localization Todo_Title}" />
</Window>
```

**优点**:
- 简洁明了
- 支持动态语言切换
- 自动刷新 UI

### 方式 2: 使用 Binding

```xaml
<Window xmlns:services="clr-namespace:SceneTodo.Services">
    <TextBlock Text="{Binding Source={x:Static services:LocalizationService.Instance},
                             Path=[Common_OK]}" />
</Window>
```

**适用场景**: 需要更复杂的绑定逻辑

### 方式 3: 使用转换器

```xaml
<Window xmlns:services="clr-namespace:SceneTodo.Services">
    <Window.Resources>
        <services:LocalizationConverter x:Key="LocalizationConverter" />
    </Window.Resources>

    <Button Content="{Binding Converter={StaticResource LocalizationConverter},
                            ConverterParameter=Common_Save}" />
</Window>
```

---

## 💻 在 C# 中使用国际化

### 获取本地化文本

```csharp
using SceneTodo.Services;

// 方式 1: 使用索引器
string title = LocalizationService.Instance["Todo_Title"];

// 方式 2: 使用 GetString 方法
string message = LocalizationService.Instance.GetString("Message_Success");

// 方式 3: 格式化字符串
string formatted = LocalizationService.Instance.GetString("Message_ItemCount", 10);
```

### 切换语言

```csharp
using SceneTodo.Models;
using SceneTodo.Services;

// 切换为英文
LocalizationService.Instance.ChangeLanguage(SupportedLanguage.English);

// 切换为简体中文
LocalizationService.Instance.ChangeLanguage(SupportedLanguage.ChineseSimplified);

// 自动检测系统语言
LocalizationService.Instance.AutoDetectLanguage();
```

### 监听语言变化

```csharp
LocalizationService.Instance.PropertyChanged += (s, e) =>
{
    if (e.PropertyName == "Item[]")
    {
        // 刷新所有绑定，重新加载UI
        UpdateUI();
    }
};
```

---

## ➕ 添加新的本地化文本

### 步骤 1: 在 Strings.resx 中添加中文

在 Visual Studio 中：
1. 打开 `Resources/Strings.resx`
2. 添加新的键值对，例如：
   - 名称: `NewFeature_Title`
   - 值: `新功能`

### 步骤 2: 在 Strings.en.resx 中添加英文

1. 打开 `Resources/Strings.en.resx`
2. 添加对应的英文翻译：
   - 名称: `NewFeature_Title`
   - 值: `New Feature`

### 步骤 3: 在代码中使用

```xaml
<TextBlock Text="{services:Localization NewFeature_Title}" />
```

或

```csharp
string title = LocalizationService.Instance["NewFeature_Title"];
```

---

## 📋 资源命名规范

为确保一致性，请遵循以下命名规范：

### 通用按钮
- `Common_*` (如 Common_Save, Common_Cancel, Common_OK, Common_Delete)
- `Common_Edit, Common_Add, Common_Search, Common_Filter`
- `Common_Clear, Common_Close, Common_Apply, Common_Reset`

### 窗口标题
- `*Window_Title` (如 MainWindow_Title, EditTodoItemWindow_Title)

### 待办事项
- `Todo_*` (如 Todo_Title, Todo_Description, Todo_Priority, Todo_DueDate)
- `Todo_Tags, Todo_Status, Todo_Create, Todo_Edit`

### 优先级
- `Priority_Low, Priority_Medium, Priority_High`

### 设置
- `Settings_*` (如 Settings_Title, Settings_Appearance, Settings_Language)
- `Settings_Theme, Settings_Opacity, Settings_Transparency`
- `Settings_Reminders, Settings_Shortcuts`

### 消息提示
- `Message_*` (如 Message_Success, Message_Error, Message_Warning)
- `Message_SaveSuccess, Message_SaveFailed`
- `Message_ConfirmDelete, Message_ConfirmAction`

### 标签
- `Tags_*` (如 Tags_Management, Tags_Add, Tags_Edit, Tags_Delete)
- `Tags_Filter, Tags_SelectAll, Tags_ClearSelection`

### 备份
- `Backup_*` (如 Backup_Create, Backup_Restore, Backup_Manage)
- `Backup_Export, Backup_Import, Backup_Success, Backup_Failed`

### 搜索和过滤
- `Search_*` (如 Search_Placeholder, Search_Results)
- `Filter_*` (如 Filter_Status, Filter_Priority, Filter_DateRange)
- `Filter_Advanced, Filter_Reset`

### 历史记录
- `History_*` (如 History_Title, History_Restore, History_Delete)
- `History_Empty, History_ClearAll`

### 日历视图
- `Calendar_*` (如 Calendar_Today, Calendar_PreviousMonth, Calendar_NextMonth)
- `Calendar_TaskCount, Calendar_NoTasks`

### 定时任务
- `ScheduledTask_*` (如 ScheduledTask_Title, ScheduledTask_CronExpression)
- `ScheduledTask_Enable, ScheduledTask_Disable`

---

## 🎨 语言设置窗口

```csharp
using SceneTodo.Models;
using SceneTodo.Views;

var settings = AppSettings.Load();
var languageWindow = new LanguageSettingsWindow(settings);
if (languageWindow.ShowDialog() == true)
{
    // 用户已更改语言设置
    MessageBox.Show("语言设置已保存，请重启应用生效");
}
```

---

## 📚 当前已有的本地化文本

### 通用
- Common_OK, Common_Cancel, Common_Save, Common_Delete
- Common_Edit, Common_Add, Common_Search, Common_Filter
- Common_Clear, Common_Close, Common_Apply, Common_Reset

### 主窗口
- MainWindow_TodoListView, MainWindow_CalendarView
- MainWindow_HistoryView, MainWindow_ScheduledTasks

### 待办事项
- Todo_Title, Todo_Description, Todo_Priority
- Todo_DueDate, Todo_Tags, Todo_Status

### 优先级
- Priority_Low, Priority_Medium, Priority_High

### 设置
- Settings_Title, Settings_Appearance, Settings_Language
- Settings_Theme, Settings_Opacity

### 消息提示
- Message_Success, Message_Error, Message_Warning
- Message_SaveSuccess, Message_SaveFailed

---

## 🔄 迁移硬编码的文本

项目中有大量硬编码的中文文本需要迁移到资源文件中。迁移时注意：

1. **按功能模块迁移**
2. **确保键名和值对应正确**
3. **测试切换语言后的显示效果**
4. **更新相关文档**

---

## 🌐 添加新语言支持

如需添加其他语言支持（如日语、法语等）：

1. 在 `Models/SupportedLanguage.cs` 中添加新的枚举值
2. 创建新的资源文件（如 `Resources/Strings.ja.resx`）
3. 在 `LocalizationService.cs` 中 `ChangeLanguage` 方法添加对应的 CultureInfo
4. 更新语言设置窗口显示新语言选项

---

## ⚠️ 注意事项

1. **切换后生效**: 语言切换需要重启应用才能完全生效
2. **默认语言**: 默认使用中文（简体）
3. **资源缺失**: 如果资源键不存在，将显示 `[KeyName]` 格式的占位符
4. **单例模式**: LocalizationService 是单例模式，确保不会有多实例冲突
5. **动态绑定**: 使用 Binding 方式可以实现动态语言切换（推荐使用）

---

## 🎯 最佳实践

1. **所有用户可见的文本都应使用本地化**
2. **使用规范的资源键名前缀**
3. **保持中英文资源文件同步**
4. **为调试信息使用英文**
5. **不要遗漏任何本地化文本**

---

## 📖 完整示例

### 示例: 新窗口的国际化实现

```xaml
<hc:Window
    x:Class="SceneTodo.Views.ExampleWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:hc="https://handyorg.github.io/handycontrol"
    xmlns:services="clr-namespace:SceneTodo.Services"
    Title="{Binding Source={x:Static services:LocalizationService.Instance}, Path=[Example_Title]}"
    Width="600"
    Height="400">

    <Grid>
        <StackPanel Margin="20">
            <TextBlock Text="{services:Localization Example_Description}" />

            <Button
                Content="{services:Localization Common_Save}"
                Click="SaveButton_Click" />

            <Button
                Content="{services:Localization Common_Cancel}"
                Click="CancelButton_Click" />
        </StackPanel>
    </Grid>
</hc:Window>
```

```csharp
using HandyControl.Controls;
using SceneTodo.Services;
using System.Windows;

namespace SceneTodo.Views
{
    public partial class ExampleWindow : HandyControl.Controls.Window
    {
        public ExampleWindow()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                LocalizationService.Instance["Message_SaveSuccess"],
                LocalizationService.Instance["Message_Info"],
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
```

---

## 🗓️ 后续计划

1. 迁移所有硬编码的 UI 文本到资源文件
2. 添加更多语言支持
3. 实现动态语言切换（无需重启）
4. 提供语言包导出/导入功能
5. 支持右向左（RTL）语言布局

---

**注意**: 本文档基于国际化功能的定制编写，相关文档请参考 `Doc/00-必读/国际化使用指南.md`
