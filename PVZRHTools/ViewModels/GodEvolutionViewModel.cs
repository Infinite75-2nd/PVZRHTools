using PVZRHTools.Services;
using PVZRHTools.Utils;
using ReactiveUI.SourceGenerators;
using ToolData;

namespace PVZRHTools.ViewModels;

public partial class GodEvolutionViewModel : ModifierPageViewModelBase
{
    [Reactive] public partial bool GodEvolutionUnlimitedRefresh { get; set; }
    [Reactive] public partial bool GodEvolutionFreeUpgradeQuality { get; set; }
    [Reactive] public partial bool GodEvolutionLuckyEnabled { get; set; }
    [Reactive] public partial double GodEvolutionLucky { get; set; } = 0;
    [Reactive] public partial bool GodEvolutionDifficultyEnabled { get; set; }
    [Reactive] public partial int GodEvolutionDifficulty { get; set; }
    [Reactive] public partial bool GodEvolutionRefreshCountEnabled { get; set; }
    [Reactive] public partial int GodEvolutionRefreshCount { get; set; } = 99999999;
    [Reactive] public partial bool GodEvolutionMaxPlantCountEnabled { get; set; }
    [Reactive] public partial int GodEvolutionMaxPlantCount { get; set; } = 5;
    [Reactive] public partial bool GodEvolutionSuperUpgrade { get; set; }
    [Reactive] public partial bool GodEvolutionForceSuperQuality { get; set; }
    [Reactive] public partial bool GodEvolutionUncrashable { get; set; }
    [Reactive] public partial bool GodEvolutionQualityWeightEnabled { get; set; }
    [Reactive] public partial double GodEvolutionQualityDefault { get; set; } = 65;
    [Reactive] public partial double GodEvolutionQualitySilver { get; set; } = 23;
    [Reactive] public partial double GodEvolutionQualityGold { get; set; } = 10;
    [Reactive] public partial double GodEvolutionQualityDiamond { get; set; } = 2;
    [Reactive] public partial bool GodEvolutionDamageMultiplierEnabled { get; set; }
    [Reactive] public partial double GodEvolutionDamageMultiplier { get; set; } = 1.0;
    [Reactive] public partial bool GodEvolutionMultiSelectBuff { get; set; }
    [Reactive] public partial bool GodEvolutionCheatHard { get; set; }
    [Reactive] public partial bool GodEvolutionForceExpertBuff { get; set; }

