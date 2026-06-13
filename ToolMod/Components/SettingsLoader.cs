using System;
using System.IO;
using System.Text.Json;
using ToolData;
using UnityEngine;

namespace ToolMod.Components;

/// <summary>
/// 用于读取配置文件并同步到PatchDataCache
/// </summary>
public static class SettingsLoader
{
    /// <summary>
    /// 在模组启动时加载配置文件并应用设置
    /// </summary>
    public static void LoadAndApplySettings()
    {
        try
        {
            var settingsPath = Path.Combine(BepInEx.Paths.GameRootPath, ToolData.Paths.SaveSettingsPath);

            if (!File.Exists(settingsPath))
            {
                ModCore.Instance.Log.LogDebug("配置文件不存在，跳过加载");
                return;
            }

            var json = File.ReadAllText(settingsPath);
            var settings = JsonSerializer.Deserialize<SettingsData>(json);

            if (settings == null)
            {
                ModCore.Instance.Log.LogWarning("配置文件解析失败");
                return;
            }

            if (!settings.SaveAllSettings)
            {
                ModCore.Instance.Log.LogDebug("SaveAllSettings为false，跳过加载");
                return;
            }

            ApplySettings(settings);
            ModCore.Instance.Log.LogMessage("已从配置文件加载并应用设置");
        }
        catch (Exception ex)
        {
            ModCore.Instance.Log.LogWarning($"加载配置文件失败: {ex.Message}");
        }
    }

