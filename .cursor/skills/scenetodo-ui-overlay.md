# SceneTodo - UI Overlay Development Skill

> **Purpose**: 专门处理 SceneTodo 项目的悬浮窗（Overlay）UI 开发
> **Context**: WPF + HandyControl + Win32 API 互操作
> **适用场景**: 悬浮窗功能开发、窗口关联、位置调整

---

## 🖥️ Overlay 系统架构

### 核心组件
- **OverlayWindow**: 悬浮窗窗口 (`Views/OverlayWindow.xaml`)
- **NativeMethods**: Win32 API 调用 (`Utils/NativeMethods.cs`)
- **WindowHelper**: 窗口辅助工具 (`Utils/WindowHelper.cs`)
- **TrayIconManager**: 托盘图标管理 (`Utils/TrayIconManager.cs`)

### Overlay 窗口特性
- 半透明遮盖层
- 跟随目标窗口移动
- 6 个预设位置 + 自定义偏移
- DPI 感知
- 最小化/隐藏同步

---

## 🎨 XAML Overlay 窗口规范

### 基本结构

```xaml
<hc:Window
    x:Class="SceneTodo.Views.OverlayWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:hc="https://handyorg.github.io/handycontrol"
    xmlns:services="clr-namespace:SceneTodo.Services"
    Title="{services:Localization Overlay_Title}"
    WindowStyle="None"
    AllowsTransparency="True"
    Background="Transparent"
    ShowInTaskbar="False"
    Topmost="True"
    SizeToContent="WidthAndHeight">

    <Border
        Background="{Binding BackgroundColor}"
        BorderBrush="{Binding BorderColor}"
        BorderThickness="2"
        CornerRadius="8"
        Opacity="{Binding Opacity}">

        <StackPanel Margin="10">
            <!-- 待办事项列表 -->
        </StackPanel>
    </Border>
</hc:Window>
```

### 关键属性

| 属性 | 值 | 说明 |
|------|-----|------|
| `WindowStyle` | `None` | 无标题栏 |
| `AllowsTransparency` | `True` | 允许透明 |
| `Background` | `Transparent` | 透明背景 |
| `ShowInTaskbar` | `False` | 不显示在任务栏 |
| `Topmost` | `True` | 始终在最上层 |
| `SizeToContent` | `WidthAndHeight` | 自动调整大小 |

---

## 📍 位置选择系统

### 6 个预设位置

```csharp
public enum OverlayPosition
{
    TopLeft,        // 左上角
    TopRight,       // 右上角
    TopCenter,      // 顶部居中
    BottomLeft,     // 左下角
    BottomRight,    // 右下角
    BottomCenter    // 底部居中
}
```

### 位置计算逻辑

```csharp
private void UpdateOverlayPosition()
{
    if (TargetWindow == null || !TargetWindow.IsVisible)
        return;

    var targetRect = new Rect(
        TargetWindow.Left,
        TargetWindow.Top,
        TargetWindow.Width,
        TargetWindow.Height);

    Point position;

    switch (Settings.OverlayPosition)
    {
        case OverlayPosition.TopLeft:
            position = new Point(targetRect.Left + OffsetX, targetRect.Top + OffsetY);
            break;

        case OverlayPosition.TopRight:
            position = new Point(targetRect.Right - Width - OffsetX, targetRect.Top + OffsetY);
            break;

        case OverlayPosition.TopCenter:
            position = new Point(targetRect.Left + (targetRect.Width - Width) / 2 + OffsetX,
                                  targetRect.Top + OffsetY);
            break;

        case OverlayPosition.BottomLeft:
            position = new Point(targetRect.Left + OffsetX, targetRect.Bottom - Height - OffsetY);
            break;

        case OverlayPosition.BottomRight:
            position = new Point(targetRect.Right - Width - OffsetX,
                                  targetRect.Bottom - Height - OffsetY);
            break;

        case OverlayPosition.BottomCenter:
            position = new Point(targetRect.Left + (targetRect.Width - Width) / 2 + OffsetX,
                                  targetRect.Bottom - Height - OffsetY);
            break;

        default:
            position = new Point(targetRect.Left + 10, targetRect.Top + 10);
            break;
    }

    this.Left = position.X;
    this.Top = position.Y;
}
```

