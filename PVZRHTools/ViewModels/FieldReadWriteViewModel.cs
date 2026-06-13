using ToolData;
using PVZRHTools.Services;
using PVZRHTools.Utils;
using ReactiveUI.SourceGenerators;

namespace PVZRHTools.ViewModels;

public partial class FieldReadWriteViewModel : ModifierPageViewModelBase
{
    public IInitDataService InitDataService { get; }

    public void MessageReceived(object? sender, SyncData data)
    {
        switch (data.Command)
        {
            case Strings.GetPlantFormationCode:
            {
                PlantFormationCode = data.Parameters[0];
                break;
            }
        }
    }

    #region 常规布阵 - 植物布阵

    [Reactive] public partial string PlantFormationCode { get; set; } = string.Empty;
    [Reactive] public partial bool PlantFormationClearField { get; set; }
    [Reactive] public partial bool PlantFormationUseAdvancedFormat { get; set; }

    #endregion

    #region 常规布阵 - 僵尸布阵

    [Reactive] public partial string ZombieFormationCode { get; set; } = string.Empty;
    [Reactive] public partial bool ZombieFormationClearField { get; set; }
    [Reactive] public partial bool ZombieFormationUseAdvancedFormat { get; set; }

    #endregion

    #region 常规布阵 - 混合布阵

    [Reactive] public partial string MixedFormationCode { get; set; } = string.Empty;
    [Reactive] public partial bool MixedFormationClearField { get; set; }

    #endregion

    #region 常规布阵 - 罐子布阵

    [Reactive] public partial string PotFormationCode { get; set; } = string.Empty;
    [Reactive] public partial bool PotFormationClearField { get; set; }

    #endregion

    #region 斗蛐蛐布阵 - 植物礼盒

    [Reactive] public partial int GiftBox1PlantIndex { get; set; } = -1;
    [Reactive] public partial int GiftBox2PlantIndex { get; set; } = -1;
    [Reactive] public partial int GiftBox3PlantIndex { get; set; } = -1;
    [Reactive] public partial int GiftBox4PlantIndex { get; set; } = -1;
    [Reactive] public partial int GiftBox5PlantIndex { get; set; } = -1;

    #endregion

    #region 斗蛐蛐布阵 - 僵尸槽位

    [Reactive] public partial int ZombieSlot1Index { get; set; } = -1;
    [Reactive] public partial int ZombieSlot2Index { get; set; } = -1;
    [Reactive] public partial int ZombieSlot3Index { get; set; } = -1;
    [Reactive] public partial int ZombieSlot4Index { get; set; } = -1;
    [Reactive] public partial int ZombieSlot5Index { get; set; } = -1;
    [Reactive] public partial int ZombieSlot6Index { get; set; } = -1;

    #endregion

    #region 斗蛐蛐布阵 - 其他设置

    [Reactive] public partial bool KillUpgrade { get; set; }
    [Reactive] public partial bool RandomUpgradeMode { get; set; }

    #endregion

    #region Commands

    // 植物布阵命令
    [ReactiveCommand]
    public void GetPlantFormationCode() => DataSyncService.SendCommand(new SyncData()
    {
        Command = Strings.GetPlantFormationCode,
        Parameters = [PlantFormationUseAdvancedFormat.ToString()]
    });

    [ReactiveCommand]
    public void ApplyPlantFormation() => DataSyncService.SendCommand(new SyncData()
    {
        Command = Strings.ApplyPlantFormation,
        Parameters =
            [PlantFormationCode, PlantFormationClearField.ToString(), PlantFormationUseAdvancedFormat.ToString()]
    });

    // 僵尸布阵命令
    [ReactiveCommand]
    public void GetZombieFormationCode() => DataSyncService.SendCommand(new SyncData()
    {
        Command = Strings.GetZombieFormationCode,
        Parameters = [ZombieFormationUseAdvancedFormat.ToString()]
    });

    [ReactiveCommand]
    public void ApplyZombieFormation() => DataSyncService.SendCommand(new SyncData()
    {
        Command = Strings.ApplyZombieFormation,
        Parameters =
            [ZombieFormationCode, ZombieFormationClearField.ToString(), ZombieFormationUseAdvancedFormat.ToString()]
    });

    // 混合布阵命令
    [ReactiveCommand]
    public void GetMixedFormationCode() => DataSyncService.SendCommand(new SyncData()
    {
        Command = Strings.GetMixedFormationCode,
        Parameters = []
    });

    [ReactiveCommand]
    public void ApplyMixedFormation() => DataSyncService.SendCommand(new SyncData()
    {
        Command = Strings.ApplyMixedFormation,
        Parameters = [MixedFormationCode, MixedFormationClearField.ToString()]
    });

    // 罐子布阵命令
    [ReactiveCommand]
    public void GetPotFormationCode() => DataSyncService.SendCommand(new SyncData()
    {
        Command = Strings.GetPotFormationCode,
        Parameters = []
    });

