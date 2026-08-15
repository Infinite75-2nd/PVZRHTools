using System.Collections.Generic;
using System.Reflection;
using GameLevel.RogueShooting;
using HarmonyLib;
using UI;
using UnityEngine;
using UnityEngine.Events;
using static ToolMod.Utils;
using static ToolMod.Components.PatchDataCache;

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
            if (GodEvolutionLucky >= 0)
                __instance.Lucky = GodEvolutionLucky;
            if (GodEvolutionDifficulty >= 0)
                __instance.difficulty = GodEvolutionDifficulty;
            if (ShouldFixGodEvolutionRefreshButton)
                __instance.refreshCount = GetGodEvolutionMenuRefreshCount();
            if (GodEvolutionMaxPlantCount >= 0)
                __instance.maxPlantCount = GodEvolutionMaxPlantCount;
            if (GodEvolutionSuperUpgrade)
                __instance.superUpgrade = GodEvolutionSuperUpgrade;
            if (GodEvolutionUncrashable)
                (_uncrashableField ??= typeof(ShootingManager).GetField("uncrashable",
                    BindingFlags.Instance | BindingFlags.NonPublic))?.SetValue(__instance, true);
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
                        Quality.diamond);
                    break;

                case 1: // 超质变：步步高升
                    menu.RegisterOption(
                        "超质变：步步高升",
                        "所有词条一定是最高品质，且钻石词条的加成x5\n注意：部分植物攻速过快时会丢失动画导致无法攻击或攻速降低",
                        (UnityAction)(__instance._RegisterOtherBuff_b__96_15),
                        (PlantType)254,
                        (ZombieType)(-1),
                        Quality.diamond);
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
                        Quality.diamond);
                    break;

                case 3: // 超质变：神秘大炮 — NEW
                    menu.RegisterOption(
                        "超质变：神秘大炮",
                        "获得一个神秘大炮",
                        ShootingManager.__c.__9__96_17 ?? (ShootingManager.__c.__9__96_17 = (UnityAction)(ShootingManager.__c.__9._RegisterOtherBuff_b__96_17)),
                        (PlantType)3,
                        (ZombieType)(-1),
                        Quality.diamond);
                    break;
            }
        }
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
    }
}