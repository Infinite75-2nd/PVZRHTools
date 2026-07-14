using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using Core;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Unity.VisualScripting;
using UnityEngine;
using static ToolMod.Components.PatchDataCache;
using static ToolMod.Utils;
using Object = UnityEngine.Object;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(Board))]
public static class BoardPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Board.Awake))]
    public static void PostAwake(Board __instance)
    {
        OriginalBoardTag = __instance.boardTag.CloneViaFakeSerialization();
        var t = __instance.boardTag;
        t.isColumn |= ColumnPlanting;
        t.isSeedRain |= SeedRain;
        t.enableTravelPlant |= RemoveFusionLimit;
        t.enableAllTravelPlant |= RemoveFusionLimit;
        __instance.boardTag = t;
        NewBoard = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Board.NewZombieUpdate))]
    public static void PostNewZombieUpdate()
    {
        try
        {
            if (NewZombieUpdateCD > 0f && NewZombieUpdateCD <= 30f && Board.Instance != null)
            {
                // 确保waveInterval不超过设置的最大值
                if (Board.Instance.config != null && Board.Instance.config.waveInterval > NewZombieUpdateCD)
                {
                    Board.Instance.config.waveInterval = NewZombieUpdateCD;
                }
            }
        }
        catch
        {
        }
    }

    /// <summary>
    /// 修改 GetSun 方法 - 移除阳光上限限制
    /// 3.6：Board.GetSun 签名为 (float count, bool save = true)
    /// </summary>
    [HarmonyPatch(nameof(Board.GetSun))]
    [HarmonyPrefix]
    public static bool PreGetSun(Board __instance, float count, bool save)
    {
        if (!UnlimitedSunlight) return true;

        try
        {
            if (__instance != null)
            {
                // 3.6：不再依赖旧版 GetSun 的中间参数，直接按 count 累加即可避免上限裁剪。
                __instance.theSun += (int)count;
            }

            return false;
        }
        catch
        {
            return true;
        }
    }

    /// <summary>
    /// 修改UseSun方法 - 确保使用阳光时不受上限限制
    /// </summary>
    [HarmonyPatch(nameof(Board.UseSun))]
    [HarmonyPrefix]
    public static bool PreUseSun(Board __instance, float count)
    {
        if (!UnlimitedSunlight) return true;

        try
        {
            if (__instance != null)
            {
                int countInt = (int)count; // 3.3.1版本UseSun参数类型为float，需要转换为int
                __instance.theSun -= countInt;
            }

            return false;
        }
        catch
        {
            return true;
        }
    }


    [HarmonyPostfix]
    [HarmonyPatch(nameof(Board.Update))]
    public static void PostUpdate(Board __instance)
    {
        try
        {
            // 处理无限积分（使用新的独立开关或旧的兼容开关）
            if (UnlimitedScore && __instance != null)
            {
                __instance.thePoints = int.MaxValue;
            }

            // 处理诅咒免疫
            if (CurseImmunity)
            {
                CurseClearTimer += Time.deltaTime;
                if (CurseClearTimer >= CurseClearInterval)
                {
                    CurseClearTimer = 0f;
                    ClearAllPlantsCurseVisual();
                }
            }

            // 处理踩踏免疫 - 通过设置 canBeCrashed 属性
            if (TrampleImmunity)
            {
                TrampleImmunityTimer += Time.deltaTime;
                if (TrampleImmunityTimer >= TrampleImmunityInterval)
                {
                    TrampleImmunityTimer = 0f;
                    SetAllPlantsCanBeCrashed(false);
                }
            }

            // 处理两波间最大刷怪CD - 持续设置waveInterval，防止被游戏重置
            if (NewZombieUpdateCD > 0f && NewZombieUpdateCD <= 30f && __instance != null)
            {
                // 确保waveInterval不超过设置的最大值
                if (__instance.config != null && __instance.config.waveInterval > NewZombieUpdateCD)
                {
                    __instance.config.waveInterval = NewZombieUpdateCD;
                }
            }

            // 旗帜波词条功能 - 检测旗帜波并应用词条

            try
            {
                if (!FlagWaveBuffsEnabled || __instance == null || !InGame)
                    return;

                // 检测旗帜波状态变化（从非旗帜波变为旗帜波）
                bool currentHugeWave = __instance.theWave % 10 is 0;
                bool wasHugeWave = LastHugeWaveState;
                LastHugeWaveState = currentHugeWave;

                // 只在进入旗帜波时应用词条（避免重复应用）
                if (currentHugeWave && !wasHugeWave)
                {
                    UnlockNextFlagWaveBuff();
                }
            }
            catch (System.Exception ex)
            {
                ModCore.Instance.Log?.LogError($"旗帜波词条检测失败: {ex.Message}\n{ex.StackTrace}");
            }
        }
        catch
        {
        }
    }

    private static void ClearAllPlantsCurseVisual()
    {
        try
        {
            if (Board.Instance == null) return;

            var allPlants = Lawnf.GetAllPlants();
            if (allPlants == null) return;

            foreach (var plant in allPlants)
            {
                if (plant != null && plant.thePlantHealth > 0)
                {
                    ClearPlantCurseVisual(plant);
                }
            }
        }
        catch
        {
        }
    }

    private static void ClearPlantCurseVisual(Plant plant)
    {
        try
        {
            if (plant == null || plant.gameObject == null) return;

            var spriteRenderers = plant.GetComponentsInChildren<SpriteRenderer>();
            if (spriteRenderers != null)
            {
                foreach (var sr in spriteRenderers)
                {
                    if (sr != null)
                    {
                        // 重置颜色到白色（正常状态）
                        sr.color = Color.white;
                    }
                }
            }
        }
        catch
        {
        }
    }

    /// <summary>
    /// 设置所有植物的 canBeCrashed 属性
    /// 参考 SuperMachinePotComponent.cs 的实现
    /// </summary>
    private static void SetAllPlantsCanBeCrashed(bool value)
    {
        try
        {
            if (Board.Instance == null) return;

            var allPlants = Lawnf.GetAllPlants();
            if (allPlants == null) return;

            foreach (var plant in allPlants)
            {
                if (plant != null && plant.thePlantHealth > 0)
                {
                    try
                    {
                        var plantType = plant.GetType();
                        var crashedProp = plantType.GetProperty("canBeCrashed");

                        if (crashedProp != null && crashedProp.CanWrite)
                            crashedProp.SetValue(plant, value);
                    }
                    catch
                    {
                    }
                }
            }
        }
        catch
        {
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Board.SetEvePlants))]
    public static bool PreSetEvePlants(Board __instance, ref int theColumn, ref int theRow, ref bool fromWheat,
        ref Plant __result)
    {
        if (fromWheat && LockWheat >= 0)
        {
            Plant plantObject = CreatePlant.Instance.SetPlant(
                theColumn,
                theRow,
                (PlantType)LockWheat
            );

            if (plantObject != null)
            {
                plantObject.wheatType = 1;
            }

            if (plantObject == null)
            {
                float boxX = Mouse.Instance.GetBoxXFromColumn(theColumn);
                float landY = Mouse.Instance.GetLandY(boxX, theRow);
                Lawnf.SetDroppedCard(new Vector2(boxX, landY), (PlantType)LockWheat);
            }
            else
            {
                __result = plantObject;
            }

            return false;
        }

        return true;
    }


    [HarmonyPostfix]
    [HarmonyPatch(nameof(Board.Start))]
    public static void PostStart()
    {
        // 清除复制的卡片列表
        CopiedCards.Clear();
        // 应用初始词条（仅在游戏开始时应用一次）
        try
        {
            // 先检查是否真正“配置了”任何初始词条：
            // - 若所有数组都为 null / 长度为 0 / 全是 false，则认为没有设置初始词条，直接跳过，
            //   避免在场景切换时用一堆 false 覆盖当前已有的词条状态。
            bool hasAnyInitialAdv = AdvBuffs is { Count: > 0 } && AdvBuffs.Values.Any(v => v > 0);
            bool hasAnyInitialUlti = UltiBuffs is { Count: > 0 } && UltiBuffs.Values.Any(v => v > 0);
            bool hasAnyInitialInvest = InvestBuffs is { Count: > 0 } && InvestBuffs.Values.Any(v => v);
            bool hasAnyInitialDebuff = Debuffs is { Count: > 0 } && Debuffs.Values.Any(v => v);
            bool hasAnyInitialUnlockedPlant = UnlockedPlants is { Count: > 0 } && UnlockedPlants.Values.Any(v => v);
            
            // 如果所有初始词条都"完全为空"，则认为玩家没有配置初始词条，什么都不做。
            if (!hasAnyInitialAdv && !hasAnyInitialUlti && !hasAnyInitialInvest && !hasAnyInitialDebuff && !hasAnyInitialUnlockedPlant)
            {
                return;
            }


            // 应用初始词条到当前游戏状态

            if (TravelDictionary.advancedBuffsText != null)
            {
                foreach (var kvp in TravelDictionary.advancedBuffsText)
                {
                    InGameAdvBuffs.TryAdd(kvp.Key, 0);
                    AdvBuffs.TryAdd(kvp.Key, 0);
                    InGameAdvBuffs[kvp.Key] = Math.Max(InGameAdvBuffs[kvp.Key], AdvBuffs[kvp.Key]);
                }
            }

            if (TravelDictionary.ultimateBuffsText != null)
            {
                foreach (var kvp in TravelDictionary.ultimateBuffsText)
                {
                    InGameUltiBuffs.TryAdd(kvp.Key, 0);
                    UltiBuffs.TryAdd(kvp.Key, 0);
                    InGameUltiBuffs[kvp.Key] = Math.Max(InGameUltiBuffs[kvp.Key], UltiBuffs[kvp.Key]);
                }
            }

            var keys = InvestBuffs.Keys;

            foreach (var key in keys)
            {
                InGameInvestBuffs.TryAdd(key, false);
                InvestBuffs.TryAdd(key, false);
                InGameInvestBuffs[key] = InGameInvestBuffs[key] || InvestBuffs[key];
            }

            if (TravelDictionary.debuffData != null)
            {
                foreach (var kvp in TravelDictionary.debuffData)
                {
                    InGameDebuffs.TryAdd(kvp.Key, false);
                    Debuffs.TryAdd(kvp.Key, false);
                    InGameDebuffs[kvp.Key] = InGameDebuffs[kvp.Key] || Debuffs[kvp.Key];
                }
            }

            // 应用初始解锁植物到游戏内状态
            if (TravelDictionary.unlocksText != null)
            {
                foreach (var kvp in TravelDictionary.unlocksText)
                {
                    InGameUnlockedPlants.TryAdd(kvp.Key, false);
                    UnlockedPlants.TryAdd(kvp.Key, false);
                    InGameUnlockedPlants[kvp.Key] = InGameUnlockedPlants[kvp.Key] || UnlockedPlants[kvp.Key];
                }
            }

            // 应用词条到游戏（初始词条不允许移除已有词条）
            bool oldAllow = AllowBuffRemoval;
            try
            {
                AllowBuffRemoval = false;
                UpdateInGameBuffs();
            }
            finally
            {
                AllowBuffRemoval = oldAllow;
            } 
            
        }
        catch (Exception ex)
        {
            ModCore.Instance.Log.LogError($"Board.Start: 应用初始词条失败: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// 禁用游戏内置的 WASD 操控植物功能（当随机升级模式开启时）
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Board.ControledPlantUpdate))]
    public static bool PreControledPlantUpdate()
    {
        // 当随机升级模式开启时，禁用游戏内置的 WASD 操控
        if (RandomUpgradeMode)
        {
            return false; // 跳过原方法
        }

        return true; // 执行原方法
    }

    /// <summary>
    /// 旗帜波词条：按顺序每次解锁一个旗帜波的所有词条（永久保持到本局结束）
    /// </summary>
    private static void UnlockNextFlagWaveBuff()
    {
        try
        {
            // 防重复解锁：检查当前波数是否已经解锁过
            int currentWave = Board.Instance != null ? (Board.Instance.theWave + 1) / 10 : -1;
            if (currentWave < 0)
            {
                // 同一波已经解锁过，跳过
                return;
            }

            var travelMgr = ResolveTravelMgr(autoCreate: true);
            if (travelMgr == null)
            {
                ModCore.Instance.Log?.LogWarning("无法找到 TravelMgr，无法应用旗帜波词条");
                return;
            }

            // 记录当前波数，防止重复解锁
            LastUnlockWave = currentWave;
            var buffs = FlagWaveBuffs[currentWave - 1];

            if (InGame)
            {
                foreach (var adv in buffs.AdvBuffs)
                {
                    travelMgr?.GetNormalBuff((AdvBuff)adv);
                }

                foreach (var ulti in buffs.UltiBuffs)
                {
                    travelMgr?.GetUltiBuff((UltiBuff)ulti);
                }

                foreach (var debuff in buffs.Debuffs)
                {
                    travelMgr?.GetDebuff((TravelDebuff)debuff);
                }

                foreach (var invest in buffs.InvestBuffs)
                {
                    travelMgr?.GetInvestBuff((InvestBuff)invest);
                }

                try
                {
                    if (InGameText.Instance != null)
                    {
                        string displayText = buffs.Description;
                        if (displayText.IsNullOrWhiteSpace())
                        {
                            var fullNames = new List<string>();
                            fullNames.AddRange(
                                buffs.AdvBuffs.Select(i => TravelDictionary.advancedBuffsText[(AdvBuff)i]));
                            fullNames.AddRange(buffs.UltiBuffs.Select(i =>
                                TravelDictionary.ultimateBuffsText[(UltiBuff)i]));
                            fullNames.AddRange(buffs.Debuffs.Select(i =>
                                TravelDictionary.debuffData[(TravelDebuff)i].Item1));
                            fullNames.AddRange(buffs.InvestBuffs.Select(GetInvestBuffChineseName));
                            var buffNames = fullNames.Select(ExtractBuffName)
                                .Where(buffName => !string.IsNullOrEmpty(buffName)).ToList();

                            if (buffNames.Count > 0)
                            {
                                displayText = string.Join("、", buffNames);
                            }
                        }

                        if (!string.IsNullOrEmpty(displayText))
                        {
                            // 显示旗帜波文本
                            InGameText.Instance.ShowText(displayText, 5);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    ModCore.Instance.Log?.LogWarning($"[PVZRHTools] 显示旗帜波解锁文本失败: {ex}");
                }
            }

            // 增加旗帜波索引（无论是否有词条都要增加）
            CurrentFlagWaveIndex++;
        }
        catch (System.Exception ex)
        {
            ModCore.Instance.Log?.LogError($"[PVZRHTools] 旗帜波词条应用失败: {ex}");
        }
    }

    /// <summary>
    /// 从词条文本中提取词条名字（去除ID前缀和描述）
    /// </summary>
    private static string ExtractBuffName(string? fullText)
    {
        if (string.IsNullOrEmpty(fullText))
            return "";

        // 如果包含 "#数字 " 前缀，去除它
        if (fullText.StartsWith("#"))
        {
            int spaceIndex = fullText.IndexOf(' ');
            if (spaceIndex > 0 && spaceIndex < fullText.Length - 1)
            {
                fullText = fullText.Substring(spaceIndex + 1);
            }
            else if (spaceIndex < 0)
            {
                // 如果没有空格，尝试找到第一个非数字字符
                int firstNonDigit = 0;
                for (int i = 1; i < fullText.Length; i++)
                {
                    if (!char.IsDigit(fullText[i]))
                    {
                        firstNonDigit = i;
                        break;
                    }
                }

                if (firstNonDigit > 0)
                {
                    fullText = fullText.Substring(firstNonDigit);
                }
            }
        }

        // 如果包含 "：" 或 ":" 分隔符，只取前面的部分（词条名字）
        int colonIndex = fullText.IndexOf('：');
        if (colonIndex < 0) colonIndex = fullText.IndexOf(':');
        if (colonIndex > 0)
        {
            fullText = fullText.Substring(0, colonIndex).Trim();
        }

        return fullText.Trim();
    }

    /// <summary>
    /// 从词条文本中提取词条名字和描述（去除ID前缀，保留名字和描述）
    /// 返回格式：词条名字：（词条功能描述）
    /// </summary>
    private static string ExtractBuffNameWithDescription(string? fullText)
    {
        if (string.IsNullOrEmpty(fullText))
            return "";

        // 如果包含 "#数字 " 前缀，去除它
        if (fullText.StartsWith("#"))
        {
            int spaceIndex = fullText.IndexOf(' ');
            if (spaceIndex > 0 && spaceIndex < fullText.Length - 1)
            {
                fullText = fullText.Substring(spaceIndex + 1);
            }
            else if (spaceIndex < 0)
            {
                // 如果没有空格，尝试找到第一个非数字字符
                int firstNonDigit = 0;
                for (int i = 1; i < fullText.Length; i++)
                {
                    if (!char.IsDigit(fullText[i]))
                    {
                        firstNonDigit = i;
                        break;
                    }
                }

                if (firstNonDigit > 0)
                {
                    fullText = fullText.Substring(firstNonDigit);
                }
            }
        }

        // 保留完整的文本（包括名字和描述），只去除ID前缀
        return fullText.Trim();
    }
}