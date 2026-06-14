using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Threading;
using ToolData;
using PVZRHTools.Services;
using PVZRHTools.Views;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;

namespace PVZRHTools.ViewModels;

public partial class MainWindowViewModel : ModifierPageViewModelBase
{
    public void MessageReceived(object? sender, SyncData message)
    {
        switch (message.Command)
        {
            case Strings.Exit:
                Dispatcher.UIThread.InvokeShutdown();
                break;
            case Strings.ReloadInitData:
                _initDataService.Initialize();
                break;
        }
    }

    [Reactive] public partial ModifierAuthorsViewModel ModifierAuthors { get; set; }
    [Reactive] public partial List<MenuItemViewModel> MenuItems { get; set; }
    [Reactive] public partial bool ShowFloatingWindow { get; set; }

    public ViewModelBase CurrentPage => _currentPage.Value;
    private readonly ObservableAsPropertyHelper<ViewModelBase> _currentPage;
    private readonly INavigationService _navigationService;
    private readonly IInitDataService _initDataService;
    private FloatingWindow? _floatingWindow;
    private FloatingWindowViewModel? _floatingWindowViewModel;

    [ReactiveCommand]
    public void Exit()
    {
        DataSyncService.SendCommand(new()
        {
            Command = Strings.Exit,
            Parameters = []
        });
    }

    [ReactiveCommand]
    public void BringWindowToFront()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var window =
                Avalonia.Application.Current?.ApplicationLifetime as
                    Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
            window?.MainWindow?.Activate();
        });
    }

    [ReactiveCommand]
    public void Closing()
    {
        HideFloatingWindowInternal();
        Locator.Current.GetService<ISettingsService>()?.SaveAllViewModelSettings();
    }


    public MainWindowViewModel(INavigationService navigationService, IDataSyncService dataSyncService,
        IInitDataService initDataService) :
        base(dataSyncService)
    {
        DataSyncService.MessageReceived += MessageReceived;
        _navigationService = navigationService;
        _initDataService = initDataService;
        _initDataService.Initialize();
        ModifierAuthors = new();

        this.WhenAnyValue(x => x._navigationService.CurrentViewModel)
            .ToProperty(this, nameof(CurrentPage), out _currentPage);
        AppDomain.CurrentDomain.ProcessExit += (sender, e) => Exit();
        MenuItems = new()
        {
            new(_navigationService) { MenuHeader = "通用修改", PageType = typeof(CommonSettingsViewModel) },
            new(_navigationService) { MenuHeader = "游戏特性修改", PageType = typeof(PropertySettingsViewModel) },
            new(_navigationService) { MenuHeader = "精细出怪修改", PageType = typeof(ZombiesListViewModel) },
            new(_navigationService) { MenuHeader = "布阵器", PageType = typeof(FieldReadWriteViewModel) },
            new(_navigationService) { MenuHeader = "旅行词条修改", PageType = typeof(TravelBuffViewModel) },
            new(_navigationService) { MenuHeader = "旗帜波词条修改", PageType = typeof(FlagWaveBuffsViewModel) },
            new(_navigationService) { MenuHeader = "游戏内按键绑定", PageType = typeof(InGameHotkeysViewModel) },
            //new(_navigationService) { MenuHeader = "全局按键绑定", PageType = typeof(CommonSettingsViewModel) },
            new(_navigationService) { MenuHeader = "检索分区", PageType = typeof(SearchListViewModel) },
            new(_navigationService) { MenuHeader = "局内存档/回溯", PageType = typeof(SnapshotViewModel) },
            //new(_navigationService) { MenuHeader = "诸神进化", PageType = typeof(GodEvolutionViewModel) },
            new(_navigationService) { MenuHeader = "其他设置", PageType = typeof(MiscsViewModel) },
        };

        _navigationService.NavigateTo<CommonSettingsViewModel>();

        // 监听ShowFloatingWindow属性变化
        this.WhenAnyValue(x => x.ShowFloatingWindow)
            .Subscribe(show =>
            {
                if (show)
                {
                    ShowFloatingWindowInternal();
                }
                else
                {
                    HideFloatingWindowInternal();
                }
            });
    }

    private void ShowFloatingWindowInternal()
    {
        if (_floatingWindow != null) return;

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            _floatingWindowViewModel = new FloatingWindowViewModel();
            _floatingWindow = new FloatingWindow
            {
                DataContext = _floatingWindowViewModel
            };

            // 设置初始位置在主窗口右侧
            var mainWindow = Avalonia.Application.Current?.ApplicationLifetime as
                Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
            if (mainWindow?.MainWindow != null)
            {
                var mainPos = mainWindow.MainWindow.Position;
                _floatingWindow.Position = new PixelPoint(
                    mainPos.X + (int)mainWindow.MainWindow.Width + 10,
                    mainPos.Y);
            }

            _floatingWindow.Show();
        });
    }

    private void HideFloatingWindowInternal()
    {
        if (_floatingWindow == null) return;

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            _floatingWindow?.Close();
            _floatingWindow = null;
            _floatingWindowViewModel = null;
        });
    }
}