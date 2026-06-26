using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PVZRHTools.Models;
using PVZRHTools.Services;
using ReactiveUI.SourceGenerators;
using Ursa.Controls;
using Notification = Ursa.Controls.Notification;

namespace PVZRHTools.ViewModels;

public partial class ModListViewModel(
    IModsManagementService modsManagementService,
    INotificationService notificationService) : ViewModelBase
{
    [Reactive] public partial GameInstanceInfo Info { get; set; }
    private IModsManagementService _modsManagementService { get; } = modsManagementService;
    private INotificationService _notificationService { get; } = notificationService;

    [ReactiveCommand]
    private void ToggleMod(ModInfo mod) => _modsManagementService.ToggleMod(Info, mod);

    [ReactiveCommand]
    private void ShowInExplorer(ModInfo mod)
    {
        Process.Start("explorer.exe", $"/select,\"{mod.ModFilePath}\"");
    }

    [ReactiveCommand]
    private void RefreshModList()
    {
        _modsManagementService.SyncModsInfo(Info);
    }

    [ReactiveCommand]
    private async void OpenFileDialog(Control source)
    {
        var filePickerOptions = new FilePickerOpenOptions()
        {
            AllowMultiple = true,
            FileTypeFilter = [new FilePickerFileType("Mod文件") { Patterns = ["*.zip", "*.dll"] }]
        };
        var dialogWindow = source.FindAncestorOfType<CustomDialogWindow>();
        if (dialogWindow == null) return;
        var files = await dialogWindow.StorageProvider.OpenFilePickerAsync(filePickerOptions);
        if (files.Count is 0) return;
        var paths = files
            .Select(p => p.TryGetLocalPath() ?? p.Path.AbsolutePath)
            .ToList();
        await ProcessDroppedFilesAsync(paths);
    }

    [ReactiveCommand]
    private void DragOver(DragEventArgs e)
    {
        // 检查拖拽的数据是否包含文件
        e.DragEffects = e.DataTransfer.Contains(DataFormat.File) ? DragDropEffects.Copy : DragDropEffects.None;
    }

    /// <summary>
    /// 处理文件拖放事件
    /// </summary>
    [ReactiveCommand]
    private async Task DropAsync(DragEventArgs e)
    {
        if (e.DataTransfer.Contains(DataFormat.File))
        {
            var files = e.DataTransfer.Items;

            var filePaths = files
                .Select(file => file.TryGetFile()?.TryGetLocalPath() ?? string.Empty)
                .ToList();

            // 处理拖放的文件
            await ProcessDroppedFilesAsync(filePaths);
        }
    }

    /// <summary>
    /// 处理拖放的文件列表
    /// </summary>
    private async Task ProcessDroppedFilesAsync(List<string> filePaths)
    {
        var validPaths = (from path in filePaths
                where !string.IsNullOrEmpty(path)
                where path.EndsWith(".zip", System.StringComparison.OrdinalIgnoreCase) ||
                      path.EndsWith(".dll", System.StringComparison.OrdinalIgnoreCase)
                select path)
            .ToList();

        if (validPaths.Count is 0) return;

        // 文件操作（复制 DLL 到游戏目录）在后台线程执行
        await Task.Run(() => _modsManagementService.AddMods(Info, validPaths));

        // UI 更新必须在 UI 线程执行，避免 ObservableCollection 跨线程操作导致列表显示异常
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            _modsManagementService.SyncModsInfo(Info);
            _notificationService.NotificationManager?.Show(
                new Notification($"已添加{validPaths.Count}个模组", null, NotificationType.Success),
                NotificationType.Success);
        });
    }
}