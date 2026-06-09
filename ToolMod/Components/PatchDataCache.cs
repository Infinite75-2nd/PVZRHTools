using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using ToolData;
using UnityEngine;

namespace ToolMod.Components;

public class PatchDataCache
{
    #region 游戏内按键

    public static KeyCode KeySpeedSlow => KeyCode.Alpha3;
    public static KeyCode KeySpeedStop { get; set; } = KeyCode.Alpha6;
    public static KeyCode KeyShowGameInfo { get; set; } = KeyCode.BackQuote;
    public static KeyCode KeyTopMostCardBank { get; set; } = KeyCode.Tab;
    public static KeyCode KeyRandomCard { get; set; } = KeyCode.R;
    public static KeyCode KeyAlmanacCreatePlant { get; set; } = KeyCode.B;
    public static KeyCode KeyAlmanacCreatePlantVase { get; set; } = KeyCode.J;
    public static KeyCode KeyAlmanacCreateZombie { get; set; } = KeyCode.N;
    public static KeyCode KeyAlmanacCreateZombieVase { get; set; } = KeyCode.K;
    public static KeyCode KeyAlmanacZombieMindCtrl { get; set; } = KeyCode.LeftControl;

    #endregion

    #region 全局属性

    public static bool ColumnPlanting { get; set; }
    public static bool SeedRain { get; set; }
    public static bool GameSpeedEnabled { get; set; }
    public static float GameSpeed { get; set; }
    public static bool TimeStop { get; set; }
    public static bool TimeSlow { get; set; }
    public static bool ShowGameInfo { get; set; }
    public static bool GloveNoCD { get; set; }
    public static float GloveFullCD { get; set; }
    public static bool HammerNoCD { get; set; }
    public static float HammerFullCD { get; set; }
    public static bool FreePlanting { get; set; }
    public static bool CardFreeCD { get; set; }
    public static bool RemoveFusionLimit { get; set; }
    public static float NewZombieUpdateCD { get; set; }
    public static bool UnlimitedScore { get; set; }
    public static bool UnlimitedRefresh { get; set; }

    #endregion

    #region 游戏内属性

    public static int LockSun { get; set; } = -1;
    public static int LockMoney { get; set; } = -1;
    public static bool PauseSpawn { get; set; } = false;
    public static bool NoFail { get; set; } = false;
    public static ZombieSea ZombieSeaData { get; set; } = new();

    #endregion

    #region 游戏特性修改

    // 礼盒修改
    public static bool SuperPresent { get; set; }
    public static bool UltimateRandomZombie { get; set; }
    public static bool PresentFastOpen { get; set; }
    public static int LockPresent { get; set; } = -1;

    //数值修改
    public static Dictionary<ZombieType, int> ZombieHP { get; set; } = [];
    public static Dictionary<Zombie.FirstArmorType, int> FirstArmorHP { get; set; } = [];
    public static Dictionary<Zombie.SecondArmorType, int> SecondArmorHP { get; set; } = [];
    public static Dictionary<BulletType, int> BulletDamage { get; set; } = [];
    public static int LockBullet { get; set; } = -2;

    // 场地特性
    public static bool NoIceRoad { get; set; }
    public static bool NoHole { get; set; }
    public static bool ItemExistForever { get; set; }
    public static bool JackboxNotExplode { get; set; }
    public static bool GarlicDay { get; set; }
    public static bool UnlimitedSunlight { get; set; }
    public static bool UnlockRedCardPlants { get; set; }
    public static bool PotSmashingFix { get; set; }
    public static bool DisableIceEffect { get; set; }

    // 植物特性
    public static bool FastShooting { get; set; }
    public static bool HardPlant { get; set; }
    public static bool ImmuneForceDeduct { get; set; }
    public static bool CurseImmunity { get; set; }
    public static bool CrushImmunity { get; set; }
    public static bool TrampleImmunity { get; set; }
    public static bool PickaxeImmunity { get; set; }
    public static bool UndeadBullet { get; set; }
    public static bool OldObsidianBullet { get; set; }
    public static bool UltimateSuperGatling { get; set; }
    public static bool HyponoEmperorNoCD { get; set; }
    public static bool MagnetNutUnlimited { get; set; }
    public static bool MineNoCD { get; set; }
    public static bool ChomperNoCD { get; set; }
    public static bool CobCannonNoCD { get; set; }
    public static bool PlantUpgrade { get; set; }
    public static bool SuperStarNoCD { get; set; }
    public static int LockWheat { get; set; } = -1;

    // 僵尸特性
    public static int ZombieDamageLimit { get; set; } = -1;
    public static float ZombieSpeedMultiplier { get; set; } = 1.0f;
    public static float ZombieAttackMultiplier { get; set; } = 1.0f;
    public static int ZombieBulletReflect { get; set; } = -1;
    public static bool ZombieStatusCoexist { get; set; }
    public static bool ZombieImmuneAllDebuffs { get; set; }
    public static bool ZombieImmuneFreeze { get; set; }
    public static bool ZombieImmuneCold { get; set; }
    public static bool ZombieImmuneButter { get; set; }
    public static bool ZombieImmunePoison { get; set; }
    public static bool ZombieImmuneJalaed { get; set; }
    public static bool ZombieImmuneEmbered { get; set; }
    public static bool ZombieImmuneKnockback { get; set; }
    public static bool ZombieImmuneMindControl { get; set; }
    public static bool ZombieImmuneDevour { get; set; }

