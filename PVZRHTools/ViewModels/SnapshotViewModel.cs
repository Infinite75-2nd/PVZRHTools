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
    [Reactive] public partial string SnapshotString { get; set; } = "";

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
        try
        {
            var path = Path.Combine(App.GamePath, Paths.LatestSnapshotPath);
            if (!File.Exists(path))
            {
                SnapshotString = "暂无快照文件";
                return;
            }

            var json = File.ReadAllText(path);
            var snapshot = JsonSerializer.Deserialize(json, JsonSGC.Default.Snapshot);
            if (snapshot is null) return;
            var sb = new System.Text.StringBuilder();
            var captured = snapshot.CapturedAt;
            if (captured != DateTime.MinValue) sb.AppendLine($"捕获时间：{captured.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"关卡类型：{snapshot.BoardType}    关卡编号：{snapshot.LevelNumber}    生存轮次：{snapshot.SurvivalRound}");
            sb.AppendLine($"波次：{snapshot.Wave}/{snapshot.MaxWave}    旗帜波：{(snapshot.IsHugeWave ? "是" : "否")}");
            sb.AppendLine($"距下波时间：{snapshot.TimeUntilNextWave:F2}    波间隔：{snapshot.WaveInterval:F2}");
            sb.AppendLine($"阳光：{snapshot.Sun}    金币：{snapshot.Money}");
            sb.AppendLine(
                $"BoardTag：种子雨={snapshot.BoardTag.IsSeedRain}  列种={snapshot.BoardTag.IsColumn}  胆小菇梦境={snapshot.BoardTag.IsScaredyDream}");
            sb.AppendLine(
                $"高级词条：{snapshot.AdvOn.Count}    究极词条：{snapshot.UltiOn.Count}    负面词条：{snapshot.DebuffOn.Count}    投资词条：{snapshot.InvestOn.Count}");
            sb.AppendLine($"格子物品：{snapshot.GridItems.Count}    罐子：{snapshot.Vases.Count}");
            sb.AppendLine(
                $"卡槽：{snapshot.CardBank.Count}    卡片冷却：{snapshot.CardCDs.Count}    掉落物冷却：{snapshot.DroppedCDs.Count}");
            sb.AppendLine($"植物血量：{snapshot.PlantHealths.Count}    僵尸血量：{snapshot.ZombieHealths.Count}");
            sb.AppendLine($"随机种子：{snapshot.RandomSeed}");
            SnapshotString = sb.ToString();
        }
        catch (Exception ex)
        {
            SnapshotString = $"读取失败: {ex.Message}";
        }
    }
}