    [ReactiveCommand]
    public void ApplyPotFormation() => DataSyncService.SendCommand(new SyncData()
    {
        Command = Strings.ApplyPotFormation,
        Parameters = [PotFormationCode, PotFormationClearField.ToString()]
    });

    // 斗蛐蛐布阵命令
    [ReactiveCommand]
    public void ApplyBattleFormation() => DataSyncService.SendCommand(new SyncData()
    {
        Command = Strings.ApplyBattleFormation,
        Parameters =
        [
            GiftBox1PlantIndex.ToString(),
            GiftBox2PlantIndex.ToString(),
            GiftBox3PlantIndex.ToString(),
            GiftBox4PlantIndex.ToString(),
            GiftBox5PlantIndex.ToString(),
            ZombieSlot1Index.ToString(),
            ZombieSlot2Index.ToString(),
            ZombieSlot3Index.ToString(),
            ZombieSlot4Index.ToString(),
            ZombieSlot5Index.ToString(),
            ZombieSlot6Index.ToString()
        ]
    });

    #endregion

    public FieldReadWriteViewModel(IDataSyncService dataSyncService, IInitDataService initDataService) : base(
        dataSyncService)
    {
        InitDataService = initDataService;
        DataSyncService.MessageReceived += MessageReceived;
        this.SimpleOneWaySync(x => x.KillUpgrade, Strings.KillUpgrade);
        this.SimpleOneWaySync(x => x.RandomUpgradeMode, Strings.RandomUpgradeMode);
    }

    public override void SaveSettings(SettingsData settings)
    {
        // 常规布阵
        settings.PlantFormationCode = PlantFormationCode;
        settings.PlantFormationClearField = PlantFormationClearField;
        settings.PlantFormationUseAdvancedFormat = PlantFormationUseAdvancedFormat;
        settings.ZombieFormationCode = ZombieFormationCode;
        settings.ZombieFormationClearField = ZombieFormationClearField;
        settings.ZombieFormationUseAdvancedFormat = ZombieFormationUseAdvancedFormat;
        settings.MixedFormationCode = MixedFormationCode;
        settings.MixedFormationClearField = MixedFormationClearField;
        settings.PotFormationCode = PotFormationCode;
        settings.PotFormationClearField = PotFormationClearField;

        // 斗蛐蛐布阵
        settings.GiftBox1PlantIndex = GiftBox1PlantIndex;
        settings.GiftBox2PlantIndex = GiftBox2PlantIndex;
        settings.GiftBox3PlantIndex = GiftBox3PlantIndex;
        settings.GiftBox4PlantIndex = GiftBox4PlantIndex;
        settings.GiftBox5PlantIndex = GiftBox5PlantIndex;
        settings.ZombieSlot1Index = ZombieSlot1Index;
        settings.ZombieSlot2Index = ZombieSlot2Index;
        settings.ZombieSlot3Index = ZombieSlot3Index;
        settings.ZombieSlot4Index = ZombieSlot4Index;
        settings.ZombieSlot5Index = ZombieSlot5Index;
        settings.ZombieSlot6Index = ZombieSlot6Index;
        settings.KillUpgrade = KillUpgrade;
        settings.RandomUpgradeMode = RandomUpgradeMode;
    }

    public override void LoadSettings(SettingsData settings)
    {
        // 常规布阵
        PlantFormationCode = settings.PlantFormationCode;
        PlantFormationClearField = settings.PlantFormationClearField;
        PlantFormationUseAdvancedFormat = settings.PlantFormationUseAdvancedFormat;
        ZombieFormationCode = settings.ZombieFormationCode;
        ZombieFormationClearField = settings.ZombieFormationClearField;
        ZombieFormationUseAdvancedFormat = settings.ZombieFormationUseAdvancedFormat;
        MixedFormationCode = settings.MixedFormationCode;
        MixedFormationClearField = settings.MixedFormationClearField;
        PotFormationCode = settings.PotFormationCode;
        PotFormationClearField = settings.PotFormationClearField;

        // 斗蛐蛐布阵
        GiftBox1PlantIndex = settings.GiftBox1PlantIndex;
        GiftBox2PlantIndex = settings.GiftBox2PlantIndex;
        GiftBox3PlantIndex = settings.GiftBox3PlantIndex;
        GiftBox4PlantIndex = settings.GiftBox4PlantIndex;
        GiftBox5PlantIndex = settings.GiftBox5PlantIndex;
        ZombieSlot1Index = settings.ZombieSlot1Index;
        ZombieSlot2Index = settings.ZombieSlot2Index;
        ZombieSlot3Index = settings.ZombieSlot3Index;
        ZombieSlot4Index = settings.ZombieSlot4Index;
        ZombieSlot5Index = settings.ZombieSlot5Index;
        ZombieSlot6Index = settings.ZombieSlot6Index;
        KillUpgrade = settings.KillUpgrade;
        RandomUpgradeMode = settings.RandomUpgradeMode;
    }
}