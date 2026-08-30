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
        var displayClass = new ShootingManager.__c__DisplayClass97_0();
        displayClass.__4__this = __instance;
        displayClass.menu = menu;

        // Only offer plants the player doesn't already own
        var candidates = new Il2CppSystem.Collections.Generic. List<PlantType>();
        foreach (var plant in __instance.ExpertPlants)
        {
            if (displayClass._RegisterExpertBuff_b__0(plant))
                candidates.Add(plant);
        }
        if (candidates.Count == 0)
            return false;

        // Trigger a random pick to satisfy the pool (actual selection
        // happens in the ShowExpertBuffMenu sub-menu)
        candidates.GetRandom();

        menu.RegisterOption(
            "专家邀请",
            "从多个选项中自选一株专家植物",
            (UnityAction)(displayClass._RegisterExpertBuff_b__1),
            (PlantType)254,
            (ZombieType)(-1),
            Quality.diamond);
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
                case 0: // 超质变：腐化
                    menu.RegisterOption(
                        "超质变：腐化",
                        "获得词条：腐化",
                        ShootingManager.__c.__9__96_14 ?? (ShootingManager.__c.__9__96_14 = (UnityAction)(ShootingManager.__c.__9._RegisterOtherBuff_b__96_14)),
                        (PlantType)254,
                        (ZombieType)(-1),
                        Quality.iridescent);
                    break;

                case 1: // 超质变：步步高升
                    menu.RegisterOption(
                        "超质变：步步高升",
                        "所有词条一定是最高品质，且钻石词条的加成x5\n注意：部分植物攻速过快时会丢失动画导致无法攻击或攻速降低",
                        (UnityAction)(__instance._RegisterOtherBuff_b__96_15),
                        (PlantType)254,
                        (ZombieType)(-1),
                        Quality.iridescent);
                    break;

                case 2: // 超质变：力量会给予希望
                    string names = string.Concat(
                        "获得词条：力量会给予希望\n获得植物：",
                        Lawnf.GetName((PlantType)969),
                        "\n获得植物：",
                        Lawnf.GetName((PlantType)953),
                        "\n",
                        Lawnf.GetName((PlantType)953),
                        "获得600%攻击力加成");
                    menu.RegisterOption(
                        "超质变：力量会给予希望",
                        names,
                        (UnityAction)(__instance._RegisterOtherBuff_b__96_16),
                        (PlantType)969,
                        (ZombieType)(-1),
                        Quality.iridescent);
                    break;

                case 3: // 超质变：神秘大炮 — NEW
                    menu.RegisterOption(
                        "超质变：神秘大炮",
                        "获得一个神秘大炮",
                        ShootingManager.__c.__9__96_17 ?? (ShootingManager.__c.__9__96_17 = (UnityAction)(ShootingManager.__c.__9._RegisterOtherBuff_b__96_17)),
                        (PlantType)3,
                        (ZombieType)(-1),
                        Quality.iridescent);
                    break;
            }
        }
    }

    [HarmonyPatch(nameof(ShootingManager.RegisterCoreBuff))]
    [HarmonyPrefix]
    public static bool PreRegisterCoreBuff(ShootingManager __instance,MultipleChoiceMenu menu)
    {
        if (!GodEvolutionForceMutationBuff) return true;
        foreach (var plantType in __instance.CurrentPlants)
        {
            if (!Config.configs.TryGetValue(plantType, out var config))
                continue;

            // Damage share of this plant across the whole run
            float totalDamage = __instance.board.damageReporter.totalDamage;
            if (totalDamage == 0f)
                totalDamage = 1f;
            float damageShare =
                __instance.board.damageReporter.GetDamage(plantType) / totalDamage;

            int buffCount = __instance.GetPlantBuffsCount(plantType);

            foreach (var buff in config.Buffs)
            {
                var dc = new ShootingManager.__c__DisplayClass94_0
                {
                    __4__this = __instance
                };

                string buffTitle = buff.Title;
                int choiceCount = __instance.GetBuffChoiceCount(plantType, buffTitle);

                // Skip buffs at their limit or not currently available
                if (choiceCount >= buff.MaxCount || !buff.CanAppear)
                    continue;

                // 质变 (mutation) buffs: skip if one is already recorded
                bool hasMutation = false;
                if (buffTitle.Contains("质变")
                    && __instance.plantBuffRecords.TryGetValue(plantType, out var records))
                {
                    foreach (var record in records)
                    {
                        if (record.Key.Contains("质变"))
                        {
                            hasMutation = true;
                            break;
                        }
                    }
                }
                if (hasMutation)
                {
                    continue;
                }

                // 非质变词条保留原版的幸运加成出现概率判定，
                // 避免“质变词条概率大幅提升”把超进化等稀有词条也变成必出
                if (!buffTitle.Contains("质变"))
                {
                    float chance = buff.AppearWeight;
                    if (chance < 1f
                        && Random.value > (__instance._lucky * 0.3f + 1f) * chance)
                    {
                        continue;
                    }
                }

                dc.capturedPlant = plantType;
                dc.capturedBuffTitle = buffTitle;
                dc.originalOnGet = (UnityAction)buff.OnGet;

                string description = choiceCount > 0
                    ? string.Format("{0}\n已选了{1}次", buff.Description, choiceCount)
                    : buff.Description;

                // UpgradeBuff: append the plant's role/position name
                if (buff.TryCast<UpgradeBuff>()!=null)
                {
                    if (Config.configs.TryGetValue(buff.TryCast<UpgradeBuff>()!.ShowType, out var plantConfig))
                        description += "\n\n定位：" + plantConfig.Role;
                }
                // GeneralBuff: append damage share + buff count stats
                else if (buff.TryCast<GeneralBuff>() != null)
                {
                    description += string.Format(
                        "\n\n伤害占比：{0:F2}%\n总词条数：{1}",
                        damageShare * 100f,
                        buffCount);
                }

                menu.RegisterOption(
                    buffTitle,
                    description,
                    (UnityAction)dc.Method_Internal_Void_PDM_0,
                    buff.ShowType,
                    (ZombieType)(-1),
                    buff.Rarity);
            }
        }

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