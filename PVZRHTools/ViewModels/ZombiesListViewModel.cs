using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using Avalonia.Threading;
using PVZRHTools.Models;
using PVZRHTools.Services;
using PVZRHTools.Utils;
using ReactiveUI.SourceGenerators;
using ToolData;

namespace PVZRHTools.ViewModels;

public partial class ZombiesListViewModel : ModifierPageViewModelBase
{
    public IInitDataService InitDataService { get; set; }
    private INotificationService _notificationService { get; set; }
    private INavigationService _navigationService { get; set; }
    [Reactive] public partial bool IsLoading { get; set; }
    [Reactive] public partial int CurrentWave { get; set; } = -1;
    [Reactive] public partial string CurrentWaveString { get; set; } = "未获取";

    [Reactive] public partial ObservableCollection<WaveZombiesItem> ZombiesList { get; set; } = [];

    public ZombiesListViewModel(IDataSyncService dataSyncService, IInitDataService initDataService,
        INotificationService notificationService, INavigationService navigationService) : base(
        dataSyncService)
    {
        InitDataService = initDataService;
        _notificationService = notificationService;
        _navigationService = navigationService;
        dataSyncService.MessageReceived += MessageReceived;
    }

    public void MessageReceived(object? sender, SyncData data)
    {
        if (data.Command is Strings.GetZombiesList)
        {
            var zldata = JsonSerializer.Deserialize(StringCompressor.Decompress(data.Parameters[0]),
                JsonSGC.Default.ZombiesListData);
            Dispatcher.UIThread.Invoke(() =>
            {
                ZombiesList.Clear();
                foreach (var kvp in zldata.ZombiesList)
                {
                    ZombiesList.Add(new WaveZombiesItem
                    {
                        Wave = kvp.Key,
                        Zombies = kvp.Value,
                        IsCurrentWave = kvp.Key == zldata.CurrentWave
                    });
                }

                CurrentWave = zldata.CurrentWave;
                CurrentWaveString = CurrentWave < 0 ? "未获取" : $"当前为第{CurrentWave}波";
                IsLoading = false;
            });
        }
    }

    [ReactiveCommand]
    public void ChangeZombiesList((int wave, int index, int type) args)
    {
        try
        {
            // 查找对应的 Wave 项
            var waveItem = ZombiesList.FirstOrDefault(z => z.Wave == args.wave);
            if (waveItem != null && waveItem.Zombies.ContainsKey(args.index))
            {
                // 更新本地数据
                waveItem.Zombies[args.index] = args.type;

                // 发送到游戏
                SyncData data = new()
                {
                    Command = Strings.ChangeZombiesList,
                    Parameters =
                        [args.wave.ToString(), args.index.ToString(), args.type.ToString()]
                };
                DataSyncService.SendCommand(data);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ChangeZombiesList error: {ex.Message}");
        }
    }

    [ReactiveCommand]
    public void GetZombiesList()
    {
        SyncData data = new()
        {
            Command = Strings.GetZombiesList,
            Parameters = []
        };
        DataSyncService.SendCommand(data);
        IsLoading = true;
    }
}