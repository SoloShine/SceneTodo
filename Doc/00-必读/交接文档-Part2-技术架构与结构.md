# ?? SceneTodo 项目交接文档 - Part 2: 技术架构与结构

> **创建日期**: 2026-01-02 18:43:00  
> **最后更新**: 2026-01-02 22:16:53  
> **版本**: v3.2  
> **文档状态**: 正式发布  
> **所属文档**: 项目交接文档  
> **部分编号**: Part 2 / 5

---

## ?? 导航

?? [返回总索引](交接文档-最新版.md) | [上一部分: Part 1 ←](交接文档-Part1-*.md) | [下一部分: Part 3 →](交接文档-Part3-*.md)

---

## ?? 本部分内容

本部分包含第4-5章：
开发会话历史、技术架构、数据库设计

---

## 🛠️ 技术架构

### 整体架构
```
SceneTodo (WPF应用)
├── 表示层 (Views + ViewModels)
│   ├── MVVM模式
│   ├── Command绑定
│   └── 数据绑定
├── 业务层 (Models + ViewModels)
│   ├── 业务逻辑
│   ├── 数据验证
│   └── 命令处理
├── 数据层 (Services/Database)
│   ├── EF Core
│   ├── Repository模式
│   └── SQLite数据库
└── 工具层 (Utils + Converters)
    ├── 值转换器
    ├── Win32 API封装
    └── 辅助工具
```

### 核心技术选型

#### 前端技术
- **WPF**: Windows桌面应用框架
- **HandyControl**: 现代化UI控件库
- **MVVM模式**: 视图和逻辑分离

#### 后端技术
- **.NET 8**: 最新的.NET平台
- **Entity Framework Core**: ORM框架
- **SQLite**: 轻量级数据库

#### 工具库
- **Newtonsoft.Json**: JSON序列化
- **MahApps.Metro.IconPacks**: 图标库
- **Quartz**: 任务调度（前置准备）

### 数据库设计

#### TodoItems表
```sql
CREATE TABLE TodoItems (
    Id INTEGER PRIMARY KEY,
    Title TEXT NOT NULL,
    Content TEXT,
    IsCompleted INTEGER,
    ParentId INTEGER,
    AppPath TEXT,
    AppName TEXT,
    Priority INTEGER DEFAULT 0,
    LinkedActionsJson TEXT DEFAULT '[]',
    CreatedAt TEXT,
    UpdatedAt TEXT,
    FOREIGN KEY (ParentId) REFERENCES TodoItems(Id)
);
```

#### TodoItemHistories表
```sql
CREATE TABLE TodoItemHistories (
    Id INTEGER PRIMARY KEY,
    TodoItemId INTEGER,
    ContentJson TEXT,
    CreatedAt TEXT,
    FOREIGN KEY (TodoItemId) REFERENCES TodoItems(Id)
);
```

#### AutoTasks表
```sql
CREATE TABLE AutoTasks (
    Id INTEGER PRIMARY KEY,
    Name TEXT,
    CronExpression TEXT,
    TaskType TEXT,
    TaskData TEXT,
    IsEnabled INTEGER,
    CreatedAt TEXT,
    UpdatedAt TEXT
);
```

---

## 📁 项目结构详解

