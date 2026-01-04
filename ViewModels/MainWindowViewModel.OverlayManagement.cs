using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using SceneTodo.Models;
using SceneTodo.Utils;
using SceneTodo.Views;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SceneTodo.ViewModels
{
    /// <summary>
    /// 主窗口 ViewModel - 悬浮窗和应用启动
    /// 包含：悬浮窗管理、应用启动等功能
    /// </summary>
    public partial class MainWindowViewModel
    {
        /// <summary>
        /// 自动注入悬浮窗到当前前台窗口
        /// </summary>
        private void AutoInjectOverlays(object? sender, EventArgs e)
        {
            IntPtr foregroundHandle = NativeMethods.GetForegroundWindow();

            foreach (var item in Model.TodoItems)
            {
                if (!item.IsInjected || item.TodoItemType != TodoItemType.App || 
                    string.IsNullOrEmpty(item.AppPath) || !File.Exists(item.AppPath))
                    continue;

                string processName = Path.GetFileNameWithoutExtension(item.AppPath);
                var processes = Process.GetProcessesByName(processName);
                
                if (processes.Length == 0)
                {
                    if (overlayWindows.TryGetValue(item.AppPath, out OverlayWindow? value))
                    {
                        value.Close();
                        overlayWindows.Remove(item.AppPath);
                    }
                    continue;
                }

                Process targetProcess = processes[0];
                IntPtr targetWindowHandle = targetProcess.MainWindowHandle;
                
                if (!NativeMethods.IsWindow(targetWindowHandle))
                    continue;

                HandleOverlayWindow(item, targetWindowHandle, foregroundHandle);
            }
        }

        /// <summary>
        /// 切换注入状态
        /// </summary>
        private void ToggleIsInjected(object? parameter)
        {
            if (parameter is not TodoItemModel item) return;
            if (item.TodoItemType != TodoItemType.App) return;
            
            if (!File.Exists(item.AppPath))
            {
                MessageBox.Show("关联软件未安装，请检查路径。");
                return;
            }

            var processName = Path.GetFileNameWithoutExtension(item.AppPath);
            var targetProcesses = Process.GetProcessesByName(processName);
            if (targetProcesses.Length == 0) return;

            var targetProcess = targetProcesses[0];
            var targetWindowHandle = targetProcess.MainWindowHandle;

            if (!NativeMethods.IsWindow(targetWindowHandle))
            {
                MessageBox.Show("无法获取有效的窗口句柄。");
                return;
            }

            HandleOverlayWindow(item, targetWindowHandle, NativeMethods.GetForegroundWindow());

            void UpdateIsInjected(TodoItemModel todo)
            {
                todo.IsInjected = item.IsInjected;
                if (todo.SubItems != null)
                {
                    foreach (var subItem in todo.SubItems)
                    {
                        UpdateIsInjected(subItem);
                        App.TodoItemRepository.UpdateAsync(item).ConfigureAwait(false);
                    }
                }
            }

            UpdateIsInjected(item);
        }

        /// <summary>
        /// 处理悬浮窗的创建、更新和关闭
        /// </summary>
        private void HandleOverlayWindow(TodoItemModel item, IntPtr targetWindowHandle, IntPtr foregroundHandle)
        {
            var appKey = item.AppPath;
            if (string.IsNullOrEmpty(appKey)) return;

            if (foregroundHandle == targetWindowHandle && item.IsInjected)
            {
                if (!overlayWindows.ContainsKey(appKey))
                {
                    Debug.WriteLine("创建悬浮窗");
                    var app = TodoItemModel.RecTodoItems(Model.TodoItems, appKey);
                    var overlayWindow = new OverlayWindow(app) { Topmost = false };
                    overlayWindow.ApplyOverlaySettings();
                    overlayWindow.Show();

                    _ = NativeMethods.SetWindowLong(
                        overlayWindow.GetHandle(),
                        NativeMethods.GWL_HWNDPARENT,
                        targetWindowHandle.ToInt32());

                    var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
                    timer.Tick += (s, args) =>
                    {
                        UpdateOverlayPosition(overlayWindow, targetWindowHandle, item);
                        IntPtr hAbove = NativeMethods.GetWindow(targetWindowHandle, NativeMethods.GW_HWNDPREV);
                        if (hAbove == IntPtr.Zero) hAbove = targetWindowHandle;
                        
                        NativeMethods.SetWindowPos(
                            overlayWindow.GetHandle(), hAbove, 0, 0, 0, 0,
                            NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE);
                    };
                    timer.Start();
                    overlayWindow.Closed += (s, args) => timer.Stop();

                    overlayWindows[appKey] = overlayWindow;
                }
            }
            else
            {
                if (NativeMethods.IsIconic(targetWindowHandle) || 
                    !NativeMethods.IsWindowVisible(targetWindowHandle) || 
                    !item.IsInjected)
                {
                    if (overlayWindows.TryGetValue(appKey, out OverlayWindow? value))
                    {
                        Debug.WriteLine("关闭悬浮窗");
                        value.Close();
                        overlayWindows.Remove(appKey);
                    }
                }
            }
        }

        /// <summary>
        /// 更新悬浮窗位置
        /// </summary>
        private static void UpdateOverlayPosition(OverlayWindow overlayWindow, IntPtr targetWindowHandle, TodoItemModel item)
        {
            if (!NativeMethods.GetWindowRect(targetWindowHandle, out var rect)) return;

            var source = PresentationSource.FromVisual(overlayWindow);
            if (source?.CompositionTarget != null)
            {
                Matrix transformMatrix = source.CompositionTarget.TransformFromDevice;

                double targetLeft = rect.Left * transformMatrix.M11;
                double targetTop = rect.Top * transformMatrix.M22;
                double targetRight = rect.Right * transformMatrix.M11;
                double targetBottom = rect.Bottom * transformMatrix.M22;
                double targetWidth = targetRight - targetLeft;
                double targetHeight = targetBottom - targetTop;

                double left = 0, top = 0;

                switch (item.OverlayPosition)
                {
                    case OverlayPosition.Bottom:
                        left = targetLeft;
                        top = targetBottom - overlayWindow.ActualHeight;
                        break;
                    case OverlayPosition.TopLeft:
                        left = targetLeft;
                        top = targetTop;
                        break;
                    case OverlayPosition.TopRight:
                        left = targetRight - overlayWindow.ActualWidth;
                        top = targetTop;
                        break;
                    case OverlayPosition.BottomLeft:
                        left = targetLeft;
                        top = targetBottom - overlayWindow.ActualHeight;
                        break;
                    case OverlayPosition.BottomRight:
                        left = targetRight - overlayWindow.ActualWidth;
                        top = targetBottom - overlayWindow.ActualHeight;
                        break;
                    case OverlayPosition.Center:
                        left = targetLeft + (targetWidth - overlayWindow.ActualWidth) / 2;
                        top = targetTop + (targetHeight - overlayWindow.ActualHeight) / 2;
                        break;
                }

                double adjustedLeft = Math.Max(0, left);
                double adjustedTop = Math.Max(0, top);

                overlayWindow.Left = adjustedLeft + item.OverlayOffsetX;
                overlayWindow.Top = adjustedTop + item.OverlayOffsetY;
            }
            else
            {
                overlayWindow.Left = rect.Left + item.OverlayOffsetX;
                overlayWindow.Top = rect.Bottom - overlayWindow.ActualHeight + item.OverlayOffsetY;
            }
        }

        /// <summary>
        /// 强制启动或激活应用
        /// </summary>
        private void ForceLaunch(object? parameter)
        {
            if (parameter is not TodoItemModel item) return;
            if (item.TodoItemType != TodoItemType.App) return;
            
            if (!File.Exists(item.AppPath))
            {
                MessageBox.Show("关联软件未安装，请检查路径。");
                return;
            }

            string processName = Path.GetFileNameWithoutExtension(item.AppPath);
            Process[] processes = Process.GetProcessesByName(processName);

            if (processes.Length > 0)
            {
                Process targetProcess = processes[0];
                IntPtr mainWindowHandle = targetProcess.MainWindowHandle;

                if (mainWindowHandle != IntPtr.Zero && NativeMethods.IsWindow(mainWindowHandle))
                {
                    ActivateWindow(mainWindowHandle, item.Name);
                }
                else
                {
                    List<IntPtr> windowHandles = FindWindowsForProcess(targetProcess.Id);
                    if (windowHandles.Count > 0)
                    {
                        IntPtr visibleWindow = windowHandles.FirstOrDefault(hwnd => NativeMethods.IsWindowVisible(hwnd));
                        ActivateWindow(visibleWindow != IntPtr.Zero ? visibleWindow : windowHandles[0], item.Name);
                    }
                }
            }
            else
            {
                try
                {
                    Process.Start(new ProcessStartInfo(item.AppPath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"启动应用失败: {ex.Message}", "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// 激活窗口
        /// </summary>
        private static void ActivateWindow(IntPtr windowHandle, string? appName)
        {
            try
            {
                if (NativeMethods.IsIconic(windowHandle))
                {
                    NativeMethods.ShowWindow(windowHandle, NativeMethods.SW_RESTORE);
                }

                int style = NativeMethods.GetWindowLong(windowHandle, NativeMethods.GWL_STYLE);
                if ((style & NativeMethods.WS_VISIBLE) == 0)
                {
                    NativeMethods.SetWindowLong(windowHandle, NativeMethods.GWL_STYLE, style | NativeMethods.WS_VISIBLE);
                    NativeMethods.ShowWindow(windowHandle, NativeMethods.SW_SHOW);
                }

                bool foregroundResult = NativeMethods.SetForegroundWindow(windowHandle);
                NativeMethods.BringWindowToTop(windowHandle);

                if (!foregroundResult)
                {
                    NativeMethods.FlashWindow(windowHandle, true);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"激活窗口异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 查找进程的所有窗口
        /// </summary>
        private static List<IntPtr> FindWindowsForProcess(int processId)
        {
            List<IntPtr> result = new List<IntPtr>();

            NativeMethods.EnumWindows((hWnd, lParam) =>
            {
                NativeMethods.GetWindowThreadProcessId(hWnd, out int windowProcessId);
                if (windowProcessId == processId)
                {
                    result.Add(hWnd);
                }
                return true;
            }, IntPtr.Zero);

            return result;
        }
    }
}
