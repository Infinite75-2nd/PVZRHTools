using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using PVZRHTools.Services;
using PVZRHTools.Utils;
using ReactiveUI.SourceGenerators;
using ToolData;

namespace PVZRHTools.ViewModels;

public partial class SnapshotViewModel(IDataSyncService dataSyncService) : ModifierPageViewModelBase(dataSyncService)
{
    [Reactive] public partial bool HasSnapshotData { get; set; }
    [Reactive] public partial string StatusText { get; set; } = "";

    // 基本信息
    [Reactive] public partial string CapturedAtText { get; set; } = "";
    [Reactive] public partial string BoardTypeText { get; set; } = "";
    [Reactive] public partial string LevelNumberText { get; set; } = "";
    [Reactive] public partial string SurvivalRoundText { get; set; } = "";

    // 波次信息
    [Reactive] public partial string WaveDisplayText { get; set; } = "";
    [Reactive] public partial string IsHugeWaveText { get; set; } = "";
    [Reactive] public partial string TimeUntilNextWaveText { get; set; } = "";
    [Reactive] public partial string WaveIntervalText { get; set; } = "";

    // 资源
    [Reactive] public partial string SunText { get; set; } = "";
    [Reactive] public partial string MoneyText { get; set; } = "";

    // BoardTag
    [Reactive] public partial string IsSeedRainText { get; set; } = "";
    [Reactive] public partial string IsColumnText { get; set; } = "";
    [Reactive] public partial string IsScaredyDreamText { get; set; } = "";

    // 词条计数
    [Reactive] public partial string AdvOnCountText { get; set; } = "";
    [Reactive] public partial string UltiOnCountText { get; set; } = "";
    [Reactive] public partial string DebuffOnCountText { get; set; } = "";
    [Reactive] public partial string InvestOnCountText { get; set; } = "";

    // 物品
    [Reactive] public partial string GridItemsCountText { get; set; } = "";
    [Reactive] public partial string VasesCountText { get; set; } = "";

    // 卡槽/冷却
    [Reactive] public partial string CardBankCountText { get; set; } = "";
    [Reactive] public partial string CardCDsCountText { get; set; } = "";
    [Reactive] public partial string DroppedCDsCountText { get; set; } = "";

    // 血量
    [Reactive] public partial string PlantHealthsCountText { get; set; } = "";
    [Reactive] public partial string ZombieHealthsCountText { get; set; } = "";

    // 其他
    [Reactive] public partial string RandomSeedText { get; set; } = "";

    [ReactiveCommand]
    public async Task GetSnapshot()
    {
        await DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.GetSnapshot,
            Parameters = []
        });
        await Task.Delay(400);
        RefreshSnapshotInfo();
    }

    [ReactiveCommand]
    public void RestoreSnapshot()
    {
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.RestoreSnapshot,
            Parameters = []
        });
        RefreshSnapshotInfo();
    }

    [ReactiveCommand]
    public void RefreshSnapshotInfo()
    {
        HasSnapshotData = false;
        try
        {
            var path = Path.Combine(App.GamePath, Paths.LatestSnapshotPath);
            if (!File.Exists(path))
            {
                StatusText = "暂无快照文件";
                return;
            }

            var json = File.ReadAllText(path);
            var snapshot = JsonSerializer.Deserialize(json, JsonSGC.Default.Snapshot);
            if (snapshot is null) return;

            var captured = snapshot.CapturedAt;
            CapturedAtText = captured != DateTime.MinValue
                ? captured.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")
                : "";
            BoardTypeText = snapshot.BoardType.ToString();
            LevelNumberText = snapshot.LevelNumber.ToString();
            SurvivalRoundText = snapshot.SurvivalRound.ToString();
            WaveDisplayText = $"{snapshot.Wave} / {snapshot.MaxWave}";
            IsHugeWaveText = snapshot.IsHugeWave ? "是" : "否";
            TimeUntilNextWaveText = snapshot.TimeUntilNextWave.ToString("F2");
            WaveIntervalText = snapshot.WaveInterval.ToString("F2");
            SunText = snapshot.Sun.ToString();
            MoneyText = snapshot.Money.ToString();
            IsSeedRainText = snapshot.BoardTag.IsSeedRain ? "是" : "否";
            IsColumnText = snapshot.BoardTag.IsColumn ? "是" : "否";
            IsScaredyDreamText = snapshot.BoardTag.IsScaredyDream ? "是" : "否";
            AdvOnCountText = snapshot.AdvOn.Count.ToString();
            UltiOnCountText = snapshot.UltiOn.Count.ToString();
            DebuffOnCountText = snapshot.DebuffOn.Count.ToString();
            InvestOnCountText = snapshot.InvestOn.Count.ToString();
            GridItemsCountText = snapshot.GridItems.Count.ToString();
            VasesCountText = snapshot.Vases.Count.ToString();
            CardBankCountText = snapshot.CardBank.Count.ToString();
            CardCDsCountText = snapshot.CardCDs.Count.ToString();
            DroppedCDsCountText = snapshot.DroppedCDs.Count.ToString();
            PlantHealthsCountText = snapshot.PlantHealths.Count.ToString();
            ZombieHealthsCountText = snapshot.ZombieHealths.Count.ToString();
            RandomSeedText = snapshot.RandomSeed.ToString();
            HasSnapshotData = true;
        }
        catch (Exception ex)
        {
            StatusText = $"读取失败: {ex.Message}";
        }
    }
}