---

## 🎯 窗口关联和监听

### Win32 API 声明

```csharp
internal static class NativeMethods
{
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
```

### 窗口位置监听

```csharp
private DispatcherTimer _positionTimer;

private void StartPositionMonitoring()
{
    _positionTimer = new DispatcherTimer
    {
        Interval = TimeSpan.FromMilliseconds(100)  // 每 100ms 检查一次
    };

    _positionTimer.Tick += (s, e) =>
    {
        if (TargetWindow != null && TargetWindow.IsVisible)
        {
            UpdateOverlayPosition();
        }
    };

    _positionTimer.Start();
}
```

### 获取前台窗口

```csharp
private Window? GetTargetWindow()
{
    // 根据关联的应用程序窗口获取对应的 Window 对象
    var targetApp = AssociatedApp;
    if (targetApp == null)
        return null;

    // 这里需要根据实际逻辑获取目标窗口
    // 可能需要通过窗口标题、进程等匹配
    return FindWindowByProcess(targetApp.ProcessId);
}
```

---

## 🔧 透明度控制

### 滑块控制（10-100%）

```xaml
<StackPanel>
    <TextBlock Text="{services:Localization Settings_Opacity}" />

    <Slider
        Minimum="10"
        Maximum="100"
        Value="{Binding OpacityPercent, UpdateSourceTrigger=PropertyChanged}"
        TickFrequency="5"
        IsSnapToTickEnabled="True" />

    <StackPanel Orientation="Horizontal">
        <Button
            Content="25%"
            Command="{Binding SetOpacityCommand}"
            CommandParameter="25" />

        <Button
            Content="50%"
            Command="{Binding SetOpacityCommand}"
            CommandParameter="50" />

        <Button
            Content="75%"
            Command="{Binding SetOpacityCommand}"
            CommandParameter="75" />

        <Button
            Content="100%"
            Command="{Binding SetOpacityCommand}"
            CommandParameter="100" />
    </StackPanel>
</StackPanel>
```

### ViewModel 属性

```csharp
private double _opacityPercent = 80;

public double OpacityPercent
{
    get => _opacityPercent;
    set
    {
        if (SetProperty(ref _opacityPercent, value))
        {
            // 10-100% 转换为 0.1-1.0
            Opacity = _opacityPercent / 100.0;
        }
    }
}

private double _opacity = 0.8;

public double Opacity
{
    get => _opacity;
    private set => SetProperty(ref _opacity, value);
}
```

---

## 🎭 主题和样式

### Light/Dark 模式

```xaml
<hc:Window.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/HandyControl;component/Themes/SkinDefault.xaml" />
            <ResourceDictionary Source="pack://application:,,,/HandyControl;component/Themes/Theme.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</hc:Window.Resources>
```

### 优先级颜色

```csharp
public Brush GetPriorityBorderColor(Priority priority)
{
    return priority switch
    {
        Priority.High => new SolidColorBrush(Color.FromRgb(255, 100, 100)),   // 红色
        Priority.Medium => new SolidColorBrush(Color.FromRgb(255, 200, 100)),  // 黄色
        Priority.Low => new SolidColorBrush(Color.FromRgb(150, 150, 150)),    // 灰色
        _ => new SolidColorBrush(Color.FromRgb(150, 150, 150))
    };
}
```

---

## 📋 TodoItemControl 控件

### 基本结构

