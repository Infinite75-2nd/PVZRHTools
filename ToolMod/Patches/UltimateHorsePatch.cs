using System;
using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

/// <summary>
/// 诅咒免疫补丁 - UltimateHorse.GetDamage
/// 阻止终极马僵尸的诅咒效果
/// </summary>
[HarmonyPatch(typeof(UltimateHorse))]
public static class UltimateHorsePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UltimateHorse.GetDamage), typeof(int), typeof(DamageType), typeof(bool), typeof(PlantType))]
    public static bool Prefix(UltimateHorse __instance, ref int theDamage)
    {
        if (!CurseImmunity) return true;
        try
        {
            // 如果诅咒免疫激活，清空诅咒植物列表
            if (__instance != null)
            {
                Utils.ClearCursedPlants(__instance);
            }
        }
        catch
        {
        }

        return true;
    }
}