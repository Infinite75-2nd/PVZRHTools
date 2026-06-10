using System.Collections.Generic;

namespace ToolData;

/// <summary>
/// 用于保存和加载所有ViewModel的设置数据（ToolMod端使用）
/// </summary>
public class SettingsData
{
    // MiscsViewModel
    public bool SaveAllSettings { get; set; }
    public bool ShowFloatingWindow { get; set; }

    // CommonSettingsViewModel - 全局属性修改
    public bool DevMode { get; set; }
    public bool ColumnPlanting { get; set; }
    public bool SeedRain { get; set; }
    public bool GameSpeedEnabled { get; set; }
    public double GameSpeed { get; set; } = 1.0;
    public bool GloveNoCD { get; set; }
    public bool HammerNoCD { get; set; }
    public double HammerFullCD { get; set; } = 60;
    public bool HammerFullCDEnabled { get; set; }
    public double GloveFullCD { get; set; } = 10;
    public bool GloveFullCDEnabled { get; set; }
    public bool WheelNoCD { get; set; }
    public double WheelFullCD { get; set; } = 30;
    public bool WheelFullCDEnabled { get; set; }
    public bool FreePlanting { get; set; }
    public bool CardFreeCD { get; set; }
    public bool RemoveFusionLimit { get; set; }
    public double NewZombieUpdateCD { get; set; } = 30;
    public bool NewZombieUpdateCDEnabled { get; set; }
    public bool UnlimitedScore { get; set; }
    public bool UnlimitedRefresh { get; set; }

    // CommonSettingsViewModel - 游戏内属性调整
    public int Sun { get; set; }
    public bool LockSun { get; set; }
    public int Money { get; set; }
    public bool LockMoney { get; set; }
    public bool PauseSpawn { get; set; }
    public bool NoFail { get; set; }
    public string LevelName { get; set; } = "";

    // CommonSettingsViewModel - 游戏内生成操作
    public int Row { get; set; } = 0;
    public int Column { get; set; } = 0;
    public int RepeatTimes { get; set; } = 1;
    public bool PvPPotRange { get; set; }
    public int CreatePlantID { get; set; }
    public bool CreateRandomPlant { get; set; }
    public int CreateZombieID { get; set; }
    public bool CreateRandomZombie { get; set; }
    public int CreateItemID { get; set; }
    public string Text { get; set; } = "";

    // CommonSettingsViewModel - 僵尸海设置
    public bool ZombieSeaEnabled { get; set; }
    public int ZombieSeaCD { get; set; } = 40;
    public bool ZombieSeaLowEnabled { get; set; }
    public List<int> ZombieSeaTypes { get; set; } = [];

    // PropertySettingsViewModel - 礼盒修改
    public bool SuperPresent { get; set; }
    public bool UltimateRandomZombie { get; set; }
    public bool PresentFastOpen { get; set; }
    public int LockPresent { get; set; }
    public bool LockPresentEnabled { get; set; }

    // PropertySettingsViewModel - 数值修改
    public int ZombieHPType { get; set; }
    public int ZombieHPValue { get; set; }
    public int FirstArmorHPType { get; set; }
    public int FirstArmorHPValue { get; set; }
    public int SecondArmorHPType { get; set; }
    public int SecondArmorHPValue { get; set; }
    public int BulletDamageType { get; set; }
    public int BulletDamageValue { get; set; }
    public int LockBulletType { get; set; }

    // PropertySettingsViewModel - 场地特性
    public bool NoIceRoad { get; set; }
    public bool NoHole { get; set; }
    public bool ItemExistForever { get; set; }
    public bool JackboxNotExplode { get; set; }
    public bool GarlicDay { get; set; }
    public bool UnlimitedSunlight { get; set; }
    public bool UnlockRedCardPlants { get; set; }
    public bool PotSmashingFix { get; set; }
    public bool DisableIceEffect { get; set; }

    // PropertySettingsViewModel - 植物特性
    public bool FastShooting { get; set; }
    public bool HardPlant { get; set; }
    public bool ImmuneForceDeduct { get; set; }
    public bool CurseImmunity { get; set; }
    public bool CrushImmunity { get; set; }
    public bool TrampleImmunity { get; set; }
    public bool PickaxeImmunity { get; set; }
    public bool UndeadBullet { get; set; }
    public bool OldObsidianBullet { get; set; }
    public bool UltimateSuperGatling { get; set; }
    public bool HyponoEmperorNoCD { get; set; }
    public bool MagnetNutUnlimited { get; set; }
    public bool MineNoCD { get; set; }
    public bool ChomperNoCD { get; set; }
    public bool CobCannonNoCD { get; set; }
    public bool PlantUpgrade { get; set; }
    public bool SuperStarNoCD { get; set; }
    public bool LockWheatEnabled { get; set; }
    public int LockWheat { get; set; }

    // PropertySettingsViewModel - 僵尸特性
    public bool ZombieDamageLimitEnabled { get; set; } = false;
    public int ZombieDamageLimit { get; set; } = 100;
    public bool ZombieSpeedMultiplierEnabled { get; set; }
    public double ZombieSpeedMultiplier { get; set; } = 1.0;
    public bool ZombieAttackMultiplierEnabled { get; set; }
    public double ZombieAttackMultiplier { get; set; } = 1.0;
    public bool ZombieBulletReflectEnabled { get; set; }
    public int ZombieBulletReflect { get; set; } = 10;
    public bool ZombieStatusCoexist { get; set; }
    public bool ZombieImmuneAllDebuffs { get; set; }
    public bool ZombieImmuneFreeze { get; set; }
    public bool ZombieImmuneCold { get; set; }
    public bool ZombieImmuneButter { get; set; }
    public bool ZombieImmunePoison { get; set; }
    public bool ZombieImmuneJalaed { get; set; }
    public bool ZombieImmuneEmbered { get; set; }
    public bool ZombieImmuneKnockback { get; set; }
    public bool ZombieImmuneMindControl { get; set; }
    public bool ZombieImmuneDevour { get; set; }

