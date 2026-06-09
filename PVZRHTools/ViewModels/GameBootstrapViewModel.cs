using System.Collections.Generic;
using System.Diagnostics;
using PVZRHTools.Services;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;

namespace PVZRHTools.ViewModels;

public partial class GameBootstrapViewModel : ViewModelBase
{
    [Reactive] public partial List<MenuItemViewModel> MenuItems { get; set; }

    private INavigationService _navigationService;
    private IModifierInfoService _modifierInfoService;

    public ViewModelBase CurrentPage => _currentPage.Value;
    private readonly ObservableAsPropertyHelper<ViewModelBase> _currentPage;

    public GameBootstrapViewModel(INavigationService navigationService, IModifierInfoService modifierInfoService)
    {
        _navigationService = navigationService;
        _modifierInfoService = modifierInfoService;
        ModifierAuthors = new();

        this.WhenAnyValue(x => x._navigationService.CurrentViewModel)
            .ToProperty(this, nameof(CurrentPage), out _currentPage);

        MenuItems = new()
        {
            new(_navigationService) { MenuHeader = "游戏管理", PageType = typeof(GameInstancesViewModel) },
            new(_navigationService) { MenuHeader = "链接跳转", PageType = typeof(LinksViewModel) },
            new(_navigationService) { MenuHeader = "关于修改器", PageType = typeof(AboutViewModel) }
        };
        navigationService.NavigateTo<GameInstancesViewModel>();
    }

    [ReactiveCommand]
    public void Closing()
    {
        _modifierInfoService.Save(Locator.Current.GetService<GameInstancesViewModel>()!.MenuItems);
    }

    [Reactive] public partial ModifierAuthorsViewModel ModifierAuthors { get; set; }

    [ReactiveCommand]
    public void OpenGithub() => Process.Start("explorer.exe", "");
}