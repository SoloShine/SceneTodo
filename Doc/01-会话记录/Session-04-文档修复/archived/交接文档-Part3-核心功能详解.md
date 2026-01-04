# ?? SceneTodo 项目交接文档 - Part 3: 核心功能详解

> **创建日期**: 2026-01-02 18:43:00  
> **最后更新**: 2026-01-02 22:16:53  
> **版本**: v3.2  
> **文档状态**: 正式发布  
> **所属文档**: 项目交接文档  
> **部分编号**: Part 3 / 5

---

## ?? 导航

?? [返回总索引](交接文档-最新版.md) | [上一部分: Part 2 ←](交接文档-Part2-*.md) | [下一部分: Part 4 →](交接文档-Part4-*.md)

---

## ?? 本部分内容

本部分包含第6-7章：
项目结构详解、核心功能说明

---

## 🎯 核心功能说明

### 1. 优先级功能⭐⭐⭐⭐⭐

#### 功能描述
为待办项设置三个优先级，通过颜色边框区分。

#### 优先级定义
- **高优先级（Priority = 2）**: 红色边框
- **中优先级（Priority = 1）**: 黄色边框
- **低优先级（Priority = 0）**: 绿色边框（默认）

#### 技术实现
```csharp
// Converters/PriorityToBorderBrushConverter.cs
public object Convert(object value, ...)
{
    if (value is int priority)
    {
        return priority switch
        {
            2 => Brushes.Red,      // 高
            1 => Brushes.Yellow,   // 中
            _ => Brushes.Green     // 低
        };
    }
    return Brushes.Green;
}
```

#### 使用场景
- 标识紧急任务
- 任务排序和筛选
- 提醒用户关注重要事项

### 2. 关联操作功能⭐⭐⭐⭐⭐

#### 功能描述
为待办项关联外部资源（网页、文件、应用），一键执行。

#### 支持的类型
1. **网页链接**
   - HTTP/HTTPS
   - 默认浏览器打开
   - 示例：https://github.com

2. **本地文件**
   - 任意格式
   - 系统关联程序打开
   - 示例：D:\Documents\report.docx

3. **应用程序**
   - .exe可执行文件
   - 直接启动
   - 示例：C:\Program Files\VSCode\Code.exe

#### 数据结构
```csharp
public class LinkedAction
{
    public int Id { get; set; }
    public string Name { get; set; }        // 显示名称
    public string Type { get; set; }        // Web/File/App
    public string Target { get; set; }      // URL/路径
}
```

#### 存储方式
存储在 `TodoItems.LinkedActionsJson` 字段（JSON格式）：
```json
[
  {"Id":1,"Name":"官网","Type":"Web","Target":"https://example.com"},
  {"Id":2,"Name":"文档","Type":"File","Target":"D:\\docs\\file.pdf"}
]
```

#### 操作方式
- 右键菜单中选择关联操作
- 点击执行按钮
- 一键打开所有关联资源

### 3. 历史记录功能⭐⭐⭐⭐

#### 功能描述
记录待办项的所有变更，支持查看和恢复历史版本。

#### 记录时机
- 创建待办项
- 修改待办项（标题、内容、优先级等）
- 删除待办项
- 勾选/取消勾选

#### 历史记录窗口
- 按时间倒序显示
- 显示变更内容（JSON格式）
- 支持恢复到历史版本
- 支持删除历史记录

#### 技术实现
```csharp
// 保存历史记录
await _repository.AddHistoryAsync(new TodoItemHistory
{
    TodoItemId = item.Id,
    ContentJson = JsonConvert.SerializeObject(item),
    CreatedAt = DateTime.Now
});
```

#### ⚠️ 注意
历史记录窗口的入口按钮尚未添加到主界面！需要手动添加：

```xaml
<!-- MainWindow.xaml -->
<Button 
    Content="历史记录" 
    Command="{Binding ShowHistoryCommand}"
    Style="{StaticResource ButtonIcon}"
    hc:IconElement.Geometry="{StaticResource HistoryGeometry}" />
```

### 4. 右键菜单交互⭐⭐⭐⭐⭐

#### 功能描述
通过右键菜单提供所有待办项操作，替代不稳定的悬停按钮。

#### 菜单结构

**普通待办项**:
```
📝 编辑 (E)
➕ 添加子项 (A)
──────────
🗑️ 删除 (D)
```

**软件待办项**:
```
📝 编辑 (E)
➕ 添加子项 (A)
──────────
🚀 强制唤起软件 (L)
💉 开启/关闭注入 (I)  [动态文本和颜色]
──────────
🗑️ 删除 (D)
```

#### 注入状态可视化
在待办项上直接显示注入状态标签：
- **已注入**: 红色文字 "已注入"
- **未注入**: 绿色文字 "未注入"

#### 快捷键
- `Alt+E`: 编辑
- `Alt+A`: 添加子项
- `Alt+L`: 强制唤起
- `Alt+I`: 切换注入
- `Alt+D`: 删除

#### 优势
- ✅ 稳定可靠（不会消失）
- ✅ 符合Windows标准
- ✅ 支持快捷键
- ✅ 易于扩展
- ✅ 触摸屏友好

---

## ⚠️ 已知问题和限制

### P0 级别（阻断性）
目前无P0级别问题。

### P1 级别（重要）

#### 1. 历史记录窗口未集成⚠️
**问题描述**:
- MainWindow.xaml 中缺少"历史记录"按钮
- ShowHistoryCommand 已实现，但UI未添加入口
- 用户无法打开历史记录窗口

**影响范围**: 用户体验

**修复方案**:
```xaml
<!-- 在 MainWindow.xaml 的左侧菜单中添加 -->
<Button 
    Content="历史记录" 
    Command="{Binding ShowHistoryCommand}"
    Style="{StaticResource ButtonIcon}"
    hc:IconElement.Geometry="{StaticResource HistoryGeometry}" />
```

**预计工作量**: 10分钟

**优先级**: P1

#### 2. 历史记录分页未实现
**问题描述**:
- 大量历史记录时可能影响性能
- 当前一次性加载所有记录

**建议方案**:
- 实现分页加载
- 每页显示20-50条
- 添加翻页控件

**预计工作量**: 2-3小时

**优先级**: P1

### P2 级别（优化）

#### 3. 代码需要重构
**问题描述**:
- `MainWindowViewModel.cs` 约800行，较长
- 存在重复的if判断
- 部分命名不一致

**建议方案**:
- 提取公共方法
- 拆分为多个ViewModel
- 统一命名规范

**预计工作量**: 4-5小时

**优先级**: P2

#### 4. 子待办项AppPath继承测试不完整
**问题描述**:
- 功能已实现，但未充分测试
- 边界情况可能有问题

**建议方案**:
- 编写完整测试用例
- 测试各种边界情况

**预计工作量**: 1小时

**优先级**: P2

---


---

**?? 继续阅读**: [下一部分: Part 4 →](交接文档-Part4-*.md)

