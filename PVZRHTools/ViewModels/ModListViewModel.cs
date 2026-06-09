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
    private async void OpenFileDialog(Control source)
    {
        var filePickerOptions = new FilePickerOpenOptions()
        {
            AllowMultiple = true,
            FileTypeFilter = [new FilePickerFileType("Mod文件") { Patterns = ["*.zip", "*.dll"] }]
        };
        var paths = (await source.FindAncestorOfType<CustomDialogWindow>()!
                .StorageProvider
                .OpenFilePickerAsync(filePickerOptions))
            .Select(p => p.Path.AbsolutePath).ToList();
        if (paths.Count is 0) return;
        await Task.Run(() => ProcessDroppedFilesAsync(paths));
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

            var filePaths =
                files.Select(file => file.TryGetFile()?.Path.AbsolutePath ?? string.Empty)
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
                select path.Replace("%20", " "))
            .ToList();

        await Task.Run(() =>
        {
            _modsManagementService.AddMods(Info, validPaths);
            _modsManagementService.SyncModsInfo(Info);
            if (validPaths.Count > 0)
                Dispatcher.UIThread.InvokeAsync(() =>
                    _notificationService.NotificationManager?.Show(
                        new Notification($"已添加{validPaths.Count}个模组", null, NotificationType.Success),
                        NotificationType.Success));
        });
    }
}