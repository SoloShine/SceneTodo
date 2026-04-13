# SceneTodo 项目开发技能索引

本文件为 SceneTodo 项目量身定制的开发技能索引，帮助 AI 助手快速理解项目结构、规范和最佳实践。

## 📁 技能列表

### 🎯 核心开发技能

#### 1. scenetodo-session-archive.md
**用途**: 会话归档和交接流程管理
- **适用场景**: 每次开发会话后必须执行
- **关键内容**:
  - 5 阶段归档流程（会话验证、会话记录归档、规划文档归档、衔接文档编写、项目文档更新）
  - 归档清单标准
  - 衔接文档编写规范
  - 衔接文档归档标准
- **预估时间**: 60-90 分钟
- **优先级**: 🔥 **每次会话结束后必须执行**
- **依赖**: `Doc/00-必读/会话归档与交接规范.md`

#### 2. scenetodo-i18n.md
**用途**: 国际化（i18n）功能实现
- **适用场景**: 添加新功能、修改 UI 文本、支持新语言
- **关键内容**:
  - 在 XAML 中使用本地化扩展（`{services:Localization KeyName}`）
  - 在 C# 中获取本地化文本（`LocalizationService.Instance["KeyName"]`）
  - 修改 `Resources/Strings.resx`（中文）和 `Resources/Strings.en.resx`（英文）
  - 命名规范（Common_*, Todo_*, Settings_*, Filter_*, Message_*）
  - 资源文件设计模式
- **优先级**: 🔧 修改 UI 文本或添加新功能时
- **依赖**: `Doc/00-必读/国际化使用指南.md`

#### 3. scenetodo-model-repository.md
**用途**: 数据模型和 Repository 层操作
- **适用场景**: 数据库模型修改、Repository 操作、数据持久化
- **关键内容**:
  - MVVM + Repository 模式 + EF Core + SQLite
  - Repository 模式规范（接口 + 实现）
  - Async/Await 规范（强制异步，方法名以 `Async` 结尾）
  - Service Locator 模式（`App.xaml.cs` 中的静态单例）
  - 搜索服务（`SearchService`）和过滤逻辑（`SearchFilter`）
  - 数据持久化机制（JSON 文件、EF Core）
- **优先级**: 🔧 数据层修改、添加新查询、优化性能
- **依赖**: `Doc/00-必读/开发文档/开发文档-开发标准.md`

#### 4. scenetodo-ui-overlay.md
**用途**: 悬浮窗（Overlay）UI 开发
- **适用场景**: 悬浮窗功能开发、窗口关联、位置调整
- **关键内容**:
  - 6 个预设位置 + 自定义偏移
  - 透明度控制（10-100%）
  - 主题切换（Light/Dark）
  - Win32 API 互操作（`Utils/NativeMethods`）
  - 窗口位置监听
  - 遮盖层定位
  - DPI 感知
  - 关闭同步机制
- **优先级**: 🔧 悬浮窗功能开发、UI 优化
- **依赖**: `Doc/00-必读/开发文档/开发标准.md`, `Doc/00-必读/开发文档/UI 交互规范.md`

#### 5. scenetodo-documentation.md
**用途**: 文档编写规范和标准
- **适用场景**: 编写技术文档、功能文档、归档文档
- **关键内容**:
  - 文档结构规范（目录结构、命名规范）
  - 文档命名规范（Session-N、规划文档、执行清单、归档清单、归档完成报告）
  - Markdown 格式规范（标题层级、列表、代码块、表格）
  - 文档更新规范（修改历史、版本控制、同步更新）
  - 文档分割规则（>1000 行必须分割）
  - 归档文档完整性检查清单
  - 文档同步更新规范
- **优先级**: 🔧 编写或修改文档后必须同步更新
- **依赖**: `Doc/00-必读/文档编写规范.md`

---

## 🎯 修复技能（当前会话）

### 🔥 Search & Filter Fix
**用途**: 修复搜索结果显示和筛选面板问题
**适用场景**: 当前会话（Session-15+）
- **关键内容**:
  1. **修复搜索结果不显示**: 在 `ExecuteSearchAsync` 中将搜索结果应用到 `Model.TodoItems`
  2. **筛选面板 Popup 模式**: 将 `AdvancedFilterPanel` 改为 Popup 控件
  3. **搜索框可见性控制**: 搜索框只在主页面（待办列表）显示
  4. **国际化完善**: 为筛选面板添加中英文文本
  5. **浮窗不挤压内容**: 使用 Popup 控件，不占用 Grid 行空间