    private static void ApplySettings(SettingsData settings)
    {
        try
        {
            #region 游戏内按键

            PatchDataCache.KeySpeedStop = (KeyCode)settings.KeySpeedStop;
            PatchDataCache.KeyShowGameInfo = (KeyCode)settings.KeyShowGameInfo;
            PatchDataCache.KeyTopMostCardBank = (KeyCode)settings.KeyTopMostCardBank;
            PatchDataCache.KeyRandomCard = (KeyCode)settings.KeyRandomCard;
            PatchDataCache.KeyAlmanacCreatePlant = (KeyCode)settings.KeyAlmanacCreatePlant;
            PatchDataCache.KeyAlmanacCreatePlantVase = (KeyCode)settings.KeyAlmanacCreatePlantVase;
            PatchDataCache.KeyAlmanacCreateZombie = (KeyCode)settings.KeyAlmanacCreateZombie;
            PatchDataCache.KeyAlmanacCreateZombieVase = (KeyCode)settings.KeyAlmanacCreateZombieVase;
            PatchDataCache.KeyAlmanacZombieMindCtrl = (KeyCode)settings.KeyAlmanacZombieMindCtrl;

            #endregion

            #region 全局属性

            PatchDataCache.ColumnPlanting = settings.ColumnPlanting;
            PatchDataCache.SeedRain = settings.SeedRain;
            PatchDataCache.GameSpeedEnabled = settings.GameSpeedEnabled;
            PatchDataCache.GameSpeed = (float)settings.GameSpeed;
            PatchDataCache.GloveNoCD = settings.GloveNoCD;
            PatchDataCache.GloveFullCD = settings.GloveFullCDEnabled ? (float)settings.GloveFullCD : -1f;
            PatchDataCache.HammerNoCD = settings.HammerNoCD;
            PatchDataCache.HammerFullCD = settings.HammerFullCDEnabled ? (float)settings.HammerFullCD : -1f;
            PatchDataCache.WheelNoCD = settings.WheelNoCD;
            PatchDataCache.WheelFullCD = settings.WheelFullCDEnabled ? (float)settings.WheelFullCD : -1f;
            PatchDataCache.FreePlanting = settings.FreePlanting;
            PatchDataCache.CardFreeCD = settings.CardFreeCD;
            PatchDataCache.RemoveFusionLimit = settings.RemoveFusionLimit;
            PatchDataCache.NewZombieUpdateCD = settings.NewZombieUpdateCDEnabled ? (float)settings.NewZombieUpdateCD : -1f;
            PatchDataCache.UnlimitedScore = settings.UnlimitedScore;
            PatchDataCache.UnlimitedRefresh = settings.UnlimitedRefresh;

            #endregion

            #region 游戏内属性

            PatchDataCache.LockSun = settings.LockSun ? settings.Sun : -1;
            PatchDataCache.LockMoney = settings.LockMoney ? settings.Money : -1;
            PatchDataCache.PauseSpawn = settings.PauseSpawn;
            PatchDataCache.NoFail = settings.NoFail;

            #endregion

            #region 礼盒修改

            PatchDataCache.SuperPresent = settings.SuperPresent;
            PatchDataCache.UltimateRandomZombie = settings.UltimateRandomZombie;
            PatchDataCache.PresentFastOpen = settings.PresentFastOpen;
            PatchDataCache.LockPresent = settings.LockPresentEnabled ? settings.LockPresent : -1;

            #endregion

            #region 场地特性

            PatchDataCache.NoIceRoad = settings.NoIceRoad;
            PatchDataCache.NoHole = settings.NoHole;
            PatchDataCache.ItemExistForever = settings.ItemExistForever;
            PatchDataCache.JackboxNotExplode = settings.JackboxNotExplode;
            PatchDataCache.GarlicDay = settings.GarlicDay;
            PatchDataCache.UnlimitedSunlight = settings.UnlimitedSunlight;
            PatchDataCache.UnlockRedCardPlants = settings.UnlockRedCardPlants;
            PatchDataCache.PotSmashingFix = settings.PotSmashingFix;
            PatchDataCache.DisableIceEffect = settings.DisableIceEffect;

            #endregion

            #region 植物特性

            PatchDataCache.FastShooting = settings.FastShooting;
            PatchDataCache.HardPlant = settings.HardPlant;
            PatchDataCache.ImmuneForceDeduct = settings.ImmuneForceDeduct;
            PatchDataCache.CurseImmunity = settings.CurseImmunity;
            PatchDataCache.CrushImmunity = settings.CrushImmunity;
            PatchDataCache.TrampleImmunity = settings.TrampleImmunity;
            PatchDataCache.PickaxeImmunity = settings.PickaxeImmunity;
            PatchDataCache.UndeadBullet = settings.UndeadBullet;
            PatchDataCache.OldObsidianBullet = settings.OldObsidianBullet;
            PatchDataCache.UltimateSuperGatling = settings.UltimateSuperGatling;
            PatchDataCache.HyponoEmperorNoCD = settings.HyponoEmperorNoCD;
            PatchDataCache.MagnetNutUnlimited = settings.MagnetNutUnlimited;
            PatchDataCache.MineNoCD = settings.MineNoCD;
            PatchDataCache.ChomperNoCD = settings.ChomperNoCD;
            PatchDataCache.CobCannonNoCD = settings.CobCannonNoCD;
            PatchDataCache.PlantUpgrade = settings.PlantUpgrade;
            PatchDataCache.SuperStarNoCD = settings.SuperStarNoCD;
            PatchDataCache.LockWheat = settings.LockWheatEnabled ? settings.LockWheat : -1;

            #endregion

            #region 僵尸特性

            PatchDataCache.ZombieDamageLimit = settings.ZombieDamageLimitEnabled ? settings.ZombieDamageLimit : -1;
            PatchDataCache.ZombieSpeedMultiplier =
                settings.ZombieSpeedMultiplierEnabled ? (float)settings.ZombieSpeedMultiplier : 1.0f;
            PatchDataCache.ZombieAttackMultiplier = settings.ZombieAttackMultiplierEnabled
                ? (float)settings.ZombieAttackMultiplier
                : 1.0f;
            PatchDataCache.ZombieBulletReflect =
                settings.ZombieBulletReflectEnabled ? settings.ZombieBulletReflect : -1;
            PatchDataCache.ZombieStatusCoexist = settings.ZombieStatusCoexist;
            PatchDataCache.ZombieImmuneAllDebuffs = settings.ZombieImmuneAllDebuffs;
            PatchDataCache.ZombieImmuneFreeze = settings.ZombieImmuneFreeze;
            PatchDataCache.ZombieImmuneCold = settings.ZombieImmuneCold;
            PatchDataCache.ZombieImmuneButter = settings.ZombieImmuneButter;
            PatchDataCache.ZombieImmunePoison = settings.ZombieImmunePoison;
            PatchDataCache.ZombieImmuneJalaed = settings.ZombieImmuneJalaed;
            PatchDataCache.ZombieImmuneEmbered = settings.ZombieImmuneEmbered;
            PatchDataCache.ZombieImmuneKnockback = settings.ZombieImmuneKnockback;
            PatchDataCache.ZombieImmuneMindControl = settings.ZombieImmuneMindControl;
            PatchDataCache.ZombieImmuneDevour = settings.ZombieImmuneDevour;

            #endregion

            #region 其他特性

            PatchDataCache.AutoCutFruit = settings.AutoCutFruit;
            PatchDataCache.RandomCard = settings.RandomCard;
            PatchDataCache.ColumnGlove = settings.ColumnGlove;
            PatchDataCache.UnlimitedCardSlots = settings.UnlimitedCardSlots;
            PatchDataCache.RandomBullet = settings.RandomBullet;
            PatchDataCache.AutoRhythmGame = settings.AutoRhythmGame;
            PatchDataCache.StarUpBuff = settings.StarUpBuff;

            #endregion

            #region 布阵器

            PatchDataCache.KillUpgrade = settings.KillUpgrade;
            PatchDataCache.RandomUpgradeMode = settings.RandomUpgradeMode;
            PatchDataCache.Present1PlantIndex = (PlantType)settings.GiftBox1PlantIndex;
            PatchDataCache.Present2PlantIndex = (PlantType)settings.GiftBox2PlantIndex;
            PatchDataCache.Present3PlantIndex = (PlantType)settings.GiftBox3PlantIndex;
            PatchDataCache.Present4PlantIndex = (PlantType)settings.GiftBox4PlantIndex;
            PatchDataCache.Present5PlantIndex = (PlantType)settings.GiftBox5PlantIndex;
            PatchDataCache.ZombieSlot1Index = (ZombieType)settings.ZombieSlot1Index;
            PatchDataCache.ZombieSlot2Index = (ZombieType)settings.ZombieSlot2Index;
            PatchDataCache.ZombieSlot3Index = (ZombieType)settings.ZombieSlot3Index;
            PatchDataCache.ZombieSlot4Index = (ZombieType)settings.ZombieSlot4Index;
            PatchDataCache.ZombieSlot5Index = (ZombieType)settings.ZombieSlot5Index;
            PatchDataCache.ZombieSlot6Index = (ZombieType)settings.ZombieSlot6Index;

            #endregion

            #region 旗帜波词条

            PatchDataCache.FlagWaveBuffsEnabled = settings.FlagWaveBuffsEnabled;

            // 旗帜波词条需要根据保存的ID列表重建
            if (settings.FlagWaveBuffs is { Count: > 0 })
            {
                for (int i = 0; i < Math.Min(settings.FlagWaveBuffs.Count, 10); i++)
                {
                    var fbSettings = settings.FlagWaveBuffs[i];
                    PatchDataCache.FlagWaveBuffs[i] = new FlagWaveBuff
                    {
                        Wave = fbSettings.Wave,
                        AdvBuffs = fbSettings.AdvBuffs,
                        UltiBuffs = fbSettings.UltiBuffs,
                        Debuffs = fbSettings.Debuffs,
                        InvestBuffs = fbSettings.InvestBuffs,
                        Description = fbSettings.Description
                    };
                }
            }

            #endregion

            #region 旅行词条

            foreach (var kvp in settings.TravelAdvBuffs)
                PatchDataCache.AdvBuffs[(AdvBuff)kvp.Key] = kvp.Value;

            foreach (var kvp in settings.TravelUltiBuffs)
                PatchDataCache.UltiBuffs[(UltiBuff)kvp.Key] = kvp.Value;

            foreach (var kvp in settings.TravelDebuffs)
                PatchDataCache.Debuffs[(TravelDebuff)kvp.Key] = kvp.Value;

            foreach (var kvp in settings.TravelInvestBuffs)
                PatchDataCache.InvestBuffs[(InvestBuff)kvp.Key] = kvp.Value;

            #endregion
        }
        catch (Exception ex)
        {
            ModCore.Instance.Log.LogWarning($"应用设置失败: {ex.Message}");
        }
    }
}