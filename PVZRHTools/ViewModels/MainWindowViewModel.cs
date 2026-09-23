using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using PVZRHTools.Services;
using PVZRHTools.Views;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;
using ToolData;

namespace PVZRHTools.ViewModels;

public partial class MainWindowViewModel : ModifierPageViewModelBase
{
    private readonly ObservableAsPropertyHelper<ViewModelBase> _currentPage;
    private readonly IInitDataService _initDataService;
    private readonly INavigationService _navigationService;
    private FloatingWindow? _floatingWindow;
    private FloatingWindowViewModel? _floatingWindowViewModel;


    public MainWindowViewModel(INavigationService navigationService, IDataSyncService dataSyncService,
        IInitDataService initDataService) :
        base(dataSyncService)
    {
        DataSyncService.MessageReceived += MessageReceived;
        _navigationService = navigationService;
        _initDataService = initDataService;
        _initDataService.Initialize();
        ModifierAuthors = new ModifierAuthorsViewModel();

        this.WhenAnyValue(x => x._navigationService.CurrentViewModel)
            .ToProperty(this, nameof(CurrentPage), out _currentPage);
        AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
        {
            SaveSettings();
            Exit();
        };
        MenuItems = new List<MenuItemViewModel>
        {
            new(_navigationService) { MenuHeader = "通用修改", MenuIcon = "SemiIconApps", PageType = typeof(CommonSettingsViewModel) },
            new(_navigationService) { MenuHeader = "游戏特性修改", MenuIcon = "SemiIconPuzzle", PageType = typeof(PropertySettingsViewModel) },
            new(_navigationService) { MenuHeader = "精细出怪修改", MenuIcon = "SemiIconUserList", PageType = typeof(ZombiesListViewModel) },
            new(_navigationService) { MenuHeader = "布阵器", MenuIcon = "SemiIconGridSquare", PageType = typeof(FieldReadWriteViewModel) },
            new(_navigationService) { MenuHeader = "旅行词条修改", MenuIcon = "SemiIconFlag", PageType = typeof(TravelBuffViewModel) },
            //new(_navigationService) { MenuHeader = "旗帜波词条修改", PageType = typeof(FlagWaveBuffsViewModel) },
            new(_navigationService) { MenuHeader = "诸神进化", MenuIcon = "SemiIconCrown", PageType = typeof(GodEvolutionViewModel) },
            new(_navigationService) { MenuHeader = "深渊/神秘/花园/星辉", MenuIcon = "SemiIconStar", PageType = typeof(AbyssAndTreasureViewModel) },
            new(_navigationService) { MenuHeader = "局内存档/回溯", MenuIcon = "SemiIconSave", PageType = typeof(SnapshotViewModel) },
            new(_navigationService) { MenuHeader = "检索分区", MenuIcon = "SemiIconSearch", PageType = typeof(SearchListViewModel) },
            new(_navigationService) { MenuHeader = "其他设置", MenuIcon = "SemiIconSetting", PageType = typeof(MiscsViewModel) },
            new(_navigationService) { MenuHeader = "关于修改器", MenuIcon = "SemiIconInfoCircle", PageType = typeof(AboutViewModel) },
        };

        _navigationService.NavigateTo<CommonSettingsViewModel>();

        // 监听ShowFloatingWindow属性变化
        this.WhenAnyValue(x => x.ShowFloatingWindow).Subscribe(show =>
        {
            if (show)
                ShowFloatingWindowInternal();
            else
                HideFloatingWindowInternal();
        });
    }

    [Reactive] public partial ModifierAuthorsViewModel ModifierAuthors { get; set; }
    [Reactive] public partial List<MenuItemViewModel> MenuItems { get; set; }
    [Reactive] public partial bool ShowFloatingWindow { get; set; }

    public ViewModelBase CurrentPage => _currentPage.Value;

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

    [ReactiveCommand]
    public void Exit()
    {
        SaveSettings();
        DataSyncService.SendCommand(new SyncData
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
                Application.Current?.ApplicationLifetime as
                    IClassicDesktopStyleApplicationLifetime;
            window?.MainWindow?.Activate();
        });
    }

    [ReactiveCommand]
    public void Closing()
    {
        // 先保存设置，再处理浮窗关闭，避免 HideFloatingWindowInternal 抛出异常导致保存中断
        SaveSettings();
        HideFloatingWindowInternal();
    }

    private void SaveSettings()
    {
        try
        {
            Locator.Current.GetService<ISettingsService>()?.SaveAllViewModelSettings();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"保存设置时发生异常: {ex}");
        }
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
            var mainWindow = Application.Current?.ApplicationLifetime as
                IClassicDesktopStyleApplicationLifetime;
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