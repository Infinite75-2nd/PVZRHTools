using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using PVZRHTools.Models;
using PVZRHTools.Services;
using PVZRHTools.Views;
using ReactiveUI.SourceGenerators;
using Splat;
using ToolData;
using PVZRHTools.Utils;
using Ursa.Controls;
using Ursa.Controls.Options;

namespace PVZRHTools.ViewModels;

public partial class GameInstancesViewModel : ViewModelBase
{
    [Reactive] public partial ObservableCollection<GameInstanceInfo> MenuItems { get; set; }
    //public string Title { get; init; } = $"当前适配版本：{Strings.GameVersion}";
    private IRunGameService _runGameService { get; set; }
    private IModifierInfoService _modifierInfoService { get; set; }
    private IGameBootstrapService _gameBootstrapService { get; set; }
    private INotificationService _notificationService { get; set; }

    public GameInstancesViewModel(IRunGameService runGameService, IModifierInfoService modifierInfoService,
        IGameBootstrapService gameBootstrapService, INotificationService notificationService)
    {
        _runGameService = runGameService;
        _modifierInfoService = modifierInfoService;
        _gameBootstrapService = gameBootstrapService;
        _notificationService = notificationService;
        MenuItems = _modifierInfoService.InitGameInstanceInfos();
    }

    [ReactiveCommand]
    public void Unload() => _modifierInfoService.Save(MenuItems);

    [ReactiveCommand]
    public void RunGame(GameInstanceInfo info) => _runGameService.RunGame(info);

    [ReactiveCommand]
    public async Task ShowModList(GameInstanceInfo info)
    {
        var options = new DialogOptions()
        {
            Title = $"模组管理 - {new DirectoryInfo(info.GameRootPath).Name}",
            Mode = DialogMode.None,
            IsCloseButtonVisible = true,
            ShowInTaskBar = true,
            CanResize = true,
            Button = DialogButton.None
        };
        var vm = Locator.Current.GetService<ModListViewModel>();
        vm!.Info = info;
        await Dialog.ShowStandardAsync<ModListView, ModListViewModel>(vm, null, options);
    }

    [ReactiveCommand]
    public async Task ShowLaunchSettings(GameInstanceInfo info)
    {
        var vm = Locator.Current.GetService<LaunchSettingsViewModel>();
        vm!.Info = info;
        var options = new DrawerOptions()
        {
            Title = "启动设置",
            Buttons = DialogButton.None
        };
        await OverlayDrawer.ShowStandardAsync<LaunchSettingsView, LaunchSettingsViewModel>(vm, null, options);
        _modifierInfoService.WriteBootConfig(info);
    }

    [ReactiveCommand]
    public async Task RemoveGameInstance(GameInstanceInfo info)
    {
        var result = await OverlayMessageBox.ShowAsync(
            $"确定要删除游戏实例 \"{new DirectoryInfo(info.GameRootPath).Name}\" 吗？",
            "确认删除",
            icon: MessageBoxIcon.Question,
            button: MessageBoxButton.YesNo);
        if (result != MessageBoxResult.Yes) return;
        _notificationService.NotificationManager?.Show($"已移除游戏实例 {new DirectoryInfo(info.GameRootPath).Name}", NotificationType.Success);
        _modifierInfoService.RemoveGamePath(info.GameRootPath);
        MenuItems = _modifierInfoService.InitGameInstanceInfos();
    }

    [ReactiveCommand]
    public void OpenGameRootFolder(GameInstanceInfo info)
    {
        Process.Start("explorer.exe", info.GameRootPath);
    }

    [ReactiveCommand]
    public void OpenPluginsFolder(GameInstanceInfo info)
    {
        Process.Start("explorer.exe", Path.Combine(info.GameRootPath, Paths.PluginsPath));
    }

    [ReactiveCommand]
    public void OpenGameSaveFolder() => Process.Start("explorer.exe", Paths.GameDataPath);

    [ReactiveCommand]
    public async Task DetectModifierDirectory()
    {
        var dir = Path.GetDirectoryName(Environment.ProcessPath!);
        if (dir == null) return;
        var result = _modifierInfoService.TryAddGamePath(dir);
        switch (result)
        {
            case AddGamePathResult.InvalidDirectory:
                _notificationService.NotificationManager?.Show("未在当前目录检测到有效的游戏文件", NotificationType.Warning);
                break;
            case AddGamePathResult.AlreadyExists:
                _notificationService.NotificationManager?.Show("该目录已在游戏实例列表中", NotificationType.Information);
                await HandleExistingDirectoryAsync(dir);
                break;
            case AddGamePathResult.Added:
                _notificationService.NotificationManager?.Show("已检测到修改器所在目录的游戏实例", NotificationType.Success);
                await HandlePostAddAsync(dir);
                break;
        }

        MenuItems = _modifierInfoService.InitGameInstanceInfos();
    }

