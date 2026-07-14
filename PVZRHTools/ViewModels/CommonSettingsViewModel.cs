using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using ToolData;
using PVZRHTools.Services;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using PVZRHTools.Utils;

namespace PVZRHTools.ViewModels;

public partial class CommonSettingsViewModel : ModifierPageViewModelBase
{
    public IInitDataService InitDataService { get; }

    #region 全局属性修改

    [Reactive] public partial bool DevMode { get; set; }
    [Reactive] public partial bool ColumnPlanting { get; set; }
    [Reactive] public partial bool SeedRain { get; set; }
    [Reactive] public partial bool GameSpeedEnabled { get; set; }
    [Reactive] public partial double GameSpeed { get; set; } = 1.0;
    [Reactive] public partial bool GloveNoCD { get; set; }
    [Reactive] public partial bool HammerNoCD { get; set; }
    [Reactive] public partial double HammerFullCD { get; set; } = 60;
    [Reactive] public partial bool HammerFullCDEnabled { get; set; }
    [Reactive] public partial double GloveFullCD { get; set; } = 10;
    [Reactive] public partial bool GloveFullCDEnabled { get; set; }
    [Reactive] public partial bool WheelNoCD { get; set; }
    [Reactive] public partial double WheelFullCD { get; set; } = 30;
    [Reactive] public partial bool WheelFullCDEnabled { get; set; }
    [Reactive] public partial bool FreePlanting { get; set; }
    [Reactive] public partial bool CardFreeCD { get; set; }
    [Reactive] public partial bool RemoveFusionLimit { get; set; }
    [Reactive] public partial double NewZombieUpdateCD { get; set; } = 30;
    [Reactive] public partial bool NewZombieUpdateCDEnabled { get; set; }
    [Reactive] public partial bool UnlimitedScore { get; set; }
    [Reactive] public partial bool UnlimitedRefresh { get; set; }

    #endregion

    #region 游戏内属性调整

    [Reactive] public partial int Sun { get; set; }
    [Reactive] public partial bool LockSun { get; set; }
    [Reactive] public partial int Money { get; set; }
    [Reactive] public partial bool LockMoney { get; set; }
    [Reactive] public partial bool PauseSpawn { get; set; }
    [Reactive] public partial bool NoFail { get; set; }
    [Reactive] public partial string LevelName { get; set; } = "";
    [Reactive] public partial uint JumpWave { get; set; }

    #endregion

    #region 游戏内生成操作

    [Reactive] public partial int Row { get; set; } = 0;
    [Reactive] public partial int Column { get; set; } = 0;
    [Reactive] public partial int RepeatTimes { get; set; } = 1;
    [Reactive] public partial bool PvPPotRange { get; set; }
    [Reactive] public partial int CreatePlantID { get; set; }
    [Reactive] public partial bool CreateRandomPlant { get; set; }
    [Reactive] public partial int CreateZombieID { get; set; }
    [Reactive] public partial bool CreateRandomZombie { get; set; }
    [Reactive] public partial int CreateItemID { get; set; }
    [Reactive] public partial string Text { get; set; } = "";

    #endregion

    #region 僵尸海设置

    [Reactive] public partial bool ZombieSeaEnabled { get; set; }
    [Reactive] public partial int ZombieSeaCD { get; set; } = 40;
    [Reactive] public partial bool ZombieSeaLowEnabled { get; set; }
    [Reactive] public partial ObservableCollection<KeyValuePair<int, string>> ZombieSeaTypes { get; set; } = [];

    #endregion

    #region Commands

