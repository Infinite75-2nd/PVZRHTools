using ToolData;
using PVZRHTools.Services;
using PVZRHTools.Utils;
using ReactiveUI.SourceGenerators;

namespace PVZRHTools.ViewModels;

public partial class PropertySettingsViewModel : ModifierPageViewModelBase
{
    public IInitDataService InitDataService { get; }

    #region 礼盒修改

    [Reactive] public partial bool SuperPresent { get; set; }
    [Reactive] public partial bool UltimateRandomZombie { get; set; }
    [Reactive] public partial bool PresentFastOpen { get; set; }
    [Reactive] public partial int LockPresent { get; set; }
    [Reactive] public partial bool LockPresentEnabled { get; set; }

    #endregion

    #region 数值修改

    [Reactive] public partial int ZombieHPType { get; set; }
    [Reactive] public partial int ZombieHPValue { get; set; }
    [Reactive] public partial int FirstArmorHPType { get; set; }
    [Reactive] public partial int FirstArmorHPValue { get; set; }
    [Reactive] public partial int SecondArmorHPType { get; set; }
    [Reactive] public partial int SecondArmorHPValue { get; set; }
    [Reactive] public partial int BulletDamageType { get; set; }
    [Reactive] public partial int BulletDamageValue { get; set; }
    [Reactive] public partial int LockBulletType { get; set; }

    #endregion

    #region 场地特性

    [Reactive] public partial bool NoIceRoad { get; set; }
    [Reactive] public partial bool NoHole { get; set; }
    [Reactive] public partial bool ItemExistForever { get; set; }
    [Reactive] public partial bool JackboxNotExplode { get; set; }
    [Reactive] public partial bool GarlicDay { get; set; }
    [Reactive] public partial bool UnlimitedSunlight { get; set; }
    [Reactive] public partial bool UnlockRedCardPlants { get; set; }
    [Reactive] public partial bool PotSmashingFix { get; set; }
    [Reactive] public partial bool DisableIceEffect { get; set; }

    #endregion

    #region 植物特性

    [Reactive] public partial bool FastShooting { get; set; }
    [Reactive] public partial bool HardPlant { get; set; }
    [Reactive] public partial bool ImmuneForceDeduct { get; set; }
    [Reactive] public partial bool CurseImmunity { get; set; }
    [Reactive] public partial bool CrushImmunity { get; set; }
    [Reactive] public partial bool TrampleImmunity { get; set; }
    [Reactive] public partial bool PickaxeImmunity { get; set; }
    [Reactive] public partial bool UndeadBullet { get; set; }
    [Reactive] public partial bool OldObsidianBullet { get; set; }
    [Reactive] public partial bool UltimateSuperGatling { get; set; }
    [Reactive] public partial bool HyponoEmperorNoCD { get; set; }
    [Reactive] public partial bool MagnetNutUnlimited { get; set; }
    [Reactive] public partial bool MineNoCD { get; set; }
    [Reactive] public partial bool ChomperNoCD { get; set; }
    [Reactive] public partial bool CobCannonNoCD { get; set; }
    [Reactive] public partial bool PlantUpgrade { get; set; }
    [Reactive] public partial bool SuperStarNoCD { get; set; }
    [Reactive] public partial bool LockWheatEnabled { get; set; }
    [Reactive] public partial int LockWheat { get; set; }

    #endregion

    #region 僵尸特性

    [Reactive] public partial bool ZombieDamageLimitEnabled { get; set; } = false;
    [Reactive] public partial int ZombieDamageLimit { get; set; } = 100;
    [Reactive] public partial bool ZombieSpeedMultiplierEnabled { get; set; }
    [Reactive] public partial double ZombieSpeedMultiplier { get; set; } = 1.0;
    [Reactive] public partial bool ZombieAttackMultiplierEnabled { get; set; }
    [Reactive] public partial double ZombieAttackMultiplier { get; set; } = 1.0;
    [Reactive] public partial bool ZombieBulletReflectEnabled { get; set; }
    [Reactive] public partial int ZombieBulletReflect { get; set; } = 10;
    [Reactive] public partial bool ZombieStatusCoexist { get; set; }
    [Reactive] public partial bool ZombieImmuneAllDebuffs { get; set; }
    [Reactive] public partial bool ZombieImmuneFreeze { get; set; }
    [Reactive] public partial bool ZombieImmuneCold { get; set; }
    [Reactive] public partial bool ZombieImmuneButter { get; set; }
    [Reactive] public partial bool ZombieImmunePoison { get; set; }
    [Reactive] public partial bool ZombieImmuneJalaed { get; set; }
    [Reactive] public partial bool ZombieImmuneEmbered { get; set; }
    [Reactive] public partial bool ZombieImmuneKnockback { get; set; }
    [Reactive] public partial bool ZombieImmuneMindControl { get; set; }
    [Reactive] public partial bool ZombieImmuneDevour { get; set; }