    // 其他特性
    public static bool AutoCutFruit { get; set; }
    public static bool RandomCard { get; set; }
    public static bool ColumnGlove { get; set; }
    public static bool UnlimitedCardSlots { get; set; }
    public static bool RandomBullet { get; set; }
    public static bool AutoRhythmGame { get; set; }
    public static bool StarUpBuff { get; set; }

    #endregion

    #region 布阵器

    public static bool KillUpgrade { get; set; }
    public static bool RandomUpgradeMode { get; set; }
    public static PlantType Present1PlantIndex { get; set; }
    public static PlantType Present2PlantIndex { get; set; }
    public static PlantType Present3PlantIndex { get; set; }
    public static PlantType Present4PlantIndex { get; set; }
    public static PlantType Present5PlantIndex { get; set; }
    public static ZombieType ZombieSlot1Index { get; set; }
    public static ZombieType ZombieSlot2Index { get; set; }
    public static ZombieType ZombieSlot3Index { get; set; }
    public static ZombieType ZombieSlot4Index { get; set; }
    public static ZombieType ZombieSlot5Index { get; set; }
    public static ZombieType ZombieSlot6Index { get; set; }

    #endregion

    #region 词条

    public static Dictionary<AdvBuff, int> AdvBuffs { get; set; } = [];
    public static Dictionary<UltiBuff, int> UltiBuffs { get; set; } = [];
    public static Dictionary<TravelDebuff, bool> Debuffs { get; set; } = [];
    public static Dictionary<InvestBuff, bool> InvestBuffs { get; set; } = [];
    public static Dictionary<AdvBuff, int> InGameAdvBuffs { get; set; } = [];
    public static Dictionary<UltiBuff, int> InGameUltiBuffs { get; set; } = [];
    public static Dictionary<InvestBuff, bool> InGameInvestBuffs { get; set; } = [];
    public static Dictionary<TravelDebuff, bool> InGameDebuffs { get; set; } = [];
    public static FlagWaveBuff[] FlagWaveBuffs { get; set; } = new FlagWaveBuff[10];
    public static bool FlagWaveBuffsEnabled { get; set; }

    #endregion

    #region 对象缓存

    public static GameObject? SeedGroup =>
        Utils.InGame && InGameUI.Instance != null ? InGameUI.Instance.SeedBank : null;

    public static GameLose? CheckLose { get; set; }

    public static Board.BoardTag? OriginalBoardTag { get; set; }

    #endregion

    #region 对象监视

    [HideFromIl2Cpp]
    public static Il2CppSystem.Collections.Generic.List<Il2CppSystem.Collections.Generic.List<ZombieSpawnData>>
        ZombieSpawnDataList => InitZombieList.zombieList;

    #endregion

    #region 状态变量

    public static int SeaTime;
    public static int GarlicDayTime;
    public static Dictionary<int, int> PlantHealthCache { get; set; } = [];
    public static readonly Dictionary<int, float> ZombieOriginalSpeeds = [];
    public static readonly Dictionary<int, int> ZombieOriginalAttackDamages = [];
    public static Dictionary<int, PlantType> ZombieLastDamageSource { get; set; } = [];
    public static float TrampleImmunityTimer = 0f;
    public const float TrampleImmunityInterval = 0.1f;
    public static float CurseClearTimer = 0f;
    public const float CurseClearInterval = 1f;
    public static readonly HashSet<int> ModifiedPlants = [];

    public static readonly HashSet<int> InitializedPlants = [];

    //public static Dictionary<int, (bool hadCold, float coldTimer, float freezeTimer, int freezeLevel)> ZombieStatusCoexistData = [];
    public static readonly List<GameObject> CopiedCards = [];
    public static bool AllowBuffRemoval = true;
    public static bool LastHugeWaveState = false;
    public static readonly Dictionary<int, int> PveBlindBoxSlotByInstance = [];
    public static int FlagWaveUnlockIndex = 0;
    public static int LastUnlockWave = -1;
    public static int CurrentFlagWaveIndex = 0;
    public static bool NewBoard = false;
    public static int _snapshotIntervalSeconds = 5;
    public static int _snapshotDurationSeconds = 60;
    public static DateTime _lastSnapshotTime = DateTime.MinValue;
    public static bool _rewindOnFailEnabled = false;
    public static int _restoreSecondsBeforeFail = 8;
    public static int PendingManualSnapshotFrames = 0;
    public static bool CheckedAutoRestore = false;
    public static readonly List<Snapshot> Snapshots = [];
    public static PlantType AlmanacSeedType = PlantType.Nothing;
    public static bool AlmanacZombieMindCtrl = false;
    public static ZombieType AlmanacZombieType = ZombieType.Nothing;

    #endregion
}