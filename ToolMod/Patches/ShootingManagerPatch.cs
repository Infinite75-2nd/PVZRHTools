using GameLevel.RogueShooting;
using HarmonyLib;
using UI;
using UnityEngine;
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
        ApplySettings(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ShootingManager.GetRandomQuality))]
    public static bool PreGetRandomQuality(ref Quality __result)
    {
        if (!GodEvolutionQualityWeightEnabled) return true;
        __result = RollQuality();
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

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ShootingManager.ShowBuff))]
    public static void Prefix(ShootingManager __instance)
    {
        if (!ShouldFixGodEvolutionRefreshButton || __instance == null) return;
        __instance.refreshCount = GetGodEvolutionMenuRefreshCount();
    }
}