    // PropertySettingsViewModel - 其他特性
    public bool AutoCutFruit { get; set; }
    public bool RandomCard { get; set; }
    public bool ColumnGlove { get; set; }
    public bool UnlimitedCardSlots { get; set; }
    public bool RandomBullet { get; set; }
    public bool AutoRhythmGame { get; set; }
    public bool StarUpBuff { get; set; }

    // FieldReadWriteViewModel - 常规布阵
    public string PlantFormationCode { get; set; } = string.Empty;
    public bool PlantFormationClearField { get; set; }
    public bool PlantFormationUseAdvancedFormat { get; set; }
    public string ZombieFormationCode { get; set; } = string.Empty;
    public bool ZombieFormationClearField { get; set; }
    public bool ZombieFormationUseAdvancedFormat { get; set; }
    public string MixedFormationCode { get; set; } = string.Empty;
    public bool MixedFormationClearField { get; set; }
    public string PotFormationCode { get; set; } = string.Empty;
    public bool PotFormationClearField { get; set; }

    // FieldReadWriteViewModel - 斗蛐蛐布阵
    public int GiftBox1PlantIndex { get; set; }
    public int GiftBox2PlantIndex { get; set; }
    public int GiftBox3PlantIndex { get; set; }
    public int GiftBox4PlantIndex { get; set; }
    public int GiftBox5PlantIndex { get; set; }
    public int ZombieSlot1Index { get; set; }
    public int ZombieSlot2Index { get; set; }
    public int ZombieSlot3Index { get; set; }
    public int ZombieSlot4Index { get; set; }
    public int ZombieSlot5Index { get; set; }
    public int ZombieSlot6Index { get; set; }
    public bool KillUpgrade { get; set; }
    public bool RandomUpgradeMode { get; set; }

    // TravelBuffViewModel - 初始词条设置
    public Dictionary<int, int> TravelAdvBuffs { get; set; } = [];
    public Dictionary<int, int> TravelUltiBuffs { get; set; } = [];
    public Dictionary<int, bool> TravelDebuffs { get; set; } = [];
    public Dictionary<int, bool> TravelInvestBuffs { get; set; } = [];

    // FlagWaveBuffsViewModel
    public bool FlagWaveBuffsEnabled { get; set; }
    public List<FlagWaveBuffSettings> FlagWaveBuffs { get; set; } = [];

    // InGameHotkeysViewModel - 存储为int，模组端转换为Unity的KeyCode
    public int KeySpeedStop { get; set; } = 54; // KeyCode.Alpha6
    public int KeyShowGameInfo { get; set; } = 96; // KeyCode.BackQuote
    public int KeyTopMostCardBank { get; set; } = 9; // KeyCode.Tab
    public int KeyRandomCard { get; set; } = 114; // KeyCode.R
    public int KeyAlmanacCreatePlant { get; set; } = 98; // KeyCode.B
    public int KeyAlmanacCreatePlantVase { get; set; } = 106; // KeyCode.J
    public int KeyAlmanacCreateZombie { get; set; } = 110; // KeyCode.N
    public int KeyAlmanacCreateZombieVase { get; set; } = 107; // KeyCode.K
    public int KeyAlmanacZombieMindCtrl { get; set; } = 306; // KeyCode.LeftControl

    // GodEvolutionViewModel - 神明进化
    public bool GodEvolutionUnlimitedRefresh { get; set; }
    public bool GodEvolutionFreeUpgradeQuality { get; set; }
    public bool GodEvolutionLuckyEnabled { get; set; }
    public double GodEvolutionLucky { get; set; } = 1.0;
    public bool GodEvolutionDifficultyEnabled { get; set; }
    public int GodEvolutionDifficulty { get; set; }
    public bool GodEvolutionRefreshCountEnabled { get; set; }
    public int GodEvolutionRefreshCount { get; set; }
    public bool GodEvolutionMaxPlantCountEnabled { get; set; }
    public int GodEvolutionMaxPlantCount { get; set; }
    public bool GodEvolutionOptionCountEnabled { get; set; }
    public int GodEvolutionOptionCount { get; set; }
    public bool GodEvolutionUpgradeBuffChanceEnabled { get; set; }
    public int GodEvolutionUpgradeBuffChance { get; set; }
    public bool GodEvolutionSuperUpgrade { get; set; }
    public bool GodEvolutionForceSuperQuality { get; set; }
    public bool GodEvolutionUncrashable { get; set; }
    public bool GodEvolutionQualityWeightEnabled { get; set; }
    public double GodEvolutionQualityDefault { get; set; } = 1.0;
    public double GodEvolutionQualitySilver { get; set; } = 1.0;
    public double GodEvolutionQualityGold { get; set; } = 1.0;
    public double GodEvolutionQualityDiamond { get; set; } = 1.0;
    public bool GodEvolutionDamageMultiplierEnabled { get; set; }
    public double GodEvolutionDamageMultiplier { get; set; } = 1.0;
}

/// <summary>
/// 旗帜波词条设置（简化版，只保存ID列表）
/// </summary>
public class FlagWaveBuffSettings
{
    public int Wave { get; set; }
    public List<int> AdvBuffs { get; set; } = [];
    public List<int> UltiBuffs { get; set; } = [];
    public List<int> Debuffs { get; set; } = [];
    public List<int> InvestBuffs { get; set; } = [];
    public string Description { get; set; } = "";
}