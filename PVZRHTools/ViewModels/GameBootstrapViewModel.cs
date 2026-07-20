using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using PVZRHTools.Services;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;
using ToolData;

namespace PVZRHTools.ViewModels;

public partial class GameBootstrapViewModel : ViewModelBase
{
    [Reactive] public partial List<MenuItemViewModel> MenuItems { get; set; }

    private INavigationService _navigationService;
    private IModifierInfoService _modifierInfoService;
    private IGameBootstrapService _gameBootstrapService;
    private INotificationService _notificationService;

    public ViewModelBase CurrentPage => _currentPage.Value;
    private readonly ObservableAsPropertyHelper<ViewModelBase> _currentPage;

    public GameBootstrapViewModel(INavigationService navigationService, IModifierInfoService modifierInfoService,
        IGameBootstrapService gameBootstrapService, INotificationService notificationService)
    {
        _navigationService = navigationService;
        _modifierInfoService = modifierInfoService;
        _gameBootstrapService = gameBootstrapService;
        _notificationService = notificationService;
        ModifierAuthors = new();

        this.WhenAnyValue(x => x._navigationService.CurrentViewModel)
            .ToProperty(this, nameof(CurrentPage), out _currentPage);

        MenuItems = new()
        {
            new(_navigationService) { MenuHeader = "游戏管理", PageType = typeof(GameInstancesViewModel) },
            //new(_navigationService) { MenuHeader = "链接跳转", PageType = typeof(LinksViewModel) },
            //new(_navigationService) { MenuHeader = "关于修改器", PageType = typeof(AboutViewModel) }
        };
        navigationService.NavigateTo<GameInstancesViewModel>();

        _modifierInfoService.ValidateAndCleanGamePaths();
        _modifierInfoService.ModifierInfo.GameVersion = Strings.GameVersion;
        _modifierInfoService.ModifierInfo.ModifierVersion = Strings.ModifierVersion;
        _modifierInfoService.SaveModifierInfo();

        _ = CheckModifierUpdatesAsync();
    }

    private async Task CheckModifierUpdatesAsync()
    {
        try
        {
            await Task.Delay(3000);
            var outdatedPaths = _modifierInfoService.GetOutdatedGamePaths();
            if (outdatedPaths.Count == 0) return;

            foreach (var path in outdatedPaths)
                await _gameBootstrapService.ProcessOutdatedModifierAsync(path);
        }
        catch
        {
            // 静默处理更新检查中的异常
        }
    }

    [ReactiveCommand]
    public void Closing()
    {
        _modifierInfoService.Save(Locator.Current.GetService<GameInstancesViewModel>()!.MenuItems);
    }

    [Reactive] public partial ModifierAuthorsViewModel ModifierAuthors { get; set; }

    [ReactiveCommand]
    public void OpenGithub() => Process.Start("explorer.exe", "https://github.com/Infinite75-2nd/PVZRHTools");
}