- **优先级**: 🔥🔥🔥 **高优先级**
- **依赖**: `scenetodo-model-repository.md`, `scenetodo-ui-overlay.md`, `scenetodo-i18n.md`

---

## 📚 常见任务模板

### 添加新功能
1. 创建功能文件：
   ```csharp
   // Models/NewFeature.cs
   ```

2. 创建 Repository：
   ```csharp
   // Services/Database/Repositories/INewFeatureRepository.cs
   // Services/Database/Repositories/NewFeatureRepository.cs
   ```

3. 更新数据库上下文：
   ```csharp
   // Services/Database/TodoDbContext.cs
   public DbSet<NewFeature> NewFeatures { get; set; }
   ```

4. 更新主窗口 ViewModel：
   ```csharp
   // ViewModels/MainWindowViewModel.NewFeature.cs
   ```

5. 更新 UI（XAML）：
   ```xml
   <!-- Views/NewFeatureWindow.xaml -->
   ```

6. 添加国际化：
   ```xml
   <!-- Resources/Strings.resx -->
   <data name="NewFeature_Title">新功能</data>
   ```

---

## 🛠️ 已知问题

### 搜索结果不显示（已修复）
- **原因**: 搜索结果存储在 `SearchResults`，但 UI 绑定到 `Model.TodoItems`
- **修复**: 在 `ExecuteSearchAsync` 中，将搜索结果同步到 `Model.TodoItems`

### 筛选面板挤压内容（已修复）
- **原因**: 面板位于 Grid.Row="1"，Height="Auto"，挤压下方内容
- **修复**: 将面板改为 Popup 模式，不占用布局空间

### 搜索框全局显示（已修复）
- **原因**: 搜索框在所有页面显示
- **修复**: 添加 `IsSearchVisible` 属性，只在主页面显示

### 插件系统（待实现）
- **P1 功能** - Session-14 Candidate
- **优先级**: 中
- **预估时间**: 8-10 小时

### 脚本执行（P2 特性）
- **优先级**: P2
- **预估时间**: 4-6 小时

### OCR/数据提取（P3 特性）
- **优先级**: P3
- **预估时间**: 8-10 小时

---

## 🎯 技术约束

1. **MVVM 模式**: 严格的视图与视图模型分离
2. **Repository 模式**: 所有数据库操作通过 Repository
3. **异步优先**: 所有数据库操作必须异步（以 `Async` 结尾）
4. **Service Locator**: 通过 `App.xaml.cs` 访问单例（非 DI 容器）
5. **国际化优先级**: 所有用户可见文本必须国际化
6. **文档驱动**: 重要变更必须更新相关文档
7. **归档强制**: 每次会话后执行归档流程
8. **代码质量**: 保持一致性、可读性和可维护性
9. **命名规范**: PascalCase for public members, _camelCase for private fields

---

## 📚 常见快捷操作

```csharp
// 1. 创建新 Model
public class NewItem : BaseModel
{
    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                UpdatedAt = DateTime.Now;
                OnPropertyChanged(nameof(Name));
            }
        }
    }
}

// 2. 创建 Repository
public class NewItemRepository : INewItemRepository
{
    public async Task<NewItem?> GetByIdAsync(string id)
    {
        return await dbContext.NewItems
            .FirstOrDefaultAsync(t => t.Id == id);
    }
}

// 3. 更新数据库上下文
public DbSet<NewItem> NewItems { get; set; }
```

---

## 📚 文档同步规则

### 需要同步更新文档

| 变更类型 | 更新文档 |
|---------|---------|
| 新功能 | PRD、项目状态总览 |
| Bug 修复 | 项目状态总览 |
| UI 改动 | 开发文档/UI 交互规范 |
| 架构优化 | 开发文档/架构设计 |
| 国际化 | 开发文档/国际化使用指南 |

---

**注意**: 此技能索引应与 `AGENTS.md` 保持同步，作为项目特定的上下文，为 AI 助手提供准确的项目理解。
