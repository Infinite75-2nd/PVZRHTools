using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using PVZRHTools.Services;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;
using ToolData;
using Ursa.Controls;

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
            new(_navigationService) { MenuHeader = "链接跳转", PageType = typeof(LinksViewModel) },
            new(_navigationService) { MenuHeader = "关于修改器", PageType = typeof(AboutViewModel) }
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
            var outdatedPaths = _modifierInfoService.GetOutdatedGamePaths();
            foreach (var path in outdatedPaths)
            {
                var dirName = new DirectoryInfo(path).Name;
                var result = await OverlayMessageBox.ShowAsync(
                    $"检测到游戏实例 \"{dirName}\" 中的修改器版本低于当前版本，是否更新？",
                    "更新修改器",
                    icon: MessageBoxIcon.Question,
                    button: MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _gameBootstrapService.InstallModifier(path);
                        _notificationService.NotificationManager?.Show(
                            $"\"{dirName}\" 的修改器已更新完成", NotificationType.Success);
                    }
                    catch (Exception ex)
                    {
                        await OverlayMessageBox.ShowAsync(
                            $"更新修改器失败：{ex.Message}",
                            "更新失败",
                            icon: MessageBoxIcon.Error,
                            button: MessageBoxButton.OK);
                    }
                }
            }
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