    [ReactiveCommand]
    public void GodEvolutionResetQuality()
    {
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.GodEvolutionResetQuality,
            Parameters = []
        });
    }

    [ReactiveCommand]
    public void GodEvolutionUnlockAll()
    {
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.GodEvolutionUnlockAll,
            Parameters = []
        });
    }

    [ReactiveCommand]
    public void GodEvolutionChooseBuff()
    {
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.GodEvolutionChooseBuff,
            Parameters = []
        });
    }

    public GodEvolutionViewModel(IDataSyncService dataSyncService) : base(dataSyncService)
    {
        // 刷新与升级
        this.SimpleOneWaySync(x => x.GodEvolutionUnlimitedRefresh, Strings.GodEvolutionUnlimitedRefresh);
        this.SimpleOneWaySync(x => x.GodEvolutionFreeUpgradeQuality, Strings.GodEvolutionFreeUpgradeQuality);
        this.SimpleSyncFlaggedInt(x => x.GodEvolutionRefreshCount, x => x.GodEvolutionRefreshCountEnabled,
            Strings.GodEvolutionRefreshCount);

        // 局内属性
        this.SimpleSyncFlaggedDouble(x => x.GodEvolutionLucky, x => x.GodEvolutionLuckyEnabled,
            Strings.GodEvolutionLucky);
        this.SimpleSyncFlaggedInt(x => x.GodEvolutionDifficulty, x => x.GodEvolutionDifficultyEnabled,
            Strings.GodEvolutionDifficulty);
        this.SimpleSyncFlaggedInt(x => x.GodEvolutionMaxPlantCount, x => x.GodEvolutionMaxPlantCountEnabled,
            Strings.GodEvolutionMaxPlantCount);
        this.SimpleOneWaySync(x => x.GodEvolutionSuperUpgrade, Strings.GodEvolutionSuperUpgrade);
        this.SimpleOneWaySync(x => x.GodEvolutionForceSuperQuality, Strings.GodEvolutionForceSuperQuality);
        this.SimpleOneWaySync(x => x.GodEvolutionUncrashable, Strings.GodEvolutionUncrashable);
        this.SimpleSyncFlaggedDouble(x => x.GodEvolutionDamageMultiplier, x => x.GodEvolutionDamageMultiplierEnabled,
            Strings.GodEvolutionDamageMultiplier);
        this.SimpleOneWaySync(x => x.GodEvolutionMultiSelectBuff, Strings.GodEvolutionMultiSelectBuff);
        this.SimpleOneWaySync(x => x.GodEvolutionCheatHard, Strings.GodEvolutionCheatHard);
        this.SimpleOneWaySync(x => x.GodEvolutionForceExpertBuff, Strings.GodEvolutionForceExpertBuff);

        // 词条品质权重
        this.SimpleOneWaySync(x => x.GodEvolutionQualityWeightEnabled, Strings.GodEvolutionQualityWeightEnabled);
        this.SimpleOneWaySync(x => x.GodEvolutionQualityDefault, Strings.GodEvolutionQualityDefault);
        this.SimpleOneWaySync(x => x.GodEvolutionQualitySilver, Strings.GodEvolutionQualitySilver);
        this.SimpleOneWaySync(x => x.GodEvolutionQualityGold, Strings.GodEvolutionQualityGold);
        this.SimpleOneWaySync(x => x.GodEvolutionQualityDiamond, Strings.GodEvolutionQualityDiamond);
    }

    public override void SaveSettings(SettingsData settings)
    {
        settings.GodEvolutionUnlimitedRefresh = GodEvolutionUnlimitedRefresh;
        settings.GodEvolutionFreeUpgradeQuality = GodEvolutionFreeUpgradeQuality;
        settings.GodEvolutionLuckyEnabled = GodEvolutionLuckyEnabled;
        settings.GodEvolutionLucky = GodEvolutionLucky;
        settings.GodEvolutionDifficultyEnabled = GodEvolutionDifficultyEnabled;
        settings.GodEvolutionDifficulty = GodEvolutionDifficulty;
        settings.GodEvolutionRefreshCountEnabled = GodEvolutionRefreshCountEnabled;
        settings.GodEvolutionRefreshCount = GodEvolutionRefreshCount;
        settings.GodEvolutionMaxPlantCountEnabled = GodEvolutionMaxPlantCountEnabled;
        settings.GodEvolutionMaxPlantCount = GodEvolutionMaxPlantCount;
        settings.GodEvolutionSuperUpgrade = GodEvolutionSuperUpgrade;
        settings.GodEvolutionForceSuperQuality = GodEvolutionForceSuperQuality;
        settings.GodEvolutionUncrashable = GodEvolutionUncrashable;
        settings.GodEvolutionQualityWeightEnabled = GodEvolutionQualityWeightEnabled;
        settings.GodEvolutionQualityDefault = GodEvolutionQualityDefault;
        settings.GodEvolutionQualitySilver = GodEvolutionQualitySilver;
        settings.GodEvolutionQualityGold = GodEvolutionQualityGold;
        settings.GodEvolutionQualityDiamond = GodEvolutionQualityDiamond;
        settings.GodEvolutionDamageMultiplierEnabled = GodEvolutionDamageMultiplierEnabled;
        settings.GodEvolutionDamageMultiplier = GodEvolutionDamageMultiplier;
        settings.GodEvolutionMultiSelectBuff = GodEvolutionMultiSelectBuff;
        settings.GodEvolutionCheatHard = GodEvolutionCheatHard;
        settings.GodEvolutionForceExpertBuff = GodEvolutionForceExpertBuff;
    }

    public override void LoadSettings(SettingsData settings)
    {
        GodEvolutionUnlimitedRefresh = settings.GodEvolutionUnlimitedRefresh;
        GodEvolutionFreeUpgradeQuality = settings.GodEvolutionFreeUpgradeQuality;
        GodEvolutionLuckyEnabled = settings.GodEvolutionLuckyEnabled;
        GodEvolutionLucky = settings.GodEvolutionLucky;
        GodEvolutionDifficultyEnabled = settings.GodEvolutionDifficultyEnabled;
        GodEvolutionDifficulty = settings.GodEvolutionDifficulty;
        GodEvolutionRefreshCountEnabled = settings.GodEvolutionRefreshCountEnabled;
        GodEvolutionRefreshCount = settings.GodEvolutionRefreshCount;
        GodEvolutionMaxPlantCountEnabled = settings.GodEvolutionMaxPlantCountEnabled;
        GodEvolutionMaxPlantCount = settings.GodEvolutionMaxPlantCount;
        GodEvolutionSuperUpgrade = settings.GodEvolutionSuperUpgrade;
        GodEvolutionForceSuperQuality = settings.GodEvolutionForceSuperQuality;
        GodEvolutionUncrashable = settings.GodEvolutionUncrashable;
        GodEvolutionQualityWeightEnabled = settings.GodEvolutionQualityWeightEnabled;
        GodEvolutionQualityDefault = settings.GodEvolutionQualityDefault;
        GodEvolutionQualitySilver = settings.GodEvolutionQualitySilver;
        GodEvolutionQualityGold = settings.GodEvolutionQualityGold;
        GodEvolutionQualityDiamond = settings.GodEvolutionQualityDiamond;
        GodEvolutionDamageMultiplierEnabled = settings.GodEvolutionDamageMultiplierEnabled;
        GodEvolutionDamageMultiplier = settings.GodEvolutionDamageMultiplier;
        GodEvolutionMultiSelectBuff = settings.GodEvolutionMultiSelectBuff;
        GodEvolutionCheatHard = settings.GodEvolutionCheatHard;
        GodEvolutionForceExpertBuff = settings.GodEvolutionForceExpertBuff;
    }
}