    [ReactiveCommand]
    public async Task DetectRunningGame()
    {
        var processes = Process.GetProcessesByName("PlantsVsZombiesRH");
        if (processes.Length == 0)
        {
            _notificationService.NotificationManager?.Show("未检测到正在运行的游戏进程", NotificationType.Warning);
            return;
        }

        foreach (var process in processes)
        {
            try
            {
                var dir = Path.GetDirectoryName(process.MainModule?.FileName);
                if (dir == null) continue;
                var result = _modifierInfoService.TryAddGamePath(dir);
                switch (result)
                {
                    case AddGamePathResult.Added:
                        _notificationService.NotificationManager?.Show("已检测到运行中游戏实例", NotificationType.Success);
                        await HandlePostAddAsync(dir);
                        break;
                    case AddGamePathResult.AlreadyExists:
                        _notificationService.NotificationManager?.Show("该游戏进程目录已在列表中", NotificationType.Information);
                        await HandleExistingDirectoryAsync(dir);
                        break;
                }
            }
            catch
            {
                // ignored
            }
        }

        MenuItems = _modifierInfoService.InitGameInstanceInfos();
    }

    [ReactiveCommand]
    public async Task SelectGameDirectory(Control source)
    {
        var topLevel = TopLevel.GetTopLevel(source);
        if (topLevel == null) return;
        var result = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions());
        if (result.Count == 0) return;
        var path = result[0].Path.LocalPath;
        var addResult = _modifierInfoService.TryAddGamePath(path);
        switch (addResult)
        {
            case AddGamePathResult.InvalidDirectory:
                _notificationService.NotificationManager?.Show("选择的目录中未检测到有效的游戏文件", NotificationType.Warning);
                break;
            case AddGamePathResult.AlreadyExists:
                _notificationService.NotificationManager?.Show("该目录已在游戏实例列表中", NotificationType.Information);
                await HandleExistingDirectoryAsync(path);
                break;
            case AddGamePathResult.Added:
                _notificationService.NotificationManager?.Show("已添加游戏实例", NotificationType.Success);
                await HandlePostAddAsync(path);
                break;
        }

        MenuItems = _modifierInfoService.InitGameInstanceInfos();
    }

    private async Task HandleExistingDirectoryAsync(string gameRootPath)
    {
        if (_gameBootstrapService.IsBepInExInstalled(gameRootPath) &&
            !ToolUtils.GetBepInExEnabled(gameRootPath))
        {
            var confirm = await OverlayMessageBox.ShowAsync(
                "BepInEx已安装但未启用，是否启用？",
                "启用BepInEx",
                icon: MessageBoxIcon.Question,
                button: MessageBoxButton.YesNo);
            if (confirm == MessageBoxResult.Yes)
            {
                try
                {
                    _gameBootstrapService.EnableBepInEx(gameRootPath);
                    _notificationService.NotificationManager?.Show("BepInEx已启用", NotificationType.Success);
                }
                catch (Exception ex)
                {
                    await OverlayMessageBox.ShowAsync(
                        $"启用BepInEx失败：{ex.Message}",
                        "操作失败",
                        icon: MessageBoxIcon.Error,
                        button: MessageBoxButton.OK);
                }
            }
        }
    }

    private async Task HandlePostAddAsync(string gameRootPath)
    {
        if (!_gameBootstrapService.IsBepInExInstalled(gameRootPath))
        {
            var confirm = await OverlayMessageBox.ShowAsync(
                "该目录中未检测到BepInEx环境，是否安装？",
                "安装BepInEx",
                icon: MessageBoxIcon.Question,
                button: MessageBoxButton.YesNo);
            if (confirm == MessageBoxResult.Yes)
            {
                try
                {
                    _gameBootstrapService.InstallBepInEx(gameRootPath);
                    _notificationService.NotificationManager?.Show("BepInEx已安装完成", NotificationType.Success);
                    _gameBootstrapService.EnableBepInEx(gameRootPath);
                }
                catch (Exception ex)
                {
                    await OverlayMessageBox.ShowAsync(
                        $"BepInEx安装失败：{ex.Message}",
                        "安装失败",
                        icon: MessageBoxIcon.Error,
                        button: MessageBoxButton.OK);
                }
            }
        }
        else if (!ToolUtils.GetBepInExEnabled(gameRootPath))
        {
            var confirm = await OverlayMessageBox.ShowAsync(
                "BepInEx已安装但未启用，是否启用？",
                "启用BepInEx",
                icon: MessageBoxIcon.Question,
                button: MessageBoxButton.YesNo);
            if (confirm == MessageBoxResult.Yes)
            {
                try
                {
                    _gameBootstrapService.EnableBepInEx(gameRootPath);
                    _notificationService.NotificationManager?.Show("BepInEx已启用", NotificationType.Success);
                }
                catch (Exception ex)
                {
                    await OverlayMessageBox.ShowAsync(
                        $"启用BepInEx失败：{ex.Message}",
                        "操作失败",
                        icon: MessageBoxIcon.Error,
                        button: MessageBoxButton.OK);
                }
            }
        }

        if (!_gameBootstrapService.IsModifierInstalled(gameRootPath))
        {
            var confirm = await OverlayMessageBox.ShowAsync(
                "该目录中未检测到PVZRHTools修改器，是否安装？",
                "安装PVZRHTools",
                icon: MessageBoxIcon.Question,
                button: MessageBoxButton.YesNo);
            if (confirm == MessageBoxResult.Yes)
            {
                try
                {
                    _gameBootstrapService.InstallModifier(gameRootPath);
                    _notificationService.NotificationManager?.Show("PVZRHTools修改器已安装完成", NotificationType.Success);
                }
                catch (Exception ex)
                {
                    await OverlayMessageBox.ShowAsync(
                        $"PVZRHTools安装失败：{ex.Message}",
                        "安装失败",
                        icon: MessageBoxIcon.Error,
                        button: MessageBoxButton.OK);
                }
            }
        }
    }
}