    [ReactiveCommand]
    private void SetSun() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.Sun,
            Parameters = [Sun.ToString()]
        });

    [ReactiveCommand]
    public void SetMoney() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.Money,
            Parameters = [Money.ToString()]
        });

    [ReactiveCommand]
    public void RemoveAllPlants() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.RemoveAllPlants,
            Parameters = []
        });

    [ReactiveCommand]
    public void RemoveAllZombies() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.RemoveAllZombies,
            Parameters = []
        });

    [ReactiveCommand]
    public void MindCtrlAllZombies() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.MindCtrlAllZombies,
            Parameters = []
        });

    [ReactiveCommand]
    public void RemoveAllIceRoads() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.RemoveAllIceRoads,
            Parameters = []
        });

    [ReactiveCommand]
    public void RemoveAllHoles() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.RemoveAllHoles,
            Parameters = []
        });

    [ReactiveCommand]
    public void RemoveAllGraves() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.RemoveAllGraves,
            Parameters = []
        });

    [ReactiveCommand]
    public void SetLevelName() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.SetLevelName,
            Parameters = [LevelName]
        });

    [ReactiveCommand]
    public void SetZombiesIdle() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.SetZombiesIdle,
            Parameters = []
        });

    [ReactiveCommand]
    public void ApplyAllPlantSkins() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.ApplyAllPlantSkins,
            Parameters = []
        });

    [ReactiveCommand]
    public void CreatePlant() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.CreatePlant,
            Parameters =
            [
                CreateRandomPlant ? "-1" : CreatePlantID.ToString(), Row.ToString(), Column.ToString(),
                RepeatTimes.ToString()
            ]
        });

    [ReactiveCommand]
    public void CreatePlantCard() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.CreatePlantCard,
            Parameters = [CreateRandomPlant ? "-1" : CreatePlantID.ToString(), RepeatTimes.ToString()]
        });

    [ReactiveCommand]
    public void CreatePlantVase() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.CreatePlantVase,
            Parameters =
            [
                CreateRandomPlant ? "-1" : CreatePlantID.ToString(), Row.ToString(), Column.ToString(),
                PvPPotRange.ToString()
            ]
        });

    [ReactiveCommand]
    public void CreateZombie() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.CreateZombie,
            Parameters =
            [
                CreateRandomZombie ? "-1" : CreateZombieID.ToString(), Row.ToString(), Column.ToString(),
                RepeatTimes.ToString()
            ]
        });

    [ReactiveCommand]
    public void CreateMindControlledZombie() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.CreateMindControlledZombie,
            Parameters =
            [
                CreateRandomZombie ? "-1" : CreateZombieID.ToString(), Row.ToString(), Column.ToString(),
                RepeatTimes.ToString()
            ]
        });

    [ReactiveCommand]
    public void CreateZombieVase() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.CreateZombieVase,
            Parameters =
            [
                CreateRandomZombie ? "-1" : CreateZombieID.ToString(), Row.ToString(), Column.ToString(),
                PvPPotRange.ToString()
            ]
        });

    [ReactiveCommand]
    public void CreateItem() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.CreateItem,
            Parameters = [CreateItemID.ToString()]
        });

    [ReactiveCommand]
    public void CreatePassiveMeteorite() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.CreatePassiveMeteorite,
            Parameters = []
        });

    [ReactiveCommand]
    public void CreateActiveMeteorite() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.CreateActiveMeteorite,
            Parameters = []
        });

    [ReactiveCommand]
    public void CreateUltimateMeteorite() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.CreateUltimateMeteorite,
            Parameters = []
        });

    [ReactiveCommand]
    public void CreateSolarMeteorite() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.CreateSolarMeteorite,
            Parameters = []
        });

    [ReactiveCommand]
    public void NextWave() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.NextWave,
            Parameters = []
        });

    [ReactiveCommand]
    public void SetJumpWave() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.SetJumpWave,
            Parameters = [JumpWave.ToString()]
        });

    [ReactiveCommand]
    public void SetAward() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.SetAward,
            Parameters = []
        });

    [ReactiveCommand]
    public void DestroyAward() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.DestroyAward,
            Parameters = []
        });

    [ReactiveCommand]
    public void ShowText() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.ShowText,
            Parameters = [Text]
        });

    [ReactiveCommand]
    public void StartMower() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.StartMower,
            Parameters = []
        });

    [ReactiveCommand]
    public void CreateMower() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.CreateMower,
            Parameters = []
        });

    [ReactiveCommand]
    public void SpawnPetGargantuar() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.SpawnPetGargantuar,
            Parameters = []
        });

    [ReactiveCommand]
    public void SpawnPetFootball() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.SpawnPetFootball,
            Parameters = []
        });

    [ReactiveCommand]
    public void SpawnPetSnowBoss() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.SpawnPetSnowBoss,
            Parameters = []
        });

    [ReactiveCommand]
    public void SpawnPetJackbox() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.SpawnPetJackbox,
            Parameters = []
        });

    [ReactiveCommand]
    public void SpawnPetDrown() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.SpawnPetDrown,
            Parameters = []
        });

    [ReactiveCommand]
    public void SpawnPetHorse() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.SpawnPetHorse,
            Parameters = []
        }); //todo:名字？？？

    [ReactiveCommand]
    public void SpawnPetImp() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.SpawnPetImp,
            Parameters = []
        });

    [ReactiveCommand]
    public void SpawnPetKirov() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.SpawnPetKirov,
            Parameters = []
        });

    #endregion

    public CommonSettingsViewModel(IDataSyncService dataSyncService, IInitDataService initDataService) : base(
        dataSyncService)
    {
        InitDataService = initDataService;

        //全局属性修改
        this.SimpleOneWaySync(x => x.DevMode, Strings.DevMode);
        this.SimpleOneWaySync(x => x.ColumnPlanting, Strings.ColumnPlanting);
        this.SimpleOneWaySync(x => x.SeedRain, Strings.SeedRain);
        this.SimpleOneWaySync(x => x.GameSpeedEnabled, Strings.GameSpeedEnabled);
        this.SimpleOneWaySync(x => x.GameSpeed, Strings.GameSpeed);
        this.SimpleOneWaySync(x => x.GloveNoCD, Strings.GloveNoCD);
        this.SimpleOneWaySync(x => x.HammerNoCD, Strings.HammerNoCD);
        this.SimpleSyncFlaggedDouble(x => x.HammerFullCD, x => x.HammerFullCDEnabled, Strings.HammerFullCD);
        this.SimpleSyncFlaggedDouble(x => x.GloveFullCD, x => x.GloveFullCDEnabled, Strings.GloveFullCD);
        this.SimpleOneWaySync(x => x.WheelNoCD, Strings.WheelNoCD);
        this.SimpleSyncFlaggedDouble(x => x.WheelFullCD, x => x.WheelFullCDEnabled, Strings.WheelFullCD);
        this.SimpleOneWaySync(x => x.FreePlanting, Strings.FreePlanting);
        this.SimpleOneWaySync(x => x.CardFreeCD, Strings.CardFreeCD);
        this.SimpleOneWaySync(x => x.RemoveFusionLimit, Strings.RemoveFusionLimit);
        this.SimpleSyncFlaggedDouble(x => x.NewZombieUpdateCD, x => x.NewZombieUpdateCDEnabled,
            Strings.NewZombieUpdateCD);
        this.SimpleOneWaySync(x => x.UnlimitedScore, Strings.UnlimitedScore);
        this.SimpleOneWaySync(x => x.UnlimitedRefresh, Strings.UnlimitedRefresh);

        //游戏内属性修改
        this.SimpleSyncFlaggedInt(x => x.Sun, x => x.LockSun, Strings.LockSun, true);
        this.SimpleSyncFlaggedInt(x => x.Money, x => x.LockMoney, Strings.LockMoney, true);
        this.SimpleOneWaySync(x => x.PauseSpawn, Strings.PauseSpawn);
        this.SimpleOneWaySync(x => x.NoFail, Strings.NoFail);

        //僵尸海设置
        this.WhenAnyValue(x => x.ZombieSeaEnabled,
                x => x.ZombieSeaCD,
                x => x.ZombieSeaLowEnabled,
                x => x.ZombieSeaTypes)
            .Subscribe(tuple =>
            {
                dataSyncService.SendCommand(new SyncData()
                {
                    Command = Strings.ZombieSea,
                    Parameters =
                    [
                        JsonSerializer.Serialize(new ZombieSea()
                        {
                            ZombieSeaEnabled = tuple.Item1,
                            ZombieSeaCD = tuple.Item2,
                            ZombieSeaLowEnabled = tuple.Item3,
                            ZombieSeaTypes = tuple.Item4.Select(kvp => kvp.Key).ToList()
                        }, JsonSGC.Default.ZombieSea)
                    ]
                });
            });
    }

    public override void SaveSettings(SettingsData settings)
    {
        //全局属性修改
        settings.DevMode = DevMode;
        settings.ColumnPlanting = ColumnPlanting;
        settings.SeedRain = SeedRain;
        settings.GameSpeedEnabled = GameSpeedEnabled;
        settings.GameSpeed = GameSpeed;
        settings.GloveNoCD = GloveNoCD;
        settings.HammerNoCD = HammerNoCD;
        settings.HammerFullCD = HammerFullCD;
        settings.HammerFullCDEnabled = HammerFullCDEnabled;
        settings.GloveFullCD = GloveFullCD;
        settings.GloveFullCDEnabled = GloveFullCDEnabled;
        settings.WheelNoCD = WheelNoCD;
        settings.WheelFullCD = WheelFullCD;
        settings.WheelFullCDEnabled = WheelFullCDEnabled;
        settings.FreePlanting = FreePlanting;
        settings.CardFreeCD = CardFreeCD;
        settings.RemoveFusionLimit = RemoveFusionLimit;
        settings.NewZombieUpdateCD = NewZombieUpdateCD;
        settings.NewZombieUpdateCDEnabled = NewZombieUpdateCDEnabled;
        settings.UnlimitedScore = UnlimitedScore;
        settings.UnlimitedRefresh = UnlimitedRefresh;

        //游戏内属性修改
        settings.Sun = Sun;
        settings.LockSun = LockSun;
        settings.Money = Money;
        settings.LockMoney = LockMoney;
        settings.PauseSpawn = PauseSpawn;
        settings.NoFail = NoFail;

        //游戏内生成操作
        settings.Row = Row;
        settings.Column = Column;
        settings.RepeatTimes = RepeatTimes;
        settings.PvPPotRange = PvPPotRange;
        settings.CreatePlantID = CreatePlantID;
        settings.CreateRandomPlant = CreateRandomPlant;
        settings.CreateZombieID = CreateZombieID;
        settings.CreateRandomZombie = CreateRandomZombie;
        settings.CreateItemID = CreateItemID;
        settings.Text = Text;

        //僵尸海设置
        settings.ZombieSeaEnabled = ZombieSeaEnabled;
        settings.ZombieSeaCD = ZombieSeaCD;
        settings.ZombieSeaLowEnabled = ZombieSeaLowEnabled;
        settings.ZombieSeaTypes = ZombieSeaTypes.Select(kvp => kvp.Key).ToList();
    }

    public override void LoadSettings(SettingsData settings)
    {
        //全局属性修改
        DevMode = settings.DevMode;
        ColumnPlanting = settings.ColumnPlanting;
        SeedRain = settings.SeedRain;
        GameSpeedEnabled = settings.GameSpeedEnabled;
        GameSpeed = settings.GameSpeed;
        GloveNoCD = settings.GloveNoCD;
        HammerNoCD = settings.HammerNoCD;
        HammerFullCD = settings.HammerFullCD;
        HammerFullCDEnabled = settings.HammerFullCDEnabled;
        GloveFullCD = settings.GloveFullCD;
        GloveFullCDEnabled = settings.GloveFullCDEnabled;
        WheelNoCD = settings.WheelNoCD;
        WheelFullCD = settings.WheelFullCD;
        WheelFullCDEnabled = settings.WheelFullCDEnabled;
        FreePlanting = settings.FreePlanting;
        CardFreeCD = settings.CardFreeCD;
        RemoveFusionLimit = settings.RemoveFusionLimit;
        NewZombieUpdateCD = settings.NewZombieUpdateCD;
        NewZombieUpdateCDEnabled = settings.NewZombieUpdateCDEnabled;
        UnlimitedScore = settings.UnlimitedScore;
        UnlimitedRefresh = settings.UnlimitedRefresh;

        //游戏内属性修改
        Sun = settings.Sun;
        LockSun = settings.LockSun;
        Money = settings.Money;
        LockMoney = settings.LockMoney;
        PauseSpawn = settings.PauseSpawn;
        NoFail = settings.NoFail;

        //游戏内生成操作
        Row = settings.Row;
        Column = settings.Column;
        RepeatTimes = settings.RepeatTimes;
        PvPPotRange = settings.PvPPotRange;
        CreatePlantID = settings.CreatePlantID;
        CreateRandomPlant = settings.CreateRandomPlant;
        CreateZombieID = settings.CreateZombieID;
        CreateRandomZombie = settings.CreateRandomZombie;
        CreateItemID = settings.CreateItemID;
        Text = settings.Text;

        //僵尸海设置
        ZombieSeaEnabled = settings.ZombieSeaEnabled;
        ZombieSeaCD = settings.ZombieSeaCD;
        ZombieSeaLowEnabled = settings.ZombieSeaLowEnabled;

        ZombieSeaTypes.Clear();
        foreach (var typeId in settings.ZombieSeaTypes)
        {
            var name = InitDataService.InitData.Zombies.FirstOrDefault(z => z.Key == typeId).Value ?? "";
            ZombieSeaTypes.Add(new KeyValuePair<int, string>(typeId, name));
        }
    }
}