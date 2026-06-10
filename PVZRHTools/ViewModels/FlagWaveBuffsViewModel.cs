using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using PVZRHTools.Models;
using PVZRHTools.Services;
using PVZRHTools.Utils;
using ReactiveUI.SourceGenerators;
using ToolData;

namespace PVZRHTools.ViewModels;

public partial class FlagWaveBuffsViewModel : ModifierPageViewModelBase
{
    public IInitDataService InitDataService { get; }
    [Reactive] public partial bool FlagWaveBuffsEnabled { get; set; }
    [Reactive] public partial ObservableCollection<FlagWaveBuffInfo> FlagWaveBuffs { get; set; }

    [ReactiveCommand]
    public void FlagWaveBuffInfoChanged(int wave)
    {
        if (FlagWaveBuffs.Count < wave) return;
        FlagWaveBuff flagWaveBuff = new()
        {
            Wave = wave,
            AdvBuffs = FlagWaveBuffs[wave - 1].AdvBuffs.Select(pair => pair.Key).ToList(),
            UltiBuffs = FlagWaveBuffs[wave - 1].UltiBuffs.Select(pair => pair.Key).ToList(),
            Debuffs = FlagWaveBuffs[wave - 1].Debuffs.Select(pair => pair.Key).ToList(),
            InvestBuffs = FlagWaveBuffs[wave - 1].InvestBuffs.Select(pair => pair.Key).ToList(),
            Description = FlagWaveBuffs[wave - 1].Description
        };
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.FlagWaveBuff,
            Parameters = [JsonSerializer.Serialize(flagWaveBuff, JsonSGC.Default.FlagWaveBuff)]
        });
    }

    public FlagWaveBuffsViewModel(IDataSyncService dataSyncService, IInitDataService initDataService) : base(
        dataSyncService)
    {
        InitDataService = initDataService;
        FlagWaveBuffs = [];
        for (int i = 1; i <= 10; i++)
            FlagWaveBuffs.Add(new FlagWaveBuffInfo()
            {
                Wave = i
            });
        this.SimpleOneWaySync(x => x.FlagWaveBuffsEnabled, Strings.FlagWaveBuffsEnabled);
    }

    public override void SaveSettings(SettingsData settings)
    {
        settings.FlagWaveBuffsEnabled = FlagWaveBuffsEnabled;
        settings.FlagWaveBuffs = FlagWaveBuffs.Select(fb => new FlagWaveBuffSettings
        {
            Wave = fb.Wave,
            AdvBuffs = fb.AdvBuffs.Select(pair => pair.Key).ToList(),
            UltiBuffs = fb.UltiBuffs.Select(pair => pair.Key).ToList(),
            Debuffs = fb.Debuffs.Select(pair => pair.Key).ToList(),
            InvestBuffs = fb.InvestBuffs.Select(pair => pair.Key).ToList(),
            Description = fb.Description
        }).ToList();
    }

    public override void LoadSettings(SettingsData settings)
    {
        FlagWaveBuffsEnabled = settings.FlagWaveBuffsEnabled;
        if (settings.FlagWaveBuffs is { Count: > 0 })
        {
            FlagWaveBuffs.Clear();
            foreach (var fb in settings.FlagWaveBuffs)
            {
                var info = new FlagWaveBuffInfo { Wave = fb.Wave, Description = fb.Description };

                // 还原AdvBuffs

                foreach (var advId in fb.AdvBuffs)
                {
                    var name = InitDataService.InitData.AdvBuffs.FirstOrDefault(x => x.Key == advId).Value ?? "";
                    info.AdvBuffs.Add(new KeyValuePair<int, string>(advId, name));
                }


                // 还原UltiBuffs

                foreach (var ultiId in fb.UltiBuffs)
                {
                    var name = InitDataService.InitData.UltiBuffs.FirstOrDefault(x => x.Key == ultiId).Value ?? "";
                    info.UltiBuffs.Add(new KeyValuePair<int, string>(ultiId, name));
                }

                // 还原Debuffs

                foreach (var debuffId in fb.Debuffs)
                {
                    var name = InitDataService.InitData.Debuffs.FirstOrDefault(x => x.Key == debuffId).Value ?? "";
                    info.Debuffs.Add(new KeyValuePair<int, string>(debuffId, name));
                }


                // 还原InvestBuffs

                foreach (var investId in fb.InvestBuffs)
                {
                    var name = InitDataService.InitData.InvestBuffs.FirstOrDefault(x => x.Key == investId).Value ??
                               "";
                    info.InvestBuffs.Add(new KeyValuePair<int, string>(investId, name));
                }


                FlagWaveBuffs.Add(info);
            }
        }
    }
}