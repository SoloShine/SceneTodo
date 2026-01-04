using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using SceneTodo.Services;
using SceneTodo.Services.Database;
using SceneTodo.Services.Database.Repositories;
using SceneTodo.Services.Scheduler;
using SceneTodo.Utils;
using SceneTodo.ViewModels;

namespace SceneTodo;

/// <summary>  
/// Interaction logic for App.xaml  
/// </summary>  
public partial class App : Application
{
    private static MainWindowViewModel? mainViewModel;
    /// <summary>
    /// 全局单例 ViewModel
    /// </summary>
    public static MainWindowViewModel? MainViewModel
    {
        get
        {
            mainViewModel ??= Current.Resources["MainViewModel"] as MainWindowViewModel;
            return mainViewModel;
        }
    }

    // EF Core 上下文
    public static TodoDbContext DbContext { get; private set; }
    
    // 仓储实例
    //public static GroupRepository GroupRepository { get; private set; }
    public static TodoItemRepository TodoItemRepository { get; private set; }
    public static AutoTaskRepository AutoTaskRepository { get; private set; }
    public static TagRepository? TagRepository { get; private set; }
    public static DatabaseInitializer DatabaseInitializer { get; private set; }
    public static TodoItemSchedulerService SchedulerService { get; private set; }
    public static BackupService BackupService { get; private set; }


    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 初始化数据库服务
        await InitializeDatabaseAsync();

        // 初始化调度服务
        SchedulerService = new TodoItemSchedulerService();

        // 初始化备份服务
        BackupService = new BackupService(DbContext);

        // 确保资源中的ViewModel是唯一的  
        mainViewModel = Current.Resources["MainViewModel"] as MainWindowViewModel;

        // 在数据库初始化完成后，加载待办数据
        mainViewModel?.InitializeData();

        // 应用保存的主题设置
        mainViewModel?.Model.ApplyThemeSettings();

        // 初始化托盘图标
        TrayIconManager.Initialize();

        // 加载并启动现有的定时任务
        await LoadAndStartScheduledTasksAsync();
    }

    /// <summary>
    /// 初始化数据库
    /// </summary>
    private static async Task InitializeDatabaseAsync()
    {
        // 创建数据目录
        var dataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SceneTodo");
            
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }
        
        // 数据库路径
        var dbPath = Path.Combine(dataDir, "todo.db");
        
        // 初始化 DbContext
        var connectionString = $"Data Source={dbPath}";
        var factory = new TodoDbContextFactory(connectionString);
        DbContext = factory.CreateDbContext();
        
        // 初始化仓储
        //GroupRepository = new GroupRepository(DbContext);
        TodoItemRepository = new TodoItemRepository(DbContext);
        AutoTaskRepository = new AutoTaskRepository(DbContext);
        TagRepository = new TagRepository(DbContext);
        //AppAssociationRepository = new AppAssociationRepository(DbContext);

        // 初始化数据库
        DatabaseInitializer = new DatabaseInitializer(DbContext);
        await DatabaseInitializer.InitializeAsync();
    }

    private static async Task LoadAndStartScheduledTasksAsync()
    {
        try
        {
            var tasks = await AutoTaskRepository.GetAllAsync();
            foreach (var task in tasks.Where(t => t.IsEnabled))
            {
                task.UpdateNextExecuteTime();
                await SchedulerService.ScheduleAutoTask(task);
            }
            
            System.Diagnostics.Debug.WriteLine($"Loaded and scheduled {tasks.Count(t => t.IsEnabled)} tasks");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load scheduled tasks: {ex.Message}");
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        // 关闭调度服务
        if (SchedulerService != null)
        {
            await SchedulerService.ShutdownAsync();
        }

        // 释放 DbContext
        DbContext?.Dispose();
        
        // 清理托盘图标
        TrayIconManager.Cleanup();

        base.OnExit(e);
    }
}

