using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ToolData;
using PVZRHTools.Services;
using PVZRHTools.ViewModels;
using PVZRHTools.Views;
using Splat;

namespace PVZRHTools;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (Bootstrap)
            {
                desktop.MainWindow = new GameBootstrapView()
                {
                    DataContext = Locator.Current.GetService<GameBootstrapViewModel>(),
                };
            }
            else
            {
                desktop.MainWindow = new MainWindowView()
                {
                    DataContext = Locator.Current.GetService<MainWindowViewModel>(),
                };

                var dataSync = Locator.Current.GetService<IDataSyncService>();
                dataSync!.Connected += (sender, args) => { };
                dataSync.ErrorOccurred += (sender, args) => { };
                dataSync.Disconnected += (sender, args) => Dispatcher.UIThread.InvokeShutdown();
                dataSync.ConnectAsync();
                Locator.Current.GetService<InitDataService>()?.Initialize();

                // 加载保存的设置
                Locator.Current.GetService<ISettingsService>()?.LoadAllViewModelSettings();
            }

            Locator.Current.GetService<INotificationService>()?.NotificationManager = new(desktop.MainWindow)
            {
                MaxItems = 4,
                Position = NotificationPosition.BottomLeft,
            };

            // 后台检查更新
            _ = Locator.Current.GetService<UpdateCheckService>()?.CheckAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public static bool Bootstrap { get; set; }
    public static string GamePath { get; set; } = "";
}