```xaml
<UserControl
    x:Class="SceneTodo.Views.TodoItemControl"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:hc="https://handyorg.github.io/handycontrol">

    <Border
        BorderBrush="{Binding BorderColor}"
        BorderThickness="2"
        CornerRadius="4"
        Margin="5"
        Padding="10"
        Background="{Binding BackgroundColor}"
        Opacity="{Binding Opacity}">

        <StackPanel>
            <!-- 标题和完成状态 -->
            <CheckBox
                Content="{Binding Title}"
                IsChecked="{Binding IsCompleted}"
                Command="{Binding ToggleCompleteCommand}" />

            <!-- 描述 -->
            <TextBlock
                Text="{Binding Description}"
                TextWrapping="Wrap"
                Margin="0,5,0,0"
                Visibility="{Binding HasDescription, Converter={StaticResource BooleanToVisibilityConverter}}" />

            <!-- 关联操作按钮 -->
            <ItemsControl
                ItemsSource="{Binding LinkedActions}"
                Margin="0,5,0,0"
                Visibility="{Binding HasLinkedActions, Converter={StaticResource BooleanToVisibilityConverter}}">
                <ItemsControl.ItemTemplate>
                    <DataTemplate>
                        <Button
                            Content="{Binding DisplayName}"
                            Command="{Binding DataContext.ExecuteLinkedActionCommand,
                                          RelativeSource={RelativeSource AncestorType=UserControl}}"
                            CommandParameter="{Binding}"
                            Margin="0,2,0,0"
                            Padding="5,2" />
                    </DataTemplate>
                </ItemsControl.ItemTemplate>
            </ItemsControl>
        </StackPanel>
    </Border>
</UserControl>
```

---

## 🔄 动画效果

### Fade In/Out

```csharp
public async Task FadeInAsync()
{
    var storyboard = new Storyboard();

    var fadeAnimation = new DoubleAnimation
    {
        From = 0,
        To = 1,
        Duration = TimeSpan.FromMilliseconds(300),
        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
    };

    Storyboard.SetTarget(fadeAnimation, this);
    Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(Window.OpacityProperty));

    storyboard.Children.Add(fadeAnimation);
    storyboard.Begin();

    await Task.Delay(300);
}

public async Task FadeOutAsync()
{
    var storyboard = new Storyboard();

    var fadeAnimation = new DoubleAnimation
    {
        From = 1,
        To = 0,
        Duration = TimeSpan.FromMilliseconds(300),
        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
    };

    Storyboard.SetTarget(fadeAnimation, this);
    Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(Window.OpacityProperty));

    storyboard.Children.Add(fadeAnimation);
    storyboard.Begin();

    await Task.Delay(300);
}
```

---

## ⚙️ 设置和配置

### Overlay 设置模型

```csharp
public class OverlaySettings : BaseModel
{
    private OverlayPosition _position = OverlayPosition.TopRight;
    private double _offsetX = 10;
    private double _offsetY = 10;
    private double _opacity = 0.8;
    private bool _showOnStartup = true;

    public OverlayPosition Position
    {
        get => _position;
        set => SetProperty(ref _position, value);
    }

    public double OffsetX
    {
        get => _offsetX;
        set => SetProperty(ref _offsetX, value);
    }

    public double OffsetY
    {
        get => _offsetY;
        set => SetProperty(ref _offsetY, value);
    }

    public double Opacity
    {
        get => _opacity;
        set => SetProperty(ref _opacity, value);
    }

    public bool ShowOnStartup
    {
        get => _showOnStartup;
        set => SetProperty(ref _showOnStartup, value);
    }
}
```

---

## ⚠️ 重要注意事项

### Win32 API 调用

- **仅限 Windows**: 使用 Win32 API 的功能仅支持 Windows 平台
- **不安全代码**: 项目中启用了 `AllowUnsafeBlocks=true` (.csproj)
- **DPI 感知**: 确保在不同 DPI 缩放下正常显示

### 窗口同步

- **最小化同步**: 目标窗口最小化时，Overlay 窗口隐藏
- **关闭同步**: 目标窗口关闭时，Overlay 窗口关闭
- **位置同步**: 使用定时器监听目标窗口位置变化

### 性能优化

- **定时器间隔**: 位置监听定时器间隔为 100ms
- **避免频繁更新**: 只在位置变化时更新 Overlay 位置
- **淡入淡出**: 使用动画效果提升用户体验

---

## 📚 相关文档

### 必读文档
- [AGENTS.md](../../AGENTS.md) - 代码风格指南
- [Overlay Position Selection Design](../Doc/02-技术文档/Overlay Position Selection Design.md)
- [快速上手指南](../Doc/00-必读/快速上手指南.md)

### 会话记录
- [Session-05 P0 Completion](../Doc/01-会话记录/Session-05-P0完结/)
- [Session-06 P1 Features](../Doc/01-会话记录/Session-06/)

---

**注意**: 本文档基于 SceneTodo 项目的 Overlay 系统定制编写，请遵循 WPF 和 HandyControl 的最佳实践。
