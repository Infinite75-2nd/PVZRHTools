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
    private readonly ObservableAsPropertyHelper<ViewModelBase> _currentPage;
    private readonly IGameBootstrapService _gameBootstrapService;
    private readonly IModifierInfoService _modifierInfoService;

    private readonly INavigationService _navigationService;
    private INotificationService _notificationService;

    public GameBootstrapViewModel(INavigationService navigationService, IModifierInfoService modifierInfoService,
        IGameBootstrapService gameBootstrapService, INotificationService notificationService)
    {
        _navigationService = navigationService;
        _modifierInfoService = modifierInfoService;
        _gameBootstrapService = gameBootstrapService;
        _notificationService = notificationService;
        ModifierAuthors = new ModifierAuthorsViewModel();

        this.WhenAnyValue(x => x._navigationService.CurrentViewModel)
            .ToProperty(this, nameof(CurrentPage), out _currentPage);

        MenuItems = new List<MenuItemViewModel>
        {
            new(_navigationService) { MenuHeader = "游戏管理", MenuIcon = "SemiIconDesktop", PageType = typeof(GameInstancesViewModel) },
            //new(_navigationService) { MenuHeader = "链接跳转", PageType = typeof(LinksViewModel) },
            new(_navigationService) { MenuHeader = "关于修改器", MenuIcon = "SemiIconInfoCircle", PageType = typeof(AboutViewModel) }
        };
        navigationService.NavigateTo<GameInstancesViewModel>();

        _modifierInfoService.ValidateAndCleanGamePaths();
        _modifierInfoService.ModifierInfo.GameVersion = Strings.GameVersion;
        _modifierInfoService.ModifierInfo.ModifierVersion = Strings.ModifierVersion;
        _modifierInfoService.SaveModifierInfo();

        _ = CheckModifierUpdatesAsync();
    }

    [Reactive] public partial List<MenuItemViewModel> MenuItems { get; set; }

    public ViewModelBase CurrentPage => _currentPage.Value;

    [Reactive] public partial ModifierAuthorsViewModel ModifierAuthors { get; set; }

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

    [ReactiveCommand]
    public void OpenGithub()
    {
        Process.Start("explorer.exe", "https://github.com/Infinite75-2nd/PVZRHTools");
    }
}