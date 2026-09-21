using System.Reflection;
using GameLevel.RogueShooting;
using GameLevel.RogueShooting.CurseBuffs;
using HarmonyLib;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Linq;
using UI;
using UnityEngine;
using UnityEngine.Events;
using static ToolMod.Utils;
using static ToolMod.Components.PatchDataCache;
using Random = UnityEngine.Random;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(ShootingManager))]
public class ShootingManagerPatch
{
    [HarmonyPatch(nameof(ShootingManager.Update))]
    [HarmonyPostfix]
    public static void PostUpdate(ShootingManager __instance)
    {
        if (__instance == null) return;
        try
        {
            if (!float.IsNegativeInfinity(GodEvolutionLucky))
                __instance.Lucky = GodEvolutionLucky;
            if (GodEvolutionDifficulty >= 0)
                __instance.difficulty = GodEvolutionDifficulty;
            if (ShouldFixGodEvolutionRefreshButton)
                __instance.refreshCount = GetGodEvolutionMenuRefreshCount();
            if (GodEvolutionMaxPlantCount >= 0)
                __instance.maxPlantCount = GodEvolutionMaxPlantCount;
            if (GodEvolutionDifficultyPoint != int.MinValue)
                __instance.debuffPoint = GodEvolutionDifficultyPoint;
            if (GodEvolutionNonDiamondCount >= 0)
                __instance.pityThreshold = GodEvolutionNonDiamondCount;
            if (GodEvolutionQualityWeightEnabled)
            {
                __instance.qualityWeights[Quality.Default] = GodEvolutionQualityDefault;
                __instance.qualityWeights[Quality.silver] = GodEvolutionQualitySilver;
                __instance.qualityWeights[Quality.gold] = GodEvolutionQualityGold;
                __instance.qualityWeights[Quality.diamond] = GodEvolutionQualityDiamond;
            }
            else if(__instance.qualityWeights.Equals(OriginalQualityWeights))
            {
                __instance.qualityWeights[Quality.Default] = OriginalQualityWeights[Quality.Default];
                __instance.qualityWeights[Quality.silver] = OriginalQualityWeights[Quality.silver];
                __instance.qualityWeights[Quality.gold] = OriginalQualityWeights[Quality.gold];
                __instance.qualityWeights[Quality.diamond] = OriginalQualityWeights[Quality.diamond];
            }
        }
        catch
        {
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ShootingManager.RegisterOtherBuff))]
    public static void PreRegisterOtherBuff(ShootingManager __instance,ref bool __state)
    {
        __state = __instance.SuperQualitative;
        
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ShootingManager.RegisterExpertBuff))]
    public static bool PreRegisterExpertBuff(ShootingManager __instance, MultipleChoiceMenu menu)
    {
        if (!GodEvolutionForceExpertBuff) return true;
        var candidates = new List<PlantType>();
        foreach (var p in __instance.ExpertPlants)
        {
            if (!__instance.YourPlants.Contains(p)) candidates.Add(p);
        }
        if (candidates.Count <= 0)
            return false;

        candidates.GetRandom();

        menu.RegisterOption(
            "专家邀请",
            "从多个选项中自选一株专家植物",
            (UnityAction)(() => { menu.actionOnExit += (Action)__instance.ShowExpertBuffMenu; }),
            PlantType.EndPumpiner,
            (ZombieType)(-1),
            Quality.diamond,
            true);
        return false;
    }
      
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ShootingManager.RegisterOtherBuff))]
    public static void PostRegisterOtherBuff(ShootingManager __instance,MultipleChoiceMenu menu,ref bool __state)
    {
        
        if (__state == __instance.SuperQualitative && GodEvolutionForceSuperQuality)
        {
            switch (Random.Range(0, 4))
                {
                    case 0:
                        menu.RegisterOption(
                            "超质变：腐化",
                            "获得词条：腐化",
                            (UnityAction)(() => TravelMgr.Instance.GetNormalBuff((AdvBuff)2007)),
                            PlantType.EndoFlame,
                            (ZombieType)(-1),
                            Quality.iridescent,
                            true);
                        break;

                    case 1:
                        menu.RegisterOption(
                            "超质变：步步高升",
                            "所有词条一定是最高品质，且钻石词条的加成x5\n注意：部分植物攻速过快时会丢失动画导致无法攻击或攻速降低",
                            (UnityAction)(() => __instance.superUpgrade = true),
                            PlantType.EndoFlame,
                            (ZombieType)(-1),
                            Quality.iridescent,
                            true);
                        break;

                    case 2:
                        menu.RegisterOption(
                            "超质变：力量会给予希望",
                            string.Concat(
                                "获得词条：力量会给予希望\n获得植物：",
                                Lawnf.GetName(PlantType.UltimateJalaNut),
                                "\n获得植物：",
                                Lawnf.GetName(PlantType.UltimateExplodeCannon),
                                "\n",
                                Lawnf.GetName(PlantType.UltimateExplodeCannon),
                                "获得600%攻击力加成"),
                            (UnityAction)(() =>
                            {
                                TravelMgr.Instance.GetNormalBuff((AdvBuff)3005);
                                __instance.GetNewPlant(PlantType.UltimateJalaNut);
                                __instance.GetNewPlant(PlantType.UltimateExplodeCannon);
                                TravelMgr.Instance.data.AddDamage(PlantType.UltimateJalaNut, 6f);
                                TravelMgr.Instance.data.AddDamage(PlantType.UltimateExplodeCannon, 6f);
                            } ),
                            PlantType.UltimateJalaNut,
                            (ZombieType)(-1),
                            Quality.iridescent,
                            true);
                        break;

                    case 3:
                        menu.RegisterOption(
                            "超质变：神秘大炮",
                            "获得一个神秘大炮",
                            (UnityAction)(() =>
                            {
                                var cannon = Resources.Load<GameObject>("Items/BoardGame/NutShooting/Cannon");
                                UnityEngine.Object.Instantiate(cannon, Board.Instance.transform);
                            }),
                            PlantType.WallNut,
                            (ZombieType)(-1),
                            Quality.iridescent,
                            true);
                        return;
                }
        }
    }

    [HarmonyPatch(nameof(ShootingManager.RegisterCoreBuff))]
    [HarmonyPrefix]
    public static bool PreRegisterCoreBuff(ShootingManager __instance,MultipleChoiceMenu menu)
    {
        if (!GodEvolutionForceMutationBuff) return true;
        var plantUnlocks = __instance.PlantUnlocks;
            foreach (var plantType in __instance.CurrentPlants)
            {
                if (!Config.configs.TryGetValue(plantType, out var config))
                    continue;

                float totalDamage = __instance.board.damageReporter.totalDamage;
                if (totalDamage == 0f)
                    totalDamage = 1f;
                float damageShare = __instance.board.damageReporter.GetDamage(plantType) / totalDamage;
                int buffCount = __instance.GetPlantBuffsCount(plantType);

                foreach (var buff in config.Buffs)
                {
                    if (buff is UpgradeBuff
                        && !plantUnlocks.IsUpgradeUnlocked(plantType, buff.ShowType))
                        continue;

                    int choiceCount = __instance.GetBuffChoiceCount(plantType, buff.Title);
                    if (choiceCount >= buff.MaxCount || !buff.CanAppear)
                        continue;

                    bool isMutation = buff.Title.Contains("质变");

                    // 质变词条：若该植物已有质变记录则不再出现
                    if (isMutation
                        && __instance.plantBuffRecords.TryGetValue(plantType, out var records))
                    {
                        bool hasMutation = false;
                        foreach (var record in records)
                        {
                            if (record.Key.Contains("质变"))
                            {
                                hasMutation = true;
                                break;
                            }
                        }

                        if (hasMutation)
                            continue;
                    }

                    // 非质变词条保留原版的幸运加成出现概率判定，
                    // 避免“质变词条必出”把超进化等稀有词条也变成必出
                    if (!isMutation
                        && buff.AppearWeight < 1f
                        && Random.value > (__instance._lucky * 0.3f + 1f) * buff.AppearWeight)
                        continue;

                    var originalOnGet = (UnityAction)(buff.OnGet);
                    string capturedTitle = buff.Title;
                    PlantType capturedPlant = plantType;

                    string description = choiceCount > 0
                        ? string.Format("{0}\n已选了{1}次", buff.Description, choiceCount)
                        : buff.Description;

                    if (buff is UpgradeBuff)
                    {
                        if (Config.configs.TryGetValue(buff.ShowType, out var targetConfig))
                            description = string.Concat(description, "\n\n定位：", targetConfig.Role);
                    }
                    else if (buff is GeneralBuff)
                    {
                        description += string.Format(
                            "\n\n伤害占比：{0:F2}%\n总词条数：{1}",
                            damageShare * 100f,
                            buffCount);
                    }

                    menu.RegisterOption(
                        buff.Title,
                        description,
                        (UnityAction)(() =>
                        {
                            originalOnGet?.Invoke();
                            __instance.RecordBuffChoice(capturedPlant, capturedTitle);
                        }),
                        buff.ShowType,
                        (ZombieType)(-1),
                        buff.Rarity,
                        true);
                }
            }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ShootingManager.RefisterMissionBuff))]
    public static bool PreRefisterMissionBuff(ShootingManager __instance, MultipleChoiceMenu menu)
    {
        if(!GodEvolutionForceMissionBuff)return true;
        var pool = new List<AdvBuff>();
        pool.Add((AdvBuff)14000);
        pool.Add((AdvBuff)14001);
        pool.Add((AdvBuff)14002);
        pool.Add((AdvBuff)14003);
        
        var candidates = new List<AdvBuff>();
        foreach (var a in pool)
        {
            if ((TravelMgr.AdvBuffData[a].Cast<BaseCurse>()).CanAppear)
                candidates.Add(a);
        }
        if (candidates.Count <= 0)
            return false;

        AdvBuff buff = candidates.GetRandom();
        string text = TravelMgr.AdvBuffData[buff].Description;

        menu.RegisterOption(
            "试炼：" + Core.Lawnf.Before(text, "："),
            Core.Lawnf.After(text, "："),
            (UnityAction)(() => TravelMgr.Instance.GetNormalBuff(buff)),
            PlantType.EndoFlame,
            (ZombieType)(-1),
            Quality.curse,
            true);
        return false;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ShootingManager.GetQualityValue), typeof(float), typeof(Quality))]
    public static void PostGetQualityValueF(ref float __result)
    {
        if (GodEvolutionDamageMultiplier >= 0)
            __result *= GodEvolutionDamageMultiplier;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ShootingManager.GetQualityValue), typeof(int), typeof(Quality))]
    public static void PostGetQualityValueI(ref int __result)
    {
        if (GodEvolutionDamageMultiplier >= 0)
            __result = Mathf.RoundToInt(__result * GodEvolutionDamageMultiplier);
    }

    [HarmonyPatch(nameof(ShootingManager.GetRandomQuality))]
    [HarmonyPostfix]
    public static void PostGetRandomQuality(ref Quality __result)
    {
        if (GodEvolutionForceRandomBuff) __result = Quality.random;
        if (GodEvolutionForceIridescentBuff) __result = Quality.iridescent;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ShootingManager.ShowBuff))]
    public static void PreShowBuff(ShootingManager __instance)
    {
        if (!ShouldFixGodEvolutionRefreshButton || __instance == null) return;
        __instance.refreshCount = GetGodEvolutionMenuRefreshCount();
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ShootingManager.Start))]
    public static void PostStart(ShootingManager __instance)
    {
        OriginalQualityWeights[Quality.Default] = __instance.qualityWeights[Quality.Default];
        OriginalQualityWeights[Quality.silver] = __instance.qualityWeights[Quality.silver];
        OriginalQualityWeights[Quality.gold] = __instance.qualityWeights[Quality.gold];
        OriginalQualityWeights[Quality.diamond] = __instance.qualityWeights[Quality.diamond];
        
        __instance.superUpgrade = GodEvolutionSuperUpgrade;

    }
}