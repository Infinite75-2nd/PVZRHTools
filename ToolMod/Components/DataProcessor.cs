using System;
using System.Collections.Generic;
using System.Text.Json;
using Core;
using GameLevel.Abyss;
using GameLevel.RogueShooting;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ToolData;
using Unity.VisualScripting;
using UnityEngine;
using ZenGarden;
using static ToolMod.Utils;
using static ToolMod.Components.PatchDataCache;
using GardenData = ZenGarden.GardenData;
using Object = UnityEngine.Object;

namespace ToolMod.Components;

public class DataProcessor : MonoBehaviour
{
    public DataProcessor() : base(ClassInjector.DerivedConstructorPointer<DataProcessor>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    public DataProcessor(IntPtr ptr) : base(ptr)
    {
    }

    public void Awake()
    {
        Instance = this;
    }

    public void OnApplicationQuit()
    {
        ModCore.Instance.Unload();
    }

    [HideFromIl2Cpp] public Queue<SyncData> Buffer { get; set; } = [];
    public static DataProcessor? Instance { get; private set; }

    public void Update()
    {
        if (Buffer.Count is 0) return;
        lock (Buffer)
        {
            var command = Buffer.Dequeue();
            if (OverallCommands.ContainsKey(command.Command))
                OverallCommands[command.Command](command.Parameters);
            else if (InGame && InGameCommands.ContainsKey(command.Command))
                InGameCommands[command.Command](command.Parameters);
        }
    }

    [HideFromIl2Cpp]
    private static Dictionary<string, Action<List<string>>> OverallCommands => new()
    {
        { Strings.Exit, Exit },

        #region 通用修改

        { Strings.DevMode, SimpleSyncBool(() => GameAPP.developerMode) },
        { Strings.ColumnPlanting, SimpleSyncBool(() => ColumnPlanting) },
        { Strings.SeedRain, SimpleSyncBool(() => SeedRain) },
        { Strings.GameSpeedEnabled, SimpleSyncBool(() => GameSpeedEnabled) },
        { Strings.GameSpeed, SimpleSyncFloat(() => GameSpeed) },
        { Strings.GloveNoCD, SimpleSyncBool(() => GloveNoCD) },
        { Strings.HammerNoCD, SimpleSyncBool(() => HammerNoCD) },
        { Strings.HammerFullCD, SimpleSyncFloat(() => HammerFullCD) },
        { Strings.GloveFullCD, SimpleSyncFloat(() => GloveFullCD) },
        { Strings.WheelNoCD, SimpleSyncBool(() => WheelNoCD) },
        { Strings.WheelFullCD, SimpleSyncFloat(() => WheelFullCD) },
        { Strings.FreePlanting, SimpleSyncBool(() => FreePlanting) },
        { Strings.CardFreeCD, SimpleSyncBool(() => CardFreeCD) },
        { Strings.RemoveFusionLimit, SimpleSyncBool(() => RemoveFusionLimit) },
        { Strings.NewZombieUpdateCD, SimpleSyncFloat(() => NewZombieUpdateCD) },
        { Strings.UnlimitedScore, SimpleSyncBool(() => UnlimitedScore) },
        { Strings.UnlimitedRefresh, SimpleSyncBool(() => UnlimitedRefresh) },

        { Strings.LockSun, SimpleSyncInt(() => LockSun) },
        { Strings.LockMoney, SimpleSyncInt(() => LockMoney) },
        { Strings.LockLightLevel, SimpleSyncInt(() => LockLightLevel) },
        { Strings.PauseSpawn, SimpleSyncBool(() => PauseSpawn) },
        { Strings.NoFail, SimpleSyncBool(() => NoFail) },

        { Strings.ZombieSea, ZombieSea },

        #endregion

        #region 游戏特性修改

        // 礼盒修改
        { Strings.SuperPresent, SimpleSyncBool(() => SuperPresent) },
        { Strings.UltimateRandomZombie, SimpleSyncBool(() => UltimateRandomZombie) },
        { Strings.PresentFastOpen, SimpleSyncBool(() => PresentFastOpen) },
        { Strings.LockPresent, SimpleSyncInt(() => LockPresent) },

        //数值修改
        { Strings.SetZombieHP, SetZombieHP },
        { Strings.SetFirstArmorHP, SetFirstArmorHP },
        { Strings.SetSecondArmorHP, SetSecondArmorHP },
        { Strings.SetBulletDamage, SetBulletDamage },
        { Strings.LockBullet, SimpleSyncInt(() => LockBullet) },

        // 场地特性
        { Strings.NoIceRoad, SimpleSyncBool(() => NoIceRoad) },
        { Strings.NoHole, SimpleSyncBool(() => NoHole) },
        { Strings.ItemExistForever, SimpleSyncBool(() => ItemExistForever) },
        { Strings.JackboxNotExplode, SimpleSyncBool(() => JackboxNotExplode) },
        { Strings.GarlicDay, SimpleSyncBool(() => GarlicDay) },
        { Strings.UnlimitedSunlight, SimpleSyncBool(() => UnlimitedSunlight) },
        { Strings.UnlockRedCardPlants, SimpleSyncBool(() => UnlockRedCardPlants) },
        { Strings.PotSmashingFix, SimpleSyncBool(() => PotSmashingFix) },
        { Strings.DisableIceEffect, SimpleSyncBool(() => DisableIceEffect) },

        // 植物特性
        { Strings.FastShooting, SimpleSyncBool(() => FastShooting) },
        { Strings.HardPlant, SimpleSyncBool(() => HardPlant) },
        { Strings.ImmuneForceDeduct, SimpleSyncBool(() => ImmuneForceDeduct) },
        { Strings.CurseImmunity, SimpleSyncBool(() => CurseImmunity) },
        { Strings.CrushImmunity, SimpleSyncBool(() => CrushImmunity) },
        { Strings.TrampleImmunity, SimpleSyncBool(() => TrampleImmunity) },
        { Strings.PickaxeImmunity, SimpleSyncBool(() => PickaxeImmunity) },
        { Strings.UndeadBullet, SimpleSyncBool(() => UndeadBullet) },
        { Strings.OldObsidianBullet, SimpleSyncBool(() => OldObsidianBullet) },
        { Strings.UltimateSuperGatling, SimpleSyncBool(() => UltimateSuperGatling) },
        { Strings.HyponoEmperorNoCD, SimpleSyncBool(() => HyponoEmperorNoCD) },
        { Strings.MagnetNutUnlimited, SimpleSyncBool(() => MagnetNutUnlimited) },
        { Strings.MineNoCD, SimpleSyncBool(() => MineNoCD) },
        { Strings.ChomperNoCD, SimpleSyncBool(() => ChomperNoCD) },
        { Strings.CobCannonNoCD, SimpleSyncBool(() => CobCannonNoCD) },
        { Strings.PlantUpgrade, SimpleSyncBool(() => PlantUpgrade) },
        { Strings.SuperStarNoCD, SimpleSyncBool(() => SuperStarNoCD) },
        { Strings.LockWheat, SimpleSyncInt(() => LockWheat) },
        { Strings.ApplyAllPlantSkins, ApplyAllPlantSkins },
        { Strings.ObtainAllPlantSkins, ObtainAllPlantSkins },

        // 僵尸特性
        { Strings.ZombieDamageLimit, SimpleSyncInt(() => ZombieDamageLimit) },
        { Strings.ZombieSpeedMultiplier, SimpleSyncFloat(() => ZombieSpeedMultiplier) },
        { Strings.ZombieAttackMultiplier, SimpleSyncFloat(() => ZombieAttackMultiplier) },
        { Strings.ZombieBulletReflect, SimpleSyncInt(() => ZombieBulletReflect) },
        { Strings.ZombieStatusCoexist, SimpleSyncBool(() => ZombieStatusCoexist) },
        { Strings.ZombieImmuneAllDebuffs, SimpleSyncBool(() => ZombieImmuneAllDebuffs) },
        { Strings.ZombieImmuneFreeze, SimpleSyncBool(() => ZombieImmuneFreeze) },
        { Strings.ZombieImmuneCold, SimpleSyncBool(() => ZombieImmuneCold) },
        { Strings.ZombieImmuneButter, SimpleSyncBool(() => ZombieImmuneButter) },
        { Strings.ZombieImmunePoison, SimpleSyncBool(() => ZombieImmunePoison) },
        { Strings.ZombieImmuneJalaed, SimpleSyncBool(() => ZombieImmuneJalaed) },
        { Strings.ZombieImmuneEmbered, SimpleSyncBool(() => ZombieImmuneEmbered) },
        { Strings.ZombieImmuneKnockback, SimpleSyncBool(() => ZombieImmuneKnockback) },
        { Strings.ZombieImmuneMindControl, SimpleSyncBool(() => ZombieImmuneMindControl) },
        { Strings.ZombieImmuneDevour, SimpleSyncBool(() => ZombieImmuneDevour) },

        // 其他特性
        { Strings.AutoCutFruit, SimpleSyncBool(() => AutoCutFruit) },
        { Strings.RandomCard, SimpleSyncBool(() => RandomCard) },
        { Strings.ColumnGlove, SimpleSyncBool(() => ColumnGlove) },
        { Strings.UnlimitedCardSlots, SimpleSyncBool(() => UnlimitedCardSlots) },
        { Strings.RandomBullet, SimpleSyncBool(() => RandomBullet) },
        { Strings.AutoRhythmGame, SimpleSyncBool(() => AutoRhythmGame) },
        { Strings.StarUpBuff, SimpleSyncBool(() =>PatchDataCache.StarUpBuff) },

        // 神秘模式
        { Strings.TreasureFreeUpgrade, SimpleSyncBool(() => TreasureFreeUpgrade) },
        { Strings.TreasureFreeWithdraw, SimpleSyncBool(() => TreasureFreeWithdraw) },
        { Strings.TreasureMaxTime, SimpleSyncInt(() => TreasureMaxTime) },
        { Strings.TreasureAllRedCard, SimpleSyncBool(() => TreasureAllRedCard) },
        { Strings.TreasureSetMoney, SimpleSyncInt(()=>TreasureData.treasureMoney) },
        { Strings.TreasureFillCard, TreasureFillCard },
        { Strings.TreasureSellAllCards, TreasureSellAllCards },

        // 花园修改
        { Strings.ZenGardenSetMoney, SimpleSyncLong(()=>GameAPP.theMoneyCount) },
        { Strings.ZenGardenSetCoin, SimpleSyncInt(()=>GardenUI.Data.coinCount) },
        { Strings.ZenGardenGetPlant, ZenGardenGetPlant },

        // 深渊模式
        { Strings.AbyssJumpLevel, AbyssJumpLevel },
        { Strings.AbyssMoney, AbyssMoney },
        { Strings.AbyssLimitlessRefresh, SimpleSyncBool(() => AbyssLimitlessRefresh) },
        { Strings.AbyssRemoveSuperSunNutLimit, SimpleSyncBool(() => AbyssRemoveSuperSunNutLimit) },
        { Strings.AbyssMaxPlantCount, SimpleSyncInt(() => AbyssMaxPlantCount, () =>
        {
            if (AbyssManager.Instance!=null)
            {
                //AbyssManager.Instance.maxPlantCount = AbyssMaxPlantCount>=0?AbyssMaxPlantCount:OriginalAbyssMaxPlantCount;
            }
        }) },
        { Strings.AbyssMaxSuperCount, SimpleSyncInt(() => AbyssMaxSuperCount, () =>
        {
            if (AbyssManager.Instance!=null)
            {
                //AbyssManager.Instance.superPlantCount = AbyssMaxSuperCount>=0?AbyssMaxSuperCount:OriginalAbyssMaxSuperCount;
            }
        })  },
        { Strings.AbyssMaxUltimateCount, SimpleSyncInt(() => AbyssMaxUltimateCount, () =>
        {
            if (AbyssManager.Instance!=null)
            {
                //AbyssManager.Instance.ultiPlantCount = AbyssMaxUltimateCount>=0?AbyssMaxUltimateCount:OriginalAbyssMaxUltimateCount;
            }
        })  },

        #endregion

        #region 精细出怪修改

        { Strings.GetZombiesList, GetZombiesList },

        #endregion

        #region 布阵器

        { Strings.KillUpgrade, SimpleSyncBool(() => KillUpgrade) },
        { Strings.RandomUpgradeMode, SimpleSyncBool(() => RandomUpgradeMode) },

        // 植物布阵
        { Strings.GetPlantFormationCode, GetPlantFormationCode },
        { Strings.ApplyPlantFormation, ApplyPlantFormation },

        // 僵尸布阵
        { Strings.GetZombieFormationCode, GetZombieFormationCode },
        { Strings.ApplyZombieFormation, ApplyZombieFormation },

        // 混合布阵
        { Strings.GetMixedFormationCode, GetMixedFormationCode },
        { Strings.ApplyMixedFormation, ApplyMixedFormation },

        // 罐子布阵
        { Strings.GetPotFormationCode, GetPotFormationCode },
        { Strings.ApplyPotFormation, ApplyPotFormation },

        // 斗蛐蛐布阵
        { Strings.ApplyBattleFormation, ApplyBattleFormation },

        #endregion

        #region 旅行词条

        { Strings.UpdateAdvBuff, UpdateAdvBuff },
        { Strings.UpdateUltiBuff, UpdateUltiBuff },
        { Strings.UpdateDebuff, UpdateDebuff },
        { Strings.UpdateInvestBuff, UpdateInvestBuff },
        { Strings.UpdateUnlockedPlant, UpdateUnlockedPlant },
        { Strings.UpdateAllBuffs, UpdateAllBuffs },

        #endregion

        #region 旗帜波词条

        { Strings.FlagWaveBuff, FlagWaveBuff },
        { Strings.FlagWaveBuffsEnabled, SimpleSyncBool(() => FlagWaveBuffsEnabled) },

        #endregion

        #region 诸神进化

        {
            Strings.GodEvolutionUnlimitedRefresh,
            SimpleSyncBool(() => GodEvolutionUnlimitedRefresh)
        },
        {
            Strings.GodEvolutionFreeUpgradeQuality,
            SimpleSyncBool(() => GodEvolutionFreeUpgradeQuality)
        },
        { Strings.GodEvolutionLucky, SimpleSyncFloat(() => GodEvolutionLucky) },
        {
            Strings.GodEvolutionDifficulty,
            SimpleSyncInt(() => GodEvolutionDifficulty)
        },
        {
            Strings.GodEvolutionRefreshCount,
            SimpleSyncInt(() => GodEvolutionRefreshCount)
        },
        {
            Strings.GodEvolutionMaxPlantCount,
            SimpleSyncInt(() => GodEvolutionMaxPlantCount)
        },
        {
            Strings.GodEvolutionSuperUpgrade,
            SimpleSyncBool(() => GodEvolutionSuperUpgrade)
        },
        {
            Strings.GodEvolutionForceSuperQuality,
            SimpleSyncBool(() => GodEvolutionForceSuperQuality)
        },
        {
            Strings.GodEvolutionUncrashable,
            SimpleSyncBool(() => GodEvolutionUncrashable)
        },
        {
            Strings.GodEvolutionQualityWeightEnabled,
            SimpleSyncBool(() => GodEvolutionQualityWeightEnabled)
        },
        {
            Strings.GodEvolutionQualityDefault,
            SimpleSyncFloat(() => GodEvolutionQualityDefault)
        },
        {
            Strings.GodEvolutionQualitySilver,
            SimpleSyncFloat(() => GodEvolutionQualitySilver)
        },
        {
            Strings.GodEvolutionQualityGold,
            SimpleSyncFloat(() => GodEvolutionQualityGold)
        },
        {
            Strings.GodEvolutionQualityDiamond,
            SimpleSyncFloat(() => GodEvolutionQualityDiamond)
        },
        {
            Strings.GodEvolutionDamageMultiplier,
            SimpleSyncFloat(() => GodEvolutionDamageMultiplier)
        },
        { Strings.GodEvolutionUnlockAll, GodEvolutionUnlockAll },
        { Strings.GodEvolutionMultiSelectBuff, SimpleSyncBool(() => GodEvolutionMultiSelectBuff) },
        { Strings.GodEvolutionChooseBuff, GodEvolutionChooseBuff },
        { Strings.GodEvolutionCheatHard, SimpleSyncBool(() => GodEvolutionCheatHard) },
        { Strings.GodEvolutionForceExpertBuff, SimpleSyncBool(() => GodEvolutionForceExpertBuff) },
        { Strings.GodEvolutionRemoveStarsStarUp, GodEvolutionRemoveStarsStarUp },
        #endregion

        #region 游戏内按键绑定

        { Strings.KeySpeedStop, SimpleSyncKeyCode(() => KeySpeedStop) },
        { Strings.KeyShowGameInfo, SimpleSyncKeyCode(() => KeyShowGameInfo) },
        { Strings.KeyTopMostCardBank, SimpleSyncKeyCode(() => KeyTopMostCardBank) },
        { Strings.KeyRandomCard, SimpleSyncKeyCode(() => KeyRandomCard) },
        { Strings.KeyAlmanacCreatePlant, SimpleSyncKeyCode(() => KeyAlmanacCreatePlant) },
        { Strings.KeyAlmanacCreatePlantVase, SimpleSyncKeyCode(() => KeyAlmanacCreatePlantVase) },
        { Strings.KeyAlmanacCreateZombie, SimpleSyncKeyCode(() => KeyAlmanacCreateZombie) },
        { Strings.KeyAlmanacCreateZombieVase, SimpleSyncKeyCode(() => KeyAlmanacCreateZombieVase) },
        { Strings.KeyAlmanacZombieMindCtrl, SimpleSyncKeyCode(() => KeyAlmanacZombieMindCtrl) },

        #endregion

        { Strings.PlaySound, PlaySound },
    };

    private static Dictionary<string, Action<List<string>>> InGameCommands => new()
    {
        { Strings.ChangeZombiesList, ChangeZombiesList },
        { Strings.Sun, SimpleSyncInt(() => Board.Instance.theSun) },
        { Strings.Money, SimpleSyncInt(() => Board.Instance.theMoney) },
        { Strings.RemoveAllPlants, RemoveAllPlants },
        { Strings.RemoveAllZombies, RemoveAllZombies },
        { Strings.MindCtrlAllZombies, MindCtrlAllZombies },
        { Strings.RemoveAllIceRoads, RemoveAllIceRoads },
        { Strings.RemoveAllHoles, RemoveAllHoles },
        { Strings.RemoveAllGraves, RemoveAllGraves },
        { Strings.SetLevelName, SetLevelName },
        { Strings.SetZombiesIdle, SetZombiesIdle },
        { Strings.CreatePlant, CreatePlant },
        { Strings.CreateZombie, CreateZombie },
        { Strings.CreatePlantCard, CreatePlantCard },
        { Strings.CreateMindControlledZombie, CreateMindControlledZombie },
        { Strings.CreatePlantVase, CreatePlantVase },
        { Strings.CreateZombieVase, CreateZombieVase },
        { Strings.CreateItem, CreateItem },
        { Strings.CreatePassiveMeteorite, CreatePassiveMeteorite },
        { Strings.CreateActiveMeteorite, CreateActiveMeteorite },
        { Strings.CreateUltimateMeteorite, CreateUltimateMeteorite },
        { Strings.CreateSolarMeteorite, CreateSolarMeteorite },
        { Strings.NextWave, NextWave },
        { Strings.SetJumpWave, SetJumpWave },
        { Strings.SetAward, SetAward },
        { Strings.DestroyAward, DestroyAward },
        { Strings.ShowText, ShowText },
        { Strings.StartMower, StartMower },
        { Strings.CreateMower, CreateMower },
        { Strings.UpdateInGameAdvBuff, UpdateInGameAdvBuff },
        { Strings.UpdateInGameUltiBuff, UpdateInGameUltiBuff },
        { Strings.UpdateInGameDebuff, UpdateInGameDebuff },
        { Strings.UpdateInGameInvestBuff, UpdateInGameInvestBuff },
        { Strings.UpdateInGameUnlockedPlant, UpdateInGameUnlockedPlant },
        { Strings.PlayParticle, PlayParticle },
        { Strings.GetSnapshot, GetSnapshot },
        { Strings.RestoreSnapshot, RestoreSnapshot },
        { Strings.GodEvolutionResetQuality, GodEvolutionResetQuality },
        { Strings.SpawnPetGargantuar, SpawnPetGargantuar },
        { Strings.SpawnPetFootball, SpawnPetFootball },
        { Strings.SpawnPetSnowBoss, SpawnPetSnowBoss },
        { Strings.SpawnPetJackbox, SpawnPetJackbox },
        { Strings.SpawnPetDrown, SpawnPetDrown },
        { Strings.SpawnPetHorse, SpawnPetHorse },
        { Strings.SpawnPetImp, SpawnPetImp },
        { Strings.SpawnPetKirov, SpawnPetKirov },
    };

    #region OverallCommands

    private static void ZombieSea(List<string> args)
    {
        ZombieSeaData = JsonSerializer.Deserialize<ZombieSea>(args[0]);
    }

    private static void SetZombieHP(List<string> args)
    {
        var id = (ZombieType)int.Parse(args[0]);
        var hp = int.Parse(args[1]);
        if (ZombieHP.ContainsKey(id)) ZombieHP[id] = hp;
    }

    private static void SetFirstArmorHP(List<string> args)
    {
        var id = (Zombie.FirstArmorType)int.Parse(args[0]);
        var hp = int.Parse(args[1]);
        if (FirstArmorHP.ContainsKey(id)) FirstArmorHP[id] = hp;
    }

    private static void SetSecondArmorHP(List<string> args)
    {
        var id = (Zombie.SecondArmorType)int.Parse(args[0]);
        var hp = int.Parse(args[1]);
        if (SecondArmorHP.ContainsKey(id)) SecondArmorHP[id] = hp;
    }

    private static void SetBulletDamage(List<string> args)
    {
        var id = (BulletType)int.Parse(args[0]);
        var damage = int.Parse(args[1]);
        if (BulletDamage.ContainsKey(id)) BulletDamage[id] = damage;
    }

    #endregion

    private static void Exit(List<string> _)
    {
        Application.Quit();
    }

    #region InGameCommands

    private static void RemoveAllPlants(List<string> _)
    {
        var allPlants = Lawnf.GetAllPlants();
        if (allPlants != null)
        {
            for (var i = allPlants.Count - 1; i >= 0; i--) allPlants[i]?.Die();
        }
    }

    private static void RemoveAllZombies(List<string> _)
    {
        for (var j = Board.Instance.zombieArray.Count - 1; j >= 0; j--)
            try
            {
                var zombie = Board.Instance.zombieArray[j];
                if (zombie == null || !zombie||
                    (zombie.TryGetComponent<BoxCollider2D>(out var boxCollider2D) && !boxCollider2D.enabled&&zombie.isIdle)||
                    (zombie.TryGetComponent<PolygonCollider2D>(out var polygonCollider2D) && !polygonCollider2D.enabled&&zombie.isIdle))
                    continue;
                zombie.ApplyDamage(DamageType.MaxDamage, 2147483647);
                zombie.BodyTakeDamage(2147483647);
                zombie.Die();
            }
            catch
            {
            }
        Il2CppReferenceArray<Object> zombies = FindObjectsOfTypeAll(Il2CppType.Of<Zombie>());
        for (var i = zombies.Count - 1; i >= 0; i--)
        {
            try
            {
                var zombie = (Zombie)zombies[i];
                if (zombie == null || !zombie||
                    (zombie.TryGetComponent<BoxCollider2D>(out var boxCollider2D) && !boxCollider2D.enabled&&zombie.isIdle)||
                    (zombie.TryGetComponent<PolygonCollider2D>(out var polygonCollider2D) && !polygonCollider2D.enabled&&zombie.isIdle))
                    continue;
                zombie.ApplyDamage(DamageType.MaxDamage, 2147483647);
                zombie.BodyTakeDamage(2147483647);
                zombie.Die();
            }
            catch
            {
            }
        }
    }

    private static void MindCtrlAllZombies(List<string> _)
    {
        Il2CppReferenceArray<Object> zombies = FindObjectsOfTypeAll(Il2CppType.Of<Zombie>());
        for (var i = zombies.Count - 1; i >= 0; i--)
            try
            {
                ((Zombie)zombies[i])?.SetMindControl();
            }
            catch
            {
            }

        for (var j = Board.Instance.zombieArray.Count; j >= 0; j--)
            try
            {
                Board.Instance.zombieArray[j]?.SetMindControl();
            }
            catch
            {
            }
    }

    private static void RemoveAllIceRoads(List<string> _)
    {
        try
        {
            // 直接设置冰道位置，无视觉效果无伤害
            // IceRoad 字段: offset 0x30 = theX (当前位置), offset 0x34 = originalX (原始位置)
            for (var i = 0; i < Board.Instance.iceRoads.Count; i++)
            {
                try
                {
                    var iceRoad = Board.Instance.iceRoads[i];
                    if (iceRoad != null)
                    {
                        // 使用反射或直接字段访问将 theX 设为 originalX
                        // IceRoad 的 theX 和 originalX 是 float 类型
                        var ptr = iceRoad.Pointer;
                        unsafe
                        {
                            float* theXPtr = (float*)(ptr + 0x30);
                            float* originalXPtr = (float*)(ptr + 0x34);
                            *theXPtr = *originalXPtr;
                        }
                    }
                }
                catch
                {
                }
            }
        }
        catch
        {
        }
    }

    private static void RemoveAllHoles(List<string> _)
    {
        for (var i = Board.Instance.griditemArray.Count - 1; i >= 0; i--)
        {
            if (Board.Instance.griditemArray[i].theItemType is GridItemType.CraterDay or GridItemType.CraterNight)
            {
                Destroy(Board.Instance.griditemArray[i].gameObject);
                Board.Instance.griditemArray.RemoveAt(i);
            }
        }
    }

    private static void RemoveAllGraves(List<string> _)
    {
        for (var i = Board.Instance.griditemArray.Count - 1; i >= 0; i--)
        {
            if (Board.Instance.griditemArray[i].theItemType is GridItemType.Grave)
            {
                Destroy(Board.Instance.griditemArray[i].gameObject);
                Board.Instance.griditemArray.RemoveAt(i);
            }
        }
    }

    private static void SetLevelName(List<string> args) => InGameUI.Instance?.SetLevelName(args[0]);

    private static void SetZombiesIdle(List<string> args)
    {
        foreach (var z in Board.Instance.zombieArray)
        {
            if (z == null) continue;
            z?.anim.Play("idle");
        }
    }

    private static void CreatePlant(List<string> args)
    {
        var id = int.Parse(args[0]);
        var r = int.Parse(args[1]);
        var c = int.Parse(args[2]);
        var repeatTimes = int.Parse(args[3]);
        try
        {
            for (var n = 0; n < repeatTimes; n++)
            {
                if (r * r + c * c == 0)
                {
                    for (var i = 0; i < Board.Instance!.rowNum; i++)
                    for (var j = 0; j < Board.Instance.columnNum; j++)
                        global::CreatePlant.Instance.SetPlant(j, i, id is -1 ? GameAPP.resourcesManager.allPlants.GetRandom():(PlantType)id);

                    continue;
                }

                if (r == 0 && c != 0)
                {
                    for (var j = 0; j < Board.Instance!.columnNum; j++)
                        global::CreatePlant.Instance.SetPlant(c - 1, j, id is -1 ? GameAPP.resourcesManager.allPlants.GetRandom():(PlantType)id);

                    continue;
                }

                if (c == 0 && r != 0)
                {
                    for (var j = 0; j < Board.Instance!.columnNum; j++)
                        global::CreatePlant.Instance.SetPlant(j, r - 1, id is -1 ? GameAPP.resourcesManager.allPlants.GetRandom():(PlantType)id);

                    continue;
                }

                if (c > 0 && r > 0 && c <= Board.Instance!.columnNum && r <= Board.Instance.rowNum)
                    global::CreatePlant.Instance.SetPlant(c - 1, r - 1, id is -1 ? GameAPP.resourcesManager.allPlants.GetRandom():(PlantType)id);
            }
        }
        catch
        {
            if (id is 300)
                InGameText.Instance.ShowText("不支持此操作，用豌豆代替", 5);
            else
                throw;
        }
    }

    private static void CreatePlantCard(List<string> args)
    {
        var plantType = (PlantType)int.Parse(args[0]);
        var repeatTimes = int.Parse(args[1]);
        if (InGameUI.Instance == null) return;
        for (int i = 0; i < repeatTimes; i++)
        {
            var droppedCard = Lawnf.SetDroppedCard(new Vector2(0f, 0f), plantType is PlantType.Nothing ? GameAPP.resourcesManager.allPlants.GetRandom():plantType);
            if (droppedCard != null)
                droppedCard.GameObject().transform.SetParent(InGameUI.Instance.transform);
        }
    }

    private static void CreatePlantVase(List<string> args)
    {
        var id = int.Parse(args[0]);
        var r = int.Parse(args[1]);
        var c = int.Parse(args[2]);
        var PvPPotRange = bool.Parse(args[3]);
        if (PvPPotRange)
        {
            for (var i = 0; i < Board.Instance!.rowNum; i++)
            for (var j = 3; j < Board.Instance.columnNum; j++)
                GridItem.SetGridItem(j, i, GridItemType.ScaryPot).Cast<ScaryPot>().thePlantType =
                    id is -1 ? GameAPP.resourcesManager.allPlants.GetRandom():(PlantType)id;
        }
        else
        {
            if (r * r + c * c == 0)
                for (var i = 0; i < Board.Instance!.rowNum; i++)
                for (var j = 0; j < Board.Instance.columnNum; j++)
                    GridItem.SetGridItem(j, i, GridItemType.ScaryPot).Cast<ScaryPot>().thePlantType =
                        id is -1 ? GameAPP.resourcesManager.allPlants.GetRandom():(PlantType)id;

            if (r == 0 && c != 0)
                for (var j = 0; j < Board.Instance!.columnNum; j++)
                    GridItem.SetGridItem(c - 1, j, GridItemType.ScaryPot).Cast<ScaryPot>().thePlantType =
                        id is -1 ? GameAPP.resourcesManager.allPlants.GetRandom():(PlantType)id;

            if (c == 0 && r != 0)
                for (var j = 0; j < Board.Instance!.columnNum; j++)
                    GridItem.SetGridItem(j, r - 1, GridItemType.ScaryPot).Cast<ScaryPot>().thePlantType =
                        id is -1 ? GameAPP.resourcesManager.allPlants.GetRandom():(PlantType)id;

            if (c > 0 && r > 0 && c <= Board.Instance!.columnNum && r <= Board.Instance.rowNum)
                GridItem.SetGridItem(c - 1, r - 1, GridItemType.ScaryPot).Cast<ScaryPot>().thePlantType =
                    id is -1 ? GameAPP.resourcesManager.allPlants.GetRandom():(PlantType)id;
        }
    }

    private static void CreateZombie(List<string> args)
    {
        var id = int.Parse(args[0]);
        var r = int.Parse(args[1]);
        var c = int.Parse(args[2]);
        var repeatTimes = int.Parse(args[3]);
        if (repeatTimes > 50) repeatTimes = 50;
        for (var n = 0; n < repeatTimes; n++)
        {
            if (r * r + c * c == 0)
            {
                for (var i = 0; i < Board.Instance.rowNum; i++)
                for (var j = 0; j < Board.Instance.columnNum; j++)
                    global::CreateZombie.Instance.SetZombie(i, id is -1? GameAPP.resourcesManager.allZombieTypes.GetRandom():(ZombieType)id , -5f + j * 1.37f);
                continue;
            }

            if (r == 0 && c != 0)
            {
                for (var j = 0; j < Board.Instance.rowNum; j++)
                    global::CreateZombie.Instance.SetZombie(j, id is -1? GameAPP.resourcesManager.allZombieTypes.GetRandom():(ZombieType)id, -5f + (c - 1) * 1.37f);
                continue;
            }

            if (c == 0 && r != 0)
            {
                for (var j = 0; j < Board.Instance.columnNum; j++)
                    global::CreateZombie.Instance.SetZombie(r - 1, id is -1? GameAPP.resourcesManager.allZombieTypes.GetRandom():(ZombieType)id, -5f + j * 1.37f);
                continue;
            }

            if (c > 0 && r > 0 && c <= Board.Instance.columnNum + 1 && r <= Board.Instance.rowNum)
            {
                global::CreateZombie.Instance.SetZombie(r - 1, id is -1? GameAPP.resourcesManager.allZombieTypes.GetRandom():(ZombieType)id, -5f + (c - 1) * 1.37f);
            }
        }
    }

    private static void CreateMindControlledZombie(List<string> args)
    {
        var id = int.Parse(args[0]);
        var r = int.Parse(args[1]);
        var c = int.Parse(args[2]);
        var repeatTimes = int.Parse(args[3]);
        if (repeatTimes > 50) repeatTimes = 50;
        for (var n = 0; n < repeatTimes; n++)
        {
            if (r * r + c * c == 0)
            {
                for (var i = 0; i < Board.Instance.rowNum; i++)
                for (var j = 0; j < Board.Instance.columnNum; j++)
                    global::CreateZombie.Instance.SetZombieWithMindControl(i, id is -1? GameAPP.resourcesManager.allZombieTypes.GetRandom():(ZombieType)id, -5f + j * 1.37f);
                continue;
            }

            if (r == 0 && c != 0)
            {
                for (var j = 0; j < Board.Instance.rowNum; j++)
                    global::CreateZombie.Instance.SetZombieWithMindControl(j, id is -1? GameAPP.resourcesManager.allZombieTypes.GetRandom():(ZombieType)id, -5f + (c - 1) * 1.37f);
                continue;
            }

            if (c == 0 && r != 0)
            {
                for (var j = 0; j < Board.Instance.columnNum; j++)
                    global::CreateZombie.Instance.SetZombieWithMindControl(r - 1, id is -1? GameAPP.resourcesManager.allZombieTypes.GetRandom():(ZombieType)id, -5f + j * 1.37f);
                continue;
            }

            if (c > 0 && r > 0 && c <= Board.Instance.columnNum + 1 && r <= Board.Instance.rowNum)
            {
                global::CreateZombie.Instance.SetZombieWithMindControl(r - 1, id is -1? GameAPP.resourcesManager.allZombieTypes.GetRandom():(ZombieType)id, -5f + (c - 1) * 1.37f);
            }
        }
    }

    private static void CreateZombieVase(List<string> args)
    {
        var id = int.Parse(args[0]);
        var r = int.Parse(args[1]);
        var c = int.Parse(args[2]);
        var PvPPotRange = bool.Parse(args[3]);
        if (PvPPotRange)
        {
            for (var i = 0; i < Board.Instance!.rowNum; i++)
            for (var j = 3; j < Board.Instance.columnNum; j++)
                GridItem.SetGridItem(j, i, GridItemType.ScaryPot).Cast<ScaryPot>().theZombieType =
                    id is -1? GameAPP.resourcesManager.allZombieTypes.GetRandom():(ZombieType)id;
        }
        else
        {
            if (r * r + c * c == 0)
                for (var i = 0; i < Board.Instance!.rowNum; i++)
                for (var j = 0; j < Board.Instance.columnNum; j++)
                    GridItem.SetGridItem(j, i, GridItemType.ScaryPot).Cast<ScaryPot>().theZombieType =
                        id is -1? GameAPP.resourcesManager.allZombieTypes.GetRandom():(ZombieType)id;

            if (r == 0 && c != 0)
                for (var j = 0; j < Board.Instance!.columnNum; j++)
                    GridItem.SetGridItem(c - 1, j, GridItemType.ScaryPot).Cast<ScaryPot>().theZombieType =
                        id is -1? GameAPP.resourcesManager.allZombieTypes.GetRandom():(ZombieType)id;

            if (c == 0 && r != 0)
                for (var j = 0; j < Board.Instance!.columnNum; j++)
                    GridItem.SetGridItem(j, r - 1, GridItemType.ScaryPot).Cast<ScaryPot>().theZombieType =
                        id is -1? GameAPP.resourcesManager.allZombieTypes.GetRandom():(ZombieType)id;

            if (c > 0 && r > 0 && c <= Board.Instance!.columnNum && r <= Board.Instance.rowNum)
                GridItem.SetGridItem(c - 1, r - 1, GridItemType.ScaryPot).Cast<ScaryPot>().theZombieType =
                    id is -1? GameAPP.resourcesManager.allZombieTypes.GetRandom():(ZombieType)id;
        }
    }

    private static void CreateItem(List<string> args)
    {
        var itemType = int.Parse(args[0]);
        if (itemType is <= 8 and >= 0)
        {
            Instantiate(Items[itemType]).transform.SetParent(GameAPP.board.transform);
        }
        else if (itemType >= 64)
        {
            //处理 coin 类的物品
            var newItemType = itemType - 64;
            global::CreateItem.Instance.SetCoin(0, 0, newItemType, 0, Vector3.zero);
        }
    }

    private static void CreatePassiveMeteorite(List<string> _) => Board.Instance.CreatePassiveMateorite();

    private static void CreateActiveMeteorite(List<string> _) => Board.Instance.CreateActiveMateorite();

    private static void CreateUltimateMeteorite(List<string> _) => Board.Instance.CreateUltimateMateorite();

    private static void CreateSolarMeteorite(List<string> _) {
        var original = GameAPP.itemPrefab[47];
        if(original == null||GameAPP.board == null)return;
        var obj = Instantiate(original);
        if(obj == null)return;
        var transform = obj.transform;
        var boardTransform = GameAPP.board.transform;
        transform.SetParent(boardTransform);
    }

    private static void NextWave(List<string> _)
    {
        if (Board.Instance != null && Board.Instance.theMaxWave > 0)
        {
            // 第 0 波时保证进度条可见（与原版表现一致）
            if (Board.Instance.theWave == 0 && InGameUI.Instance != null && InGameUI.Instance.LevProgress != null)
            {
                InGameUI.Instance.LevelName2.gameObject.SetActive(false);
                InGameUI.Instance.LevelName3.gameObject.SetActive(true);
                InGameUI.Instance.LevProgress.SetActive(true);
            }
            // 触发下一波：必须设置成负数，确保进入 `timeUntilNextWave < 0` 分支
            Board.Instance.timeUntilNextWave = -0.01f;
            Board.Instance.NewZombieUpdate();
        }
    }

    private static void SetJumpWave(List<string> args)
    {
        if (Board.Instance != null && args.Count > 0 && int.TryParse(args[0], out var wave))
        {
            if (Board.Instance.theWave == 0 && InGameUI.Instance != null && InGameUI.Instance.LevProgress != null)
            {
                InGameUI.Instance.LevelName2.gameObject.SetActive(false);
                InGameUI.Instance.LevelName3.gameObject.SetActive(true);
                InGameUI.Instance.LevProgress.SetActive(true);
            }
            Board.Instance.theWave = Math.Min(wave, Board.Instance.theMaxWave);
        }
    }

    private static void SetAward(List<string> _) => Lawnf.SetAward(Board.Instance, Vector2.zero);

    private static void DestroyAward(List<string> _)
    {
        var prizes = FindObjectsOfType<PrizeMgr>();
        foreach (var prize in prizes)
        {
            try
            {
                Destroy(prize.gameObject);
            }
            catch
            {
            }
        }
    }

    private static void ShowText(List<string> args) => InGameText.Instance.ShowText(args[0], 5);

    private static void StartMower(List<string> _)
    {
        foreach (var mower in Board.Instance.mowerArray)
        {
            try
            {
                if (mower != null) mower.StartMove();
            }
            catch
            {
            }
        }
    }

    private static void CreateMower(List<string> _)
    {
        // 先清除现有的小推车，避免重复生成导致的问题
        if (Board.Instance != null && Board.Instance.mowerArray != null)
        {
            for (var i = Board.Instance.mowerArray.Count - 1; i >= 0; i--)
            {
                try
                {
                    var mower = Board.Instance.mowerArray[i];
                    if (mower != null)
                    {
                        Destroy(mower.gameObject);
                    }
                }
                catch
                {
                }
            }

            Board.Instance.mowerArray.Clear();
        }

        // 重新生成小推车
        var initBoard = GameAPP.board.GetComponent<InitBoard>();
        if (initBoard != null) initBoard.InitMower();
    }

    private static void GetZombiesList(List<string> _)
    {
        if (InGame)
        {
            try
            {
                var zombieListData = GetZombieListData();
                var currentWave = Board.Instance != null ? Board.Instance.theWave : -1;

                if (zombieListData != null)
                {
                    ModCore.Instance.SendCommand(new SyncData
                    {
                        Command = Strings.GetZombiesList,
                        Parameters =
                        [
                            StringCompressor.Compress(JsonSerializer.Serialize(new ZombiesListData
                                { CurrentWave = currentWave, ZombiesList = zombieListData }))
                        ]
                    });
                }
                else
                {
                    ModCore.Instance.Log.LogWarning("获取出怪列表失败，返回空数据");
                    ModCore.Instance.SendCommand(new SyncData
                    {
                        Command = Strings.GetZombiesList,
                        Parameters =
                        [
                            StringCompressor.Compress(JsonSerializer.Serialize(new ZombiesListData
                                { CurrentWave = currentWave, ZombiesList = [] }))
                        ]
                    });
                }
            }
            catch (Exception ex)
            {
                ModCore.Instance.Log.LogError($"获取出怪列表异常: {ex.Message}\n{ex.StackTrace}");
            }
        }
        else
        {
            ModCore.Instance.SendCommand(new SyncData
            {
                Command = Strings.GetZombiesList,
                Parameters =
                [
                    StringCompressor.Compress(JsonSerializer.Serialize(new ZombiesListData
                        { CurrentWave = -1, ZombiesList = [] }))
                ]
            });
        }
    }

    private static void ChangeZombiesList(List<string> args)
    {
        if (args.Count is 3)
        {
            SetZombieList(int.Parse(args[1]), int.Parse(args[0]), (ZombieType)int.Parse(args[2]));
        }
    }

    #endregion

    #region 布阵器

    private static void GetPlantFormationCode(List<string> args)
    {
        var GaoShuMode = bool.Parse(args[0]);
        if (GaoShuMode)
        {
            //from Gaoshu
            List<string> lineupData = [];
            var allPlants = Lawnf.GetAllPlants();
            if (allPlants != null)
            {
                foreach (var plant in allPlants)
                {
                    // 格式为 "行,列,类型"
                    if (plant == null) continue;
                    var plantData = $"{plant.thePlantColumn},{plant.thePlantRow},{(int)plant.thePlantType}";
                    lineupData.Add(plantData);
                }
            }

            var lineupCode = CompressString(string.Join(";", lineupData));
            ModCore.Instance.SendCommand(new SyncData()
            {
                Command = Strings.GetPlantFormationCode,
                Parameters = [lineupCode]
            });
        }
        else
        {
            List<PlantInfo> bases = [];
            List<PlantInfo> plants = [];
            var allPlants = Lawnf.GetAllPlants();
            if (allPlants != null)
            {
                foreach (var plant in allPlants)
                {
                    if (plant is null) continue;
                    if (plant.plantTag.potPlant || plant.isLily)
                    {
                        bases.Add(new PlantInfo
                        {
                            ID = (int)plant.thePlantType,
                            Row = plant.thePlantRow,
                            Column = plant.thePlantColumn,
                            LilyType = (int)plant.theLilyType
                        });
                        continue;
                    }

                    plants.Add(new PlantInfo
                    {
                        ID = (int)plant.thePlantType,
                        Row = plant.thePlantRow,
                        Column = plant.thePlantColumn,
                        LilyType = (int)plant.theLilyType
                    });
                }
            }

            bases.AddRange(plants);
            ModCore.Instance.SendCommand(new SyncData()
            {
                Command = Strings.GetPlantFormationCode,
                Parameters = [JsonSerializer.Serialize(bases)]
            });
        }
    }

    private static void ApplyPlantFormation(List<string> args)
    {
        var formationCode = args[0];
        var ClearOnWritingField = bool.Parse(args[1]);
        var GaoShuMode = bool.Parse(args[2]);

        if (ClearOnWritingField)
        {
            var allPlants = Lawnf.GetAllPlants();
            if (allPlants != null)
            {
                for (var i = allPlants.Count - 1; i >= 0; i--)
                    allPlants[i]?.Die();
            }
        }

        if (GaoShuMode)
        {
            //from Gaoshu
            var lineupCode = DecompressString(formationCode);
            var plantEntries = lineupCode.Split(';');
            foreach (var entry in plantEntries)
            {
                var plantData = entry.Split(',');
                if (plantData.Length == 3)
                    if (int.TryParse(plantData[0], out var column) &&
                        int.TryParse(plantData[1], out var row) &&
                        int.TryParse(plantData[2], out var plantType))
                        global::CreatePlant.Instance.SetPlant(column, row, (PlantType)plantType);
            }
        }
        else
            try
            {
                var plants = JsonSerializer.Deserialize<List<PlantInfo>>(formationCode);
                if (plants != null)
                {
                    foreach (var plant in plants)
                    {
                        var pl = global::CreatePlant.Instance.SetPlant(plant.Column, plant.Row,
                            (PlantType)plant.ID);
                        if (pl is null) continue;
                        if (pl.GetComponent<Plant>().isLily)
                            pl.GetComponent<Plant>().theLilyType = (PlantType)plant.LilyType;
                    }
                }
            }
            catch (JsonException)
            {
            }
            catch (NotSupportedException)
            {
            }
    }

    private static void GetZombieFormationCode(List<string> args)
    {
        var GaoShuMode = bool.Parse(args[0]);
        if (GaoShuMode)
        {
            // 高数模式（原生字符串格式 + 压缩）
            List<string> zombieDataList = [];
            foreach (var zombie in Board.Instance.zombieArray!)
                if (zombie != null && zombie.gameObject != null && !zombie.isMindControlled)
                {
                    // 使用原始坐标精度（保持 7.8724456 的完整精度）
                    var zombieData =
                        $"{zombie.theZombieRow},{zombie.gameObject.transform.position.x},{(int)zombie.theZombieType}";
                    zombieDataList.Add(zombieData);
                }

            // 先拼接后压缩（添加GZIP+Base64处理）
            var zombieCode = string.Join(";", zombieDataList);
            var compressedCode = CompressString(zombieCode); // GZIP压缩 + Base64编码

            ModCore.Instance.SendCommand(new SyncData()
            {
                Command = Strings.GetZombieFormationCode,
                Parameters = [compressedCode]
            });
        }
        else
        {
            // JSON 模式（保留完整对象结构）
            List<ZombieInfo> zombies = [];
            foreach (var zombie in Board.Instance.zombieArray!)
                if (zombie != null && zombie.gameObject != null && !zombie.isMindControlled)
                    zombies.Add(new ZombieInfo
                    {
                        ID = (int)zombie.theZombieType,
                        X = zombie.gameObject.transform.position.x,
                        Row = zombie.theZombieRow
                    });
            ModCore.Instance.SendCommand(new SyncData()
            {
                Command = Strings.GetZombieFormationCode,
                Parameters = [JsonSerializer.Serialize(zombies)]
            });
        }
    }

    private static void ApplyZombieFormation(List<string> args)
    {
        var formationCode = args[0];
        var ClearOnWritingZombies = bool.Parse(args[1]);
        var GaoShuMode = bool.Parse(args[2]);

        if (ClearOnWritingZombies)
        {
            Il2CppReferenceArray<Object> zombies =
                FindObjectsOfTypeAll(Il2CppType.Of<Zombie>());
            for (var i = zombies.Count - 1; i >= 0; i--)
                try
                {
                    ((Zombie)zombies[i])?.Die();
                }
                catch
                {
                }

            Board.Instance.zombieArray!.Clear();
        }

        if (GaoShuMode)
        {
            // 先解压后解析（添加GZIP+Base64处理）
            var zombieCode = DecompressString(formationCode); // Base64解码 + GZIP解压
            var zombieEntries = zombieCode.Split(';');

            foreach (var entry in zombieEntries)
            {
                var zombieData = entry.Split(',');
                if (zombieData.Length == 3)
                    if (int.TryParse(zombieData[0], out var row) &&
                        float.TryParse(zombieData[1], out var x) &&
                        int.TryParse(zombieData[2], out var zombieType))
                        global::CreateZombie.Instance.SetZombie(row, (ZombieType)zombieType, x);
            }
        }
        else
            try
            {
                var fieldZombies = JsonSerializer.Deserialize<List<ZombieInfo>>(formationCode);
                if (fieldZombies != null)
                {
                    foreach (var z in fieldZombies)
                        global::CreateZombie.Instance.SetZombie(z.Row, (ZombieType)z.ID, z.X);
                }
            }
            catch (JsonException)
            {
            }
            catch (NotSupportedException)
            {
            }
    }

    private static void GetMixedFormationCode(List<string> args)
    {
        List<string> zombieDataList = [];
        foreach (var zombie in Board.Instance.zombieArray!)
            if (zombie != null && zombie.gameObject != null && !zombie.isMindControlled)
            {
                // 使用原始坐标精度（保持 7.8724456 的完整精度）
                var zombieData =
                    $"{zombie.theZombieRow},{zombie.gameObject.transform.position.x},{(int)zombie.theZombieType}";
                zombieDataList.Add(zombieData);
            }

        // 先拼接后压缩（添加GZIP+Base64处理）
        var zombieCode = string.Join(";", zombieDataList);
        var zombieString = CompressString(zombieCode); // GZIP压缩 + Base64编码

        //from Gaoshu
        List<string> lineupData = [];
        var allPlants = Lawnf.GetAllPlants();
        if (allPlants != null)
        {
            foreach (var plant in allPlants)
            {
                // 格式为 "行,列,类型"
                if (plant == null) continue;
                var plantData = $"{plant.thePlantColumn},{plant.thePlantRow},{(int)plant.thePlantType}";
                lineupData.Add(plantData);
            }
        }

        var plantCode = string.Join(";", lineupData);
        var PlantString = CompressString(plantCode); // GZIP压缩 + Base64编码

        var result = PlantString + "|" + zombieString;

        ModCore.Instance.SendCommand(new SyncData()
        {
            Command = Strings.GetMixedFormationCode,
            Parameters = [result]
        });
    }

    private static void ApplyMixedFormation(List<string> args)
    {
        var formationCode = args[0];
        var ClearOnWriting = bool.Parse(args[1]);

        if (ClearOnWriting)
        {
            var allPlants = Lawnf.GetAllPlants();
            if (allPlants != null)
            {
                for (var i = allPlants.Count - 1; i >= 0; i--)
                    try
                    {
                        allPlants[i]?.Die();
                    }
                    catch
                    {
                    }
            }

            Il2CppReferenceArray<Object> zombies =
                FindObjectsOfTypeAll(Il2CppType.Of<Zombie>());
            for (var i = zombies.Count - 1; i >= 0; i--)
                try
                {
                    ((Zombie)zombies[i])?.Die();
                }
                catch
                {
                }

            Board.Instance.zombieArray!.Clear();
        }

        //from Gaoshu
        var codes = formationCode.Split('|');

        var plantCode = codes[0];
        plantCode = DecompressString(plantCode);
        var plantEntries = plantCode.Split(';');
        foreach (var entry in plantEntries)
        {
            var plantData = entry.Split(',');
            if (plantData.Length == 3)
                if (int.TryParse(plantData[0], out var column) &&
                    int.TryParse(plantData[1], out var row) &&
                    int.TryParse(plantData[2], out var plantType))
                    global::CreatePlant.Instance.SetPlant(column, row, (PlantType)plantType);
        }

        var zombieCode = codes[1];
        zombieCode = DecompressString(zombieCode);
        var zombieEntries = zombieCode.Split(';');

        foreach (var entry in zombieEntries)
        {
            var zombieData = entry.Split(',');
            if (zombieData.Length == 3)
                if (int.TryParse(zombieData[0], out var row) &&
                    float.TryParse(zombieData[1], out var x) &&
                    int.TryParse(zombieData[2], out var zombieType))
                    global::CreateZombie.Instance.SetZombie(row, (ZombieType)zombieType, x);
        }
    }

    private static void GetPotFormationCode(List<string> args)
    {
        List<VaseInfo> vases = [];
        foreach (var vase in Board.Instance.griditemArray)
        {
            if (vase is null ||
                vase.theItemType is GridItemType.ScaryPot
                    or GridItemType.ScaryPot_plant
                    or GridItemType.ScaryPot_zombie
                    or GridItemType.ScaryPot_hypnoZombie
                    or GridItemType.ScaryPot_obsidian
                    or GridItemType.ScaryPot_gold) continue;
            vases.Add(new VaseInfo
            {
                Row = vase.theItemRow,
                Col = vase.theItemColumn,
                PlantType = (int)vase.Cast<ScaryPot>().thePlantType,
                ZombieType = (int)vase.Cast<ScaryPot>().theZombieType
            });
        }

        ModCore.Instance.SendCommand(new SyncData
        {
            Command = Strings.GetPotFormationCode,
            Parameters = [JsonSerializer.Serialize(vases)]
        });
    }

    private static void ApplyPotFormation(List<string> args)
    {
        var formationCode = args[0];
        var ClearOnWritingVases = bool.Parse(args[1]);

        try
        {
            var fieldVases = JsonSerializer.Deserialize<List<VaseInfo>>(formationCode);
            if (fieldVases != null)
            {
                if (ClearOnWritingVases)
                    for (var i = Board.Instance.griditemArray.Count - 1; i >= 0; i--)
                        if (Board.Instance.griditemArray[i] != null &&
                            Board.Instance.griditemArray[i].theItemType is (GridItemType)4
                                or (GridItemType)5 or (GridItemType)6)
                        {
                            Board.Instance.griditemArray[i].gameObject.active = false;
                            Destroy(Board.Instance.griditemArray[i]);
                        }

                foreach (var vase in fieldVases)
                {
                    var g = GridItem.SetGridItem(vase.Col, vase.Row, GridItemType.ScaryPot);
                    g.Cast<ScaryPot>().thePlantType = (PlantType)vase.PlantType;
                    g.Cast<ScaryPot>().theZombieType = (ZombieType)vase.ZombieType;
                }
            }
        }
        catch (JsonException)
        {
            ModCore.Instance.Log.LogError("布阵代码存在错误！");
        }
        catch (NotSupportedException)
        {
            ModCore.Instance.Log.LogError("布阵代码存在错误！");
        }
    }

    private static void ApplyBattleFormation(List<string> args)
    {
        Present1PlantIndex = (PlantType)int.Parse(args[0]);
        Present2PlantIndex = (PlantType)int.Parse(args[1]);
        Present3PlantIndex = (PlantType)int.Parse(args[2]);
        Present4PlantIndex = (PlantType)int.Parse(args[3]);
        Present5PlantIndex = (PlantType)int.Parse(args[4]);
        ZombieSlot1Index = (ZombieType)int.Parse(args[5]);
        ZombieSlot2Index = (ZombieType)int.Parse(args[6]);
        ZombieSlot3Index = (ZombieType)int.Parse(args[7]);
        ZombieSlot4Index = (ZombieType)int.Parse(args[8]);
        ZombieSlot5Index = (ZombieType)int.Parse(args[9]);
        ZombieSlot6Index = (ZombieType)int.Parse(args[10]);

        // 放置5个礼盒在第3行（row=2），第1-5列（column=0-4）
        // 礼盒打开时会通过 PresentPatch 检查 LockPresent1-5 来决定生成什么植物
        global::CreatePlant.Instance.SetPlant(0, 2, PlantType.Present);
        global::CreatePlant.Instance.SetPlant(1, 2, PlantType.Present);
        global::CreatePlant.Instance.SetPlant(2, 2, PlantType.Present);
        global::CreatePlant.Instance.SetPlant(3, 2, PlantType.Present);
        global::CreatePlant.Instance.SetPlant(4, 2, PlantType.Present);
        PVEPresentFlag[0] = true;
        PVEPresentFlag[1] = true;
        PVEPresentFlag[2] = true;
        PVEPresentFlag[3] = true;
        PVEPresentFlag[4] = true;

        // 放置僵尸：1个巨人盲盒 + 1个钻石盲盒 + 4个黄金盲盒，并记录实例ID用于死亡时生成指定僵尸
        var zg = global::CreateZombie.Instance.SetZombie(2, ZombieType.RandomGargantuar, 8);
        if (zg != null)
        {
            var comp = zg.GetComponent<Zombie>();
            if (comp != null) PveBlindBoxSlotByInstance[comp.GetInstanceID()] = 6; // 槽6：巨人盲盒
        }

        var zd = global::CreateZombie.Instance.SetZombie(2, ZombieType.DiamondRandomZombie, 8);
        if (zd != null)
        {
            var comp = zd.GetComponent<Zombie>();
            if (comp != null) PveBlindBoxSlotByInstance[comp.GetInstanceID()] = 1; // 槽1：钻石盲盒
        }

        for (int i = 0; i < 4; i++)
        {
            var zp = global::CreateZombie.Instance.SetZombie(2, ZombieType.RandomPlusZombie, 8);
            if (zp != null)
            {
                var comp = zp.GetComponent<Zombie>();
                if (comp != null) PveBlindBoxSlotByInstance[comp.GetInstanceID()] = 2 + i; // 槽2~5：四个黄金盲盒
            }
        }
    }

    #endregion

    #region 旅行词条

    private static void UpdateAdvBuff(List<string> args)
    {
        var id = (AdvBuff)int.Parse(args[0]);
        var level = int.Parse(args[1]);
        AdvBuffs[id] = level;
    }

    private static void UpdateUltiBuff(List<string> args)
    {
        var id = (UltiBuff)int.Parse(args[0]);
        var level = int.Parse(args[1]);
        UltiBuffs[id] = level;
    }

    private static void UpdateDebuff(List<string> args)
    {
        var id = (TravelDebuff)int.Parse(args[0]);
        var isEnabled = bool.Parse(args[1]);
        Debuffs[id] = isEnabled;
    }

    private static void UpdateInvestBuff(List<string> args)
    {
        var id = (InvestBuff)int.Parse(args[0]);
        var isEnabled = bool.Parse(args[1]);
        InvestBuffs[id] = isEnabled;
    }

    private static void UpdateInGameAdvBuff(List<string> args)
    {
        var id = (AdvBuff)int.Parse(args[0]);
        var level = int.Parse(args[1]);
        InGameAdvBuffs[id] = level;
        UpdateInGameBuffs();
    }

    private static void UpdateInGameUltiBuff(List<string> args)
    {
        var id = (UltiBuff)int.Parse(args[0]);
        var level = int.Parse(args[1]);
        InGameUltiBuffs[id] = level;
        UpdateInGameBuffs();
    }

    private static void UpdateInGameDebuff(List<string> args)
    {
        var id = (TravelDebuff)int.Parse(args[0]);
        var isEnabled = bool.Parse(args[1]);
        InGameDebuffs[id] = isEnabled;
        UpdateInGameBuffs();
    }

    private static void UpdateInGameInvestBuff(List<string> args)
    {
        var id = (InvestBuff)int.Parse(args[0]);
        var isEnabled = bool.Parse(args[1]);
        InGameInvestBuffs[id] = isEnabled;
        UpdateInGameBuffs();
    }

    private static void UpdateUnlockedPlant(List<string> args)
    {
        var id = (TravelUnlocks)int.Parse(args[0]);
        var isEnabled = bool.Parse(args[1]);
        UnlockedPlants[id] = isEnabled;
    }

    private static void UpdateInGameUnlockedPlant(List<string> args)
    {
        var id = (TravelUnlocks)int.Parse(args[0]);
        var isEnabled = bool.Parse(args[1]);
        InGameUnlockedPlants[id] = isEnabled;
        UpdateInGameBuffs();
    }

    private static void UpdateAllBuffs(List<string> args)
    {
        var buffs = JsonSerializer.Deserialize<SyncTravelBuffs>(args[0]);
        if (buffs.AdvBuffs.Count > 0)
        {
            foreach (var advBuff in buffs.AdvBuffs)
            {
                AdvBuffs[(AdvBuff)advBuff.Key] = advBuff.Value;
            }
        }

        if (buffs.UltiBuffs.Count > 0)
        {
            foreach (var ultiBuff in buffs.UltiBuffs)
            {
                UltiBuffs[(UltiBuff)ultiBuff.Key] = ultiBuff.Value;
            }
        }

        if (buffs.Debuffs.Count > 0)
        {
            foreach (var debuff in buffs.Debuffs)
            {
                Debuffs[(TravelDebuff)debuff.Key] = debuff.Value;
            }
        }

        if (buffs.InvestBuffs.Count > 0)
        {
            foreach (var investBuff in buffs.InvestBuffs)
            {
                InvestBuffs[(InvestBuff)investBuff.Key] = investBuff.Value;
            }
        }
        if (buffs.UnlockedPlants.Count > 0)
        {
            foreach (var unlock in buffs.UnlockedPlants)
            {
                UnlockedPlants[(TravelUnlocks)unlock.Key] = unlock.Value; 
            }
        }
        if (!InGame) return;

        var needRefresh = false;
        if (buffs.InGameAdvBuffs.Count > 0)
        {
            foreach (var advBuff in buffs.InGameAdvBuffs)
            {
                InGameAdvBuffs[(AdvBuff)advBuff.Key] = advBuff.Value;
            }
            needRefresh = true;
        }

        if (buffs.InGameUltiBuffs.Count > 0)
        {
            foreach (var ultiBuff in buffs.InGameUltiBuffs)
            {
                InGameUltiBuffs[(UltiBuff)ultiBuff.Key] = ultiBuff.Value;
            }
            needRefresh = true;
        }

        if (buffs.InGameDebuffs.Count > 0)
        {
            foreach (var debuff in buffs.InGameDebuffs)
            {
                InGameDebuffs[(TravelDebuff)debuff.Key] = debuff.Value;
            }
            needRefresh = true;
        }

        if (buffs.InGameInvestBuffs.Count > 0)
        {
            foreach (var investBuff in buffs.InGameInvestBuffs)
            {
                InGameInvestBuffs[(InvestBuff)investBuff.Key] = investBuff.Value;
            }
            needRefresh = true;
        }
        
        if (buffs.InGameUnlockedPlants.Count > 0)
        {
            foreach (var unlock in buffs.InGameUnlockedPlants)
            {
                InGameUnlockedPlants[(TravelUnlocks)unlock.Key] = unlock.Value;
            }
            needRefresh = true;
        }

        if (needRefresh) UpdateInGameBuffs();
    }

    private static void FlagWaveBuff(List<string> args)
    {
        var buffs = JsonSerializer.Deserialize<FlagWaveBuff>(args[0]);
        FlagWaveBuffs[buffs.Wave - 1] = buffs;
    }

    #endregion

    private static void PlaySound(List<string> args)
    {
        try
        {
            var soundId = int.Parse(args[0]);
            GameAPP.PlaySound(soundId);
        }
        catch (Exception e)
        {
            ModCore.Instance.Log.LogError($"播放音效失败: {e.Message}");
        }
    }

    public static void PlayParticle(List<string> args)
    {
        try
        {
            int particleId = int.Parse(args[0]);
            // 在屏幕中央位置播放特效，使用第3行（中间行）
            Vector3 position = new Vector3(0, 0, 0);
            if (Board.Instance != null)
            {
                // 计算屏幕中央位置
                int centerRow = Board.Instance.rowNum / 2;
                int centerCol = Board.Instance.columnNum / 2;
                position = new Vector3(-5f + centerCol * 1.37f, centerRow * 0.8f, 0);
            }

            CreateParticle.SetParticle(particleId, position, Board.Instance?.rowNum / 2 ?? 2, true);
        }
        catch (Exception ex)
        {
            ModCore.Instance.Log.LogError($"播放特效失败: {ex.Message}");
        }
    }

    public static void GetSnapshot(List<string> args)
    {
        if (IsBoardReadyForSnapshot())
        {
            TryCaptureSnapshot();
        }
        else
        {
            // 延后到关卡初始化完成后再保存（最多约3秒）
            PendingManualSnapshotFrames = Math.Max(PendingManualSnapshotFrames, 180);
            try
            {
                InGameText.Instance?.ShowText("正在初始化，稍后保存快照…", 1.5f);
            }
            catch
            {
            }
        }
    }

    public static void RestoreSnapshot(List<string> args)
    {
        Snapshot target = new();
        if (Snapshots.Count > 0)
        {
            target = Snapshots[^1];
        }
        else
        {
            // 兼容跨重启场景：内存环形缓冲为空时，回退读取磁盘最近快照。
            target = TryLoadLatestSnapshotFromDisk();
            if (target != null)
                Snapshots.Add(target);
        }

        if (target != null)
        {
            Utils.RestoreSnapshot(target);
        }
        else
        {
            try
            {
                InGameText.Instance?.ShowText("未找到可恢复快照", 2.0f);
            }
            catch
            {
            }
        }
    }

    private static void GodEvolutionResetQuality(List<string> _)
    {
        GodEvolutionUnlimitedRefresh = false;
        GodEvolutionFreeUpgradeQuality = false;
        GodEvolutionLucky = -1f;
        GodEvolutionDifficulty = -1;
        GodEvolutionRefreshCount = -1;
        GodEvolutionMaxPlantCount = -1;
        GodEvolutionSuperUpgrade = false;
        GodEvolutionForceSuperQuality = false;
        GodEvolutionUncrashable = false;
        GodEvolutionQualityWeightEnabled = false;
        GodEvolutionQualityDefault = 55f;
        GodEvolutionQualitySilver = 25f;
        GodEvolutionQualityGold = 12f;
        GodEvolutionQualityDiamond = 3f;
        GodEvolutionDamageMultiplier = -1f;
    }

    /// <summary>
    /// 手动实现 RogueShootingData.Record 逻辑（Il2CppInterop 未暴露该泛型方法）
    /// 在列表中查找匹配 element 的记录并 count+1，找不到则新建记录(count=1)并添加
    /// DataRecord 字段偏移: element=0x10, count=0x14
    /// </summary>
    private static unsafe void RecordData(Il2CppSystem.Collections.Generic.List<DataRecord<int>> list, int element)
    {
        if (list == null) return;
        foreach (var record in list)
        {
            if (record == null) continue;
            var ptr = (int*)record.Pointer;
            if (ptr[4] == element) // offset 0x10 = 4 ints
            {
                ptr[5]++; // offset 0x14 = 5 ints
                return;
            }
        }
        var newRecord = new DataRecord<int>();
        int* newPtr = (int*)newRecord.Pointer;
        newPtr[4] = element;
        newPtr[5] = 1;
        list.Add(newRecord);
    }

    private static void GodEvolutionUnlockAll(List<string> _)
    {
        var data = ShootingManager.Data;
        if (data == null) return;

        // 设置总胜利次数≥20，解锁canTab和屋顶模式
        data.victoryTimes = 20;

        // 解锁全部难度（难度2/3/4各需前一级难度至少1次胜利）
        RecordData(data.difficultyWin, 1);
        RecordData(data.difficultyWin, 2);
        RecordData(data.difficultyWin, 3);

        // 解锁全部模式（模式1/2/3各需前一级至少1次胜利，模式4需模式3至少10次胜利）
        RecordData(data.stageWins, 0);
        RecordData(data.stageWins, 1);
        RecordData(data.stageWins, 2);
        // 模式4需要 stageWins[3] ≥ 10
        for (var i = 0; i < 10; i++)
            RecordData(data.stageWins, 3);

        InGameText.Instance?.ShowText("已解锁全部难度与模式", 5);
    }

    private static void GodEvolutionChooseBuff(List<string> _)
    {
        if (InGame && Board.Instance.TryGetComponent<ShootingManager>(out var shooting)&&FindObjectsOfTypeAll(Il2CppType.Of<MultipleChoiceMenu>()).Count is 2)
        {
            shooting.ShowBuff();
        }
    }

    private static void GodEvolutionRemoveStarsStarUp(List<string> _)
    {
        if (InGame && Board.Instance.TryGetComponent<ShootingManager>(out var shooting))
        {
            if (shooting.plantBuffRecords.ContainsKey(PlantType.UltimateStar))
            {
                if (shooting.plantBuffRecords[PlantType.UltimateStar].ContainsKey("超进化：星辉"))
                {
                    shooting.plantBuffRecords[PlantType.UltimateStar].Remove("超进化：星辉");
                }
            }

            foreach (var plant in Lawnf.GetAllPlants())
            {
                if (plant.thePlantType is PlantType.UltimateStar)
                {
                    plant.Die();
                }
            }
        }
    }

    private static void SpawnPetGargantuar(List<string> _)
    {
        if (Mouse.Instance != null)
        {
            var mousePos = Mouse.Instance.transform.position;
            MiniPet.SetPet(Board.Instance, new Vector2(mousePos.x, mousePos.y), PetType.PetGargantuar);
        }
    }

    private static void SpawnPetFootball(List<string> _)
    {
        if (Mouse.Instance != null)
        {
            var mousePos = Mouse.Instance.transform.position;
            MiniPet.SetPet(Board.Instance, new Vector2(mousePos.x, mousePos.y), PetType.PetFootball);
        }
    }

    private static void SpawnPetSnowBoss(List<string> _)
    {
        if (Mouse.Instance != null)
        {
            var mousePos = Mouse.Instance.transform.position;
            MiniPet.SetPet(Board.Instance, new Vector2(mousePos.x, mousePos.y), PetType.PetSnowBoss);
        }
    }

    private static void SpawnPetJackbox(List<string> _)
    {
        if (Mouse.Instance != null)
        {
            var mousePos = Mouse.Instance.transform.position;
            MiniPet.SetPet(Board.Instance, new Vector2(mousePos.x, mousePos.y), PetType.PetJackbox);
        }
    }

    private static void SpawnPetDrown(List<string> _)
    {
        if (Mouse.Instance != null)
        {
            var mousePos = Mouse.Instance.transform.position;
            MiniPet.SetPet(Board.Instance, new Vector2(mousePos.x, mousePos.y), PetType.PetDrown);
        }
    }

    private static void SpawnPetHorse(List<string> _)
    {
        if (Mouse.Instance != null)
        {
            var mousePos = Mouse.Instance.transform.position;
            MiniPet.SetPet(Board.Instance, new Vector2(mousePos.x, mousePos.y), PetType.PetHorse);
        }
    }

    private static void SpawnPetImp(List<string> _)
    {
        if (Mouse.Instance != null)
        {
            var mousePos = Mouse.Instance.transform.position;
            MiniPet.SetPet(Board.Instance, new Vector2(mousePos.x, mousePos.y), PetType.PetImp);
        }
    }

    private static void SpawnPetKirov(List<string> _)
    {
        if (Mouse.Instance != null)
        {
            var mousePos = Mouse.Instance.transform.position;
            MiniPet.SetPet(Board.Instance, new Vector2(mousePos.x, mousePos.y), PetType.PetKirov);
        }
    }

    /// <summary>
    /// 一键应用全部植物皮肤：遍历所有植物类型，将每种植物设置为最后一个可用皮肤（最高阶皮肤）
    /// 使用游戏原生 SetSkin 方法避免直接操作 Il2Cpp 字典导致的运行时类型转换失败
    /// </summary>
    private static void ApplyAllPlantSkins(List<string> _)
    {
        //if (!InGame) return;
        try
        {
            var rm = GameAPP.resourcesManager;
            if (rm?.allPlants == null) return;

            var appliedCount = 0;
            foreach (var plantType in rm.allPlants)
            {
                try
                {
                    // 检查该植物是否有多个皮肤（_plantPrefabs 中对应的 List 长度 > 1）
                    if (rm._plantPrefabs.TryGetValue(plantType, out Il2CppSystem.Collections.Generic.List<GameObject> skinList) && skinList != null)
                    {
                        var count = skinList.Count;
                        if (count > 1)
                        {
                            var lastSkinIndex = count - 1;
                            // 使用游戏原生 SetSkin 方法，避免直接操作 Il2Cpp 词典的索引器
                            rm.SetSkin(plantType, lastSkinIndex);
                            appliedCount++;
                        }
                    }
                }
                catch
                {
                    // 跳过没有皮肤的植物类型
                }
            }

            InGameText.Instance?.ShowText(appliedCount > 0 ? "已应用全部植物皮肤" : "没有找到可应用的植物皮肤", 2);
        }
        catch (Exception ex)
        {
            ModCore.Instance.Log.LogError($"ApplyAllPlantSkins 异常: {ex.Message}\n{ex.StackTrace}");
        }
    }
    
    private static void ObtainAllPlantSkins(List<string> _)
    {
        try
        {
            GameAPP.skinLevelCompleted.Clear();
            for (var i = 1; i <= 10; i++)
            {
                GameAPP.skinLevelCompleted.Add(i);
            }
            InGameText.Instance?.ShowText("已获得所有植物皮肤", 2);
        }
        catch (Exception ex)
        {
            ModCore.Instance.Log.LogError($"ObtainAllPlantSkins 异常: {ex.Message}\n{ex.StackTrace}");
        }
    }
    
    private static void TreasureFillCard(List<string> _)
    {
        foreach (var card in TreasureData.treasureCards)
        {
            card.durability = 40;
        }
    }

    private static void TreasureSellAllCards(List<string> _)
    {
        var menu = TreasureWarehouseMenu.Instance;
        if (menu!=null)
        {
            foreach (var card in menu.cards)
            {
                card.Sell();
            }
        }
        else
        {
            foreach (var card in TreasureData.treasureCards)
            {
                var plantData = PlantDataManager.GetPlantData(card.thePlantType);
                if (plantData == null)
                    continue;

                // Base cost (clamped to 1000), then scaled by card level
                var cost = plantData.cost;
                if (cost > 1000)
                    cost = 1000;

                CardLevel level = TreasureData.GetCardLevel(card.thePlantType);
                switch ((int)level)
                {
                    case 0:                     break; // ×1
                    case 1:  cost *= 2;         break;
                    case 2:  cost *= 4;         break;
                    case 3:  cost *= 8;         break;
                    case 4:  cost *= 32;        break;
                    case 5:  cost <<= 7;        break; // ×128
                    default: return;                   // unknown level
                }
                
                int refund;
                if (card.maxDurability <= 1)
                {
                    refund = 0;
                }
                else
                {
                    refund = Mathf.RoundToInt(card.durability * cost / (float)card.maxDurability);
                }

                // Refund 80 % of the proportional cost
                TreasureData.treasureMoney += Mathf.RoundToInt(refund * 0.8f);
            }
            TreasureData.treasureCards.Clear();
        }
    }
    

    private static void ZenGardenGetPlant(List<string> args)
    {
        var plantType = (PlantType)int.Parse(args[0]);
        var data = GardenUI.Data;
        if (data?.allPlants == null) return;

        // Replicate TryAddPlantData's position-finding logic:
        // scan all 64 pages, 8 columns × 4 rows, find first page with an empty slot
        int targetPage;
        var available = new System.Collections.Generic.List<Vector2Int>();
        for (targetPage = 0; targetPage < 64; targetPage++)
        {
            for (int col = 0; col < 8; col++)
            {
                for (int row = 0; row < 4; row++)
                {
                    var occupied = false;
                    for (int i = 0; i < data.allPlants.Count; i++)
                    {
                        var p = data.allPlants[i];
                        if (p.page == targetPage && p.thePlantColumn == col && p.thePlantRow == row)
                        {
                            occupied = true;
                            break;
                        }
                    }

                    if (!occupied)
                        available.Add(new Vector2Int(col, row));
                }
            }

            if (available.Count > 0)
                break;
        }

        if (available.Count == 0) return;

        var index =UnityEngine. Random.Range(0, available.Count);
        var pos = available[index];

        if (GardenUI.Instance != null)
            data.CreatePlantObject(plantType, pos.x, pos.y, targetPage, GardenUI.Instance);
        else
            data.CreatePlantData(plantType, pos.x, pos.y, targetPage);
        GardenUI.Data.Save();
    }

    private static void AbyssJumpLevel(List<string> args)
    {
        /*
        var level=int.Parse(args[0]);
        if (AbyssManager.Instance != null)
        {
            //AbyssManager.Instance.abyssData.arrivedLevel=level;
            //AbyssManager.Instance.abyssData.maxArrivedLevel=level;
            //AbyssManager.Instance.abyssData.tempAbyssData.arrivedLevel=level;
        }*/
    }

    private static void AbyssMoney(List<string> args)
    {
        var money=int.Parse(args[0]);
        if (AbyssManager.Instance != null)
        {
            //AbyssManager.Instance.abyssData.money=money;
        }
    }
}