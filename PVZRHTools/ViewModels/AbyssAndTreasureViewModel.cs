using PVZRHTools.Services;
using PVZRHTools.Utils;
using ReactiveUI.SourceGenerators;
using ToolData;

namespace PVZRHTools.ViewModels;

public partial class AbyssAndTreasureViewModel:ModifierPageViewModelBase
{
    public AbyssAndTreasureViewModel(IDataSyncService dataSyncService) : base(dataSyncService)
    {
        this.SimpleOneWaySync(x => x.TreasureFreeUpgrade, Strings.TreasureFreeUpgrade);
        this.SimpleOneWaySync(x => x.TreasureFreeWithdraw, Strings.TreasureFreeWithdraw);
        this.SimpleSyncFlaggedInt(x => x.TreasureMaxTime, x => x.TreasureMaxTimeEnabled, Strings.TreasureMaxTime);
        this.SimpleOneWaySync(x => x.TreasureAllRedCard, Strings.TreasureAllRedCard);

        // 深渊模式同步
        this.SimpleOneWaySync(x => x.AbyssLimitlessRefresh, Strings.AbyssLimitlessRefresh);
        this.SimpleOneWaySync(x => x.AbyssRemoveSuperSunNutLimit, Strings.AbyssRemoveSuperSunNutLimit);
        this.SimpleSyncFlaggedInt(x => x.AbyssMaxPlantCount, x => x.AbyssMaxPlantCountEnabled, Strings.AbyssMaxPlantCount);
        this.SimpleSyncFlaggedInt(x => x.AbyssMaxSuperCount, x => x.AbyssMaxSuperCountEnabled, Strings.AbyssMaxSuperCount);
        this.SimpleSyncFlaggedInt(x => x.AbyssMaxUltimateCount, x => x.AbyssMaxUltimateCountEnabled, Strings.AbyssMaxUltimateCount);
    }

    [ReactiveCommand]
    public void TreasureSetMoney() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.TreasureSetMoney,
            Parameters = [TreasureMoney.ToString()]
        });

    [ReactiveCommand]
    public void TreasureFillCard() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.TreasureFillCard,
            Parameters = []
        });

    [ReactiveCommand]
    public void AbyssSetJumpLevel() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.AbyssJumpLevel,
            Parameters = [AbyssJumpLevel.ToString()]
        });

    [ReactiveCommand]
    public void AbyssSetMoney() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.AbyssMoney,
            Parameters = [AbyssMoney.ToString()]
        });

    [ReactiveCommand]
    public void EnterWIPAbyss()
    {
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.AbyssJumpLevel,
            Parameters = []
        });
    }

    #region 深渊模式修改

    
    
    [Reactive] public partial int AbyssJumpLevel { get; set; }
    [Reactive] public partial int AbyssMoney { get; set; }
    [Reactive] public partial bool AbyssLimitlessRefresh { get; set; }
    [Reactive] public partial bool AbyssRemoveSuperSunNutLimit { get; set; }
    [Reactive] public partial bool AbyssMaxPlantCountEnabled { get; set; }
    [Reactive] public partial int AbyssMaxPlantCount { get; set; }
    [Reactive] public partial bool AbyssMaxSuperCountEnabled { get; set; }
    [Reactive] public partial int AbyssMaxSuperCount { get; set; }
    [Reactive] public partial bool AbyssMaxUltimateCountEnabled { get; set; }
    [Reactive] public partial int AbyssMaxUltimateCount { get; set; }

    #endregion

    #region 神秘模式修改

    [Reactive] public partial int TreasureMoney { get; set; }
    [Reactive] public partial bool TreasureFreeUpgrade { get; set; }
    [Reactive] public partial bool TreasureFreeWithdraw { get; set; }
    [Reactive] public partial bool TreasureMaxTimeEnabled { get; set; }
    [Reactive] public partial int TreasureMaxTime { get; set; } = 1500;
    [Reactive] public partial bool TreasureAllRedCard { get; set; }

    #endregion

    public override void SaveSettings(SettingsData settings)
    {
        settings.TreasureMoney = TreasureMoney;
        settings.TreasureFreeUpgrade = TreasureFreeUpgrade;
        settings.TreasureFreeWithdraw = TreasureFreeWithdraw;
        settings.TreasureMaxTimeEnabled = TreasureMaxTimeEnabled;
        settings.TreasureMaxTime = TreasureMaxTime;
        settings.TreasureAllRedCard = TreasureAllRedCard;

        settings.AbyssJumpLevel = AbyssJumpLevel;
        settings.AbyssMoney = AbyssMoney;
        settings.AbyssLimitlessRefresh = AbyssLimitlessRefresh;
        settings.AbyssRemoveSuperSunNutLimit = AbyssRemoveSuperSunNutLimit;
        settings.AbyssMaxPlantCountEnabled = AbyssMaxPlantCountEnabled;
        settings.AbyssMaxPlantCount = AbyssMaxPlantCount;
        settings.AbyssMaxSuperCountEnabled = AbyssMaxSuperCountEnabled;
        settings.AbyssMaxSuperCount = AbyssMaxSuperCount;
        settings.AbyssMaxUltimateCountEnabled = AbyssMaxUltimateCountEnabled;
        settings.AbyssMaxUltimateCount = AbyssMaxUltimateCount;
    }

    public override void LoadSettings(SettingsData settings)
    {
        TreasureMoney = settings.TreasureMoney;
        TreasureFreeUpgrade = settings.TreasureFreeUpgrade;
        TreasureFreeWithdraw = settings.TreasureFreeWithdraw;
        TreasureMaxTimeEnabled = settings.TreasureMaxTimeEnabled;
        TreasureMaxTime = settings.TreasureMaxTime;
        TreasureAllRedCard = settings.TreasureAllRedCard;

        AbyssJumpLevel = settings.AbyssJumpLevel;
        AbyssMoney = settings.AbyssMoney;
        AbyssLimitlessRefresh = settings.AbyssLimitlessRefresh;
        AbyssRemoveSuperSunNutLimit = settings.AbyssRemoveSuperSunNutLimit;
        AbyssMaxPlantCountEnabled = settings.AbyssMaxPlantCountEnabled;
        AbyssMaxPlantCount = settings.AbyssMaxPlantCount;
        AbyssMaxSuperCountEnabled = settings.AbyssMaxSuperCountEnabled;
        AbyssMaxSuperCount = settings.AbyssMaxSuperCount;
        AbyssMaxUltimateCountEnabled = settings.AbyssMaxUltimateCountEnabled;
        AbyssMaxUltimateCount = settings.AbyssMaxUltimateCount;
    }
}