### 目录结构
```
SceneTodo/
├── Models/                         # 数据模型层
│   ├── TodoItem.cs                # 数据库实体
│   ├── TodoItemModel.cs           # 业务模型
│   ├── LinkedAction.cs            # 关联操作模型
│   ├── MainWindowModel.cs         # 主窗口配置
│   └── AutoTask.cs                # 自动任务模型
│
├── ViewModels/                     # 视图模型层
│   ├── MainWindowViewModel.cs     # 主窗口逻辑（核心）
│   ├── HistoryWindowViewModel.cs  # 历史记录逻辑
│   └── RelayCommand.cs            # 命令基类
│
├── Views/                          # 视图层
│   ├── MainWindow.xaml/.cs        # 主窗口
│   ├── OverlayWindow.xaml/.cs     # 悬浮窗
│   ├── EditTodoItemWindow.xaml/.cs         # 编辑窗口
│   ├── EditLinkedActionWindow.xaml/.cs     # 关联操作编辑
│   ├── HistoryWindow.xaml/.cs              # 历史记录窗口
│   ├── TodoItemControl.xaml/.cs            # 待办项控件⭐
│   ├── TodoTreeView.xaml/.cs               # 树形视图
│   ├── SelectRunningAppWindow.xaml/.cs     # 选择运行应用
│   ├── ThemeSettingsWindow.xaml/.cs        # 主题设置
│   └── AboutWindow.xaml/.cs                # 关于窗口
│
├── Converters/                     # 值转换器
│   ├── PriorityToBorderBrushConverter.cs   # 优先级颜色⭐
│   ├── InjectedToTextConverter.cs          # 注入文本⭐
│   ├── InjectedToColorConverter.cs         # 注入颜色⭐
│   ├── EnumToDescriptionConverter.cs       # 枚举描述
│   ├── EnumToComboBoxConverter.cs          # 枚举下拉
│   ├── AppPathToIconConverter.cs           # 应用图标
│   ├── StringNotEmptyConverter.cs          # 字符串判空
│   ├── WidthMinusIndentConverter.cs        # 宽度计算
│   ├── DateTimeToStringConverter.cs        # 日期时间
│   ├── NullableToVisibilityConverter.cs    # 可空可见性
│   └── Int2VisibilityConverter.cs          # 整数可见性
│
├── Services/                       # 服务层
│   ├── Database/                  # 数据库服务
│   │   ├── TodoDbContext.cs              # EF Core上下文
│   │   ├── DatabaseInitializer.cs        # 数据库初始化
│   │   ├── DbContextFactory.cs           # 上下文工厂
│   │   └── Repositories/                 # 仓储模式
│   │       ├── TodoItemRepository.cs     # 待办项仓储
│   │       └── AutoTaskRepository.cs     # 自动任务仓储
│   └── Scheduler/                 # 调度服务
│       └── TodoItemSchedulerService.cs   # Quartz调度
│
├── Utils/                          # 工具类
│   ├── IconHelper.cs              # 图标提取工具
│   ├── TrayIconManager.cs         # 托盘图标管理
│   ├── NativeMethods.cs           # Win32 API封装
│   └── ColorPickerHelper.cs       # 颜色选择辅助
│
├── Doc/                            # 文档目录⭐
│   ├── 00-必读/                   # 核心文档
│   ├── 01-会话记录/               # 开发会话
│   ├── 02-功能文档/               # 功能说明
│   ├── 03-测试文档/               # 测试文档
│   ├── 04-技术文档/               # 技术文档
│   ├── 05-用户文档/               # 用户手册
│   └── 06-规划文档/               # 产品规划
│
├── app_associations.json           # 应用关联配置
├── App.xaml/.cs                    # 应用入口
├── MainWindow.xaml                 # 主窗口XAML
├── SceneTodo.csproj               # 项目文件
└── TodoOverlayApp.sln             # 解决方案
```

**⭐ 标记的是最重要/最常修改的文件**

### 关键文件说明

#### MainWindowViewModel.cs
- **行数**: 约800+行（项目中最大的文件）
- **职责**: 主窗口的所有业务逻辑
- **主要功能**:
  - 待办项CRUD操作
  - 软件窗口注入/解除
  - 历史记录管理
  - 命令处理
- **注意**: 代码较长，需要重构

#### TodoItemControl.xaml
- **职责**: 单个待办项的显示和交互
- **主要功能**:
  - 右键菜单（最新实现）
  - 注入状态标签
  - 优先级颜色边框
  - 勾选框和标题显示
- **最近修改**: Session-03（右键菜单）

#### TodoDbContext.cs
- **职责**: EF Core数据库上下文
- **主要功能**:
  - 定义数据库表
  - 配置实体关系
  - 数据库连接
- **位置**: `%LocalAppData%\SceneTodo\todo.db`

---


---

**?? 继续阅读**: [下一部分: Part 3 →](交接文档-Part3-*.md)