    #endregion

    #region 其他特性

    [Reactive] public partial bool AutoCutFruit { get; set; }
    [Reactive] public partial bool RandomCard { get; set; }
    [Reactive] public partial bool ColumnGlove { get; set; }
    [Reactive] public partial bool UnlimitedCardSlots { get; set; }
    [Reactive] public partial bool RandomBullet { get; set; }
    [Reactive] public partial bool AutoRhythmGame { get; set; }
    [Reactive] public partial bool StarUpBuff { get; set; }

    #endregion

    #region Commands

    [ReactiveCommand]
    public void SetZombieHP() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.SetZombieHP,
            Parameters = [ZombieHPType.ToString(), ZombieHPValue.ToString()]
        });

    [ReactiveCommand]
    public void SetFirstArmorHP() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.SetFirstArmorHP,
            Parameters = [FirstArmorHPType.ToString(), FirstArmorHPValue.ToString()]
        });

    [ReactiveCommand]
    public void SetSecondArmorHP() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.SetSecondArmorHP,
            Parameters = [SecondArmorHPType.ToString(), SecondArmorHPValue.ToString()]
        });

    [ReactiveCommand]
    public void SetBulletDamage() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.SetBulletDamage,
            Parameters = [BulletDamageType.ToString(), BulletDamageValue.ToString()]
        });

    [ReactiveCommand]
    public void SetLockBullet() =>
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.LockBullet,
            Parameters = [LockBulletType.ToString()]
        });

    #endregion


    public PropertySettingsViewModel(IDataSyncService dataSyncService, IInitDataService initDataService) : base(
        dataSyncService)
    {
        InitDataService = initDataService;
        // 礼盒修改
        this.SimpleOneWaySync(x => x.SuperPresent, Strings.SuperPresent);
        this.SimpleOneWaySync(x => x.UltimateRandomZombie, Strings.UltimateRandomZombie);
        this.SimpleOneWaySync(x => x.PresentFastOpen, Strings.PresentFastOpen);
        this.SimpleSyncFlaggedInt(x => x.LockPresent, x => x.LockPresentEnabled, Strings.LockPresent);
        // 场地特性
        this.SimpleOneWaySync(x => x.NoIceRoad, Strings.NoIceRoad);
        this.SimpleOneWaySync(x => x.NoHole, Strings.NoHole);
        this.SimpleOneWaySync(x => x.ItemExistForever, Strings.ItemExistForever);
        this.SimpleOneWaySync(x => x.JackboxNotExplode, Strings.JackboxNotExplode);
        this.SimpleOneWaySync(x => x.GarlicDay, Strings.GarlicDay);
        this.SimpleOneWaySync(x => x.UnlimitedSunlight, Strings.UnlimitedSunlight);
        this.SimpleOneWaySync(x => x.UnlockRedCardPlants, Strings.UnlockRedCardPlants);
        this.SimpleOneWaySync(x => x.PotSmashingFix, Strings.PotSmashingFix);
        this.SimpleOneWaySync(x => x.DisableIceEffect, Strings.DisableIceEffect);

        // 植物特性
        this.SimpleOneWaySync(x => x.FastShooting, Strings.FastShooting);
        this.SimpleOneWaySync(x => x.HardPlant, Strings.HardPlant);
        this.SimpleOneWaySync(x => x.ImmuneForceDeduct, Strings.ImmuneForceDeduct);
        this.SimpleOneWaySync(x => x.CurseImmunity, Strings.CurseImmunity);
        this.SimpleOneWaySync(x => x.CrushImmunity, Strings.CrushImmunity);
        this.SimpleOneWaySync(x => x.TrampleImmunity, Strings.TrampleImmunity);
        this.SimpleOneWaySync(x => x.PickaxeImmunity, Strings.PickaxeImmunity);
        this.SimpleOneWaySync(x => x.UndeadBullet, Strings.UndeadBullet);
        this.SimpleOneWaySync(x => x.OldObsidianBullet, Strings.OldObsidianBullet);
        this.SimpleOneWaySync(x => x.UltimateSuperGatling, Strings.UltimateSuperGatling);
        this.SimpleOneWaySync(x => x.HyponoEmperorNoCD, Strings.HyponoEmperorNoCD);
        this.SimpleOneWaySync(x => x.MagnetNutUnlimited, Strings.MagnetNutUnlimited);
        this.SimpleOneWaySync(x => x.MineNoCD, Strings.MineNoCD);
        this.SimpleOneWaySync(x => x.ChomperNoCD, Strings.ChomperNoCD);
        this.SimpleOneWaySync(x => x.CobCannonNoCD, Strings.CobCannonNoCD);
        this.SimpleOneWaySync(x => x.PlantUpgrade, Strings.PlantUpgrade);
        this.SimpleOneWaySync(x => x.SuperStarNoCD, Strings.SuperStarNoCD);
        this.SimpleSyncFlaggedInt(x => x.LockWheat, x => x.LockWheatEnabled, Strings.LockWheat);

        // 僵尸特性
        this.SimpleSyncFlaggedInt(x => x.ZombieDamageLimit, x => x.ZombieDamageLimitEnabled, Strings.ZombieDamageLimit);
        this.SimpleSyncFlaggedDouble(x => x.ZombieSpeedMultiplier, x => x.ZombieSpeedMultiplierEnabled,
            Strings.ZombieSpeedMultiplier);
        this.SimpleSyncFlaggedDouble(x => x.ZombieAttackMultiplier, x => x.ZombieAttackMultiplierEnabled,
            Strings.ZombieAttackMultiplier);
        this.SimpleSyncFlaggedInt(x => x.ZombieBulletReflect, x => x.ZombieBulletReflectEnabled,
            Strings.ZombieBulletReflect);
        this.SimpleOneWaySync(x => x.ZombieStatusCoexist, Strings.ZombieStatusCoexist);
        this.SimpleOneWaySync(x => x.ZombieImmuneAllDebuffs, Strings.ZombieImmuneAllDebuffs);
        this.SimpleOneWaySync(x => x.ZombieImmuneFreeze, Strings.ZombieImmuneFreeze);
        this.SimpleOneWaySync(x => x.ZombieImmuneCold, Strings.ZombieImmuneCold);
        this.SimpleOneWaySync(x => x.ZombieImmuneButter, Strings.ZombieImmuneButter);
        this.SimpleOneWaySync(x => x.ZombieImmunePoison, Strings.ZombieImmunePoison);
        this.SimpleOneWaySync(x => x.ZombieImmuneJalaed, Strings.ZombieImmuneJalaed);
        this.SimpleOneWaySync(x => x.ZombieImmuneEmbered, Strings.ZombieImmuneEmbered);
        this.SimpleOneWaySync(x => x.ZombieImmuneKnockback, Strings.ZombieImmuneKnockback);
        this.SimpleOneWaySync(x => x.ZombieImmuneMindControl, Strings.ZombieImmuneMindControl);
        this.SimpleOneWaySync(x => x.ZombieImmuneDevour, Strings.ZombieImmuneDevour);

        // 其他特性
        this.SimpleOneWaySync(x => x.AutoCutFruit, Strings.AutoCutFruit);
        this.SimpleOneWaySync(x => x.RandomCard, Strings.RandomCard);
        this.SimpleOneWaySync(x => x.ColumnGlove, Strings.ColumnGlove);
        this.SimpleOneWaySync(x => x.UnlimitedCardSlots, Strings.UnlimitedCardSlots);
        this.SimpleOneWaySync(x => x.RandomBullet, Strings.RandomBullet);
        this.SimpleOneWaySync(x => x.AutoRhythmGame, Strings.AutoRhythmGame);
        this.SimpleOneWaySync(x => x.StarUpBuff, Strings.StarUpBuff);
    }

    public override void SaveSettings(SettingsData settings)
    {
        // 礼盒修改
        settings.SuperPresent = SuperPresent;
        settings.UltimateRandomZombie = UltimateRandomZombie;
        settings.PresentFastOpen = PresentFastOpen;
        settings.LockPresent = LockPresent;
        settings.LockPresentEnabled = LockPresentEnabled;

        // 场地特性
        settings.NoIceRoad = NoIceRoad;
        settings.NoHole = NoHole;
        settings.ItemExistForever = ItemExistForever;
        settings.JackboxNotExplode = JackboxNotExplode;
        settings.GarlicDay = GarlicDay;
        settings.UnlimitedSunlight = UnlimitedSunlight;
        settings.UnlockRedCardPlants = UnlockRedCardPlants;
        settings.PotSmashingFix = PotSmashingFix;
        settings.DisableIceEffect = DisableIceEffect;

        // 植物特性
        settings.FastShooting = FastShooting;
        settings.HardPlant = HardPlant;
        settings.ImmuneForceDeduct = ImmuneForceDeduct;
        settings.CurseImmunity = CurseImmunity;
        settings.CrushImmunity = CrushImmunity;
        settings.TrampleImmunity = TrampleImmunity;
        settings.PickaxeImmunity = PickaxeImmunity;
        settings.UndeadBullet = UndeadBullet;
        settings.OldObsidianBullet = OldObsidianBullet;
        settings.UltimateSuperGatling = UltimateSuperGatling;
        settings.HyponoEmperorNoCD = HyponoEmperorNoCD;
        settings.MagnetNutUnlimited = MagnetNutUnlimited;
        settings.MineNoCD = MineNoCD;
        settings.ChomperNoCD = ChomperNoCD;
        settings.CobCannonNoCD = CobCannonNoCD;
        settings.PlantUpgrade = PlantUpgrade;
        settings.SuperStarNoCD = SuperStarNoCD;
        settings.LockWheat = LockWheat;
        settings.LockWheatEnabled = LockWheatEnabled;

        // 僵尸特性
        settings.ZombieDamageLimitEnabled = ZombieDamageLimitEnabled;
        settings.ZombieDamageLimit = ZombieDamageLimit;
        settings.ZombieSpeedMultiplierEnabled = ZombieSpeedMultiplierEnabled;
        settings.ZombieSpeedMultiplier = ZombieSpeedMultiplier;
        settings.ZombieAttackMultiplierEnabled = ZombieAttackMultiplierEnabled;
        settings.ZombieAttackMultiplier = ZombieAttackMultiplier;
        settings.ZombieBulletReflectEnabled = ZombieBulletReflectEnabled;
        settings.ZombieBulletReflect = ZombieBulletReflect;
        settings.ZombieStatusCoexist = ZombieStatusCoexist;
        settings.ZombieImmuneAllDebuffs = ZombieImmuneAllDebuffs;
        settings.ZombieImmuneFreeze = ZombieImmuneFreeze;
        settings.ZombieImmuneCold = ZombieImmuneCold;
        settings.ZombieImmuneButter = ZombieImmuneButter;
        settings.ZombieImmunePoison = ZombieImmunePoison;
        settings.ZombieImmuneJalaed = ZombieImmuneJalaed;
        settings.ZombieImmuneEmbered = ZombieImmuneEmbered;
        settings.ZombieImmuneKnockback = ZombieImmuneKnockback;
        settings.ZombieImmuneMindControl = ZombieImmuneMindControl;
        settings.ZombieImmuneDevour = ZombieImmuneDevour;

        // 其他特性
        settings.AutoCutFruit = AutoCutFruit;
        settings.RandomCard = RandomCard;
        settings.ColumnGlove = ColumnGlove;
        settings.UnlimitedCardSlots = UnlimitedCardSlots;
        settings.RandomBullet = RandomBullet;
        settings.AutoRhythmGame = AutoRhythmGame;
        settings.StarUpBuff = StarUpBuff;
    }

    public override void LoadSettings(SettingsData settings)
    {
        // 礼盒修改
        SuperPresent = settings.SuperPresent;
        UltimateRandomZombie = settings.UltimateRandomZombie;
        PresentFastOpen = settings.PresentFastOpen;
        LockPresent = settings.LockPresent;
        LockPresentEnabled = settings.LockPresentEnabled;

        // 场地特性
        NoIceRoad = settings.NoIceRoad;
        NoHole = settings.NoHole;
        ItemExistForever = settings.ItemExistForever;
        JackboxNotExplode = settings.JackboxNotExplode;
        GarlicDay = settings.GarlicDay;
        UnlimitedSunlight = settings.UnlimitedSunlight;
        UnlockRedCardPlants = settings.UnlockRedCardPlants;
        PotSmashingFix = settings.PotSmashingFix;
        DisableIceEffect = settings.DisableIceEffect;

        // 植物特性
        FastShooting = settings.FastShooting;
        HardPlant = settings.HardPlant;
        ImmuneForceDeduct = settings.ImmuneForceDeduct;
        CurseImmunity = settings.CurseImmunity;
        CrushImmunity = settings.CrushImmunity;
        TrampleImmunity = settings.TrampleImmunity;
        PickaxeImmunity = settings.PickaxeImmunity;
        UndeadBullet = settings.UndeadBullet;
        OldObsidianBullet = settings.OldObsidianBullet;
        UltimateSuperGatling = settings.UltimateSuperGatling;
        HyponoEmperorNoCD = settings.HyponoEmperorNoCD;
        MagnetNutUnlimited = settings.MagnetNutUnlimited;
        MineNoCD = settings.MineNoCD;
        ChomperNoCD = settings.ChomperNoCD;
        CobCannonNoCD = settings.CobCannonNoCD;
        PlantUpgrade = settings.PlantUpgrade;
        SuperStarNoCD = settings.SuperStarNoCD;
        LockWheat = settings.LockWheat;
        LockWheatEnabled = settings.LockWheatEnabled;

        // 僵尸特性
        ZombieDamageLimitEnabled = settings.ZombieDamageLimitEnabled;
        ZombieDamageLimit = settings.ZombieDamageLimit;
        ZombieSpeedMultiplierEnabled = settings.ZombieSpeedMultiplierEnabled;
        ZombieSpeedMultiplier = settings.ZombieSpeedMultiplier;
        ZombieAttackMultiplierEnabled = settings.ZombieAttackMultiplierEnabled;
        ZombieAttackMultiplier = settings.ZombieAttackMultiplier;
        ZombieBulletReflectEnabled = settings.ZombieBulletReflectEnabled;
        ZombieBulletReflect = settings.ZombieBulletReflect;
        ZombieStatusCoexist = settings.ZombieStatusCoexist;
        ZombieImmuneAllDebuffs = settings.ZombieImmuneAllDebuffs;
        ZombieImmuneFreeze = settings.ZombieImmuneFreeze;
        ZombieImmuneCold = settings.ZombieImmuneCold;
        ZombieImmuneButter = settings.ZombieImmuneButter;
        ZombieImmunePoison = settings.ZombieImmunePoison;
        ZombieImmuneJalaed = settings.ZombieImmuneJalaed;
        ZombieImmuneEmbered = settings.ZombieImmuneEmbered;
        ZombieImmuneKnockback = settings.ZombieImmuneKnockback;
        ZombieImmuneMindControl = settings.ZombieImmuneMindControl;
        ZombieImmuneDevour = settings.ZombieImmuneDevour;

        // 其他特性
        AutoCutFruit = settings.AutoCutFruit;
        RandomCard = settings.RandomCard;
        ColumnGlove = settings.ColumnGlove;
        UnlimitedCardSlots = settings.UnlimitedCardSlots;
        RandomBullet = settings.RandomBullet;
        AutoRhythmGame = settings.AutoRhythmGame;
        StarUpBuff = settings.StarUpBuff;
    }
}