using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(CreatePlant))]
public static class CreatePlantPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(CreatePlant.SetPlant))]
    public static void PreSetPlant(ref bool isFreeSet)
    {
        isFreeSet = FreePlanting || isFreeSet;
    }

    [HarmonyPatch(nameof(CreatePlant.LimTravel))]
    [HarmonyPatch(nameof(CreatePlant.Lim))]
    [HarmonyPrefix]
    public static bool PreLimTravel(ref bool __result)
    {
        if (RemoveFusionLimit)
        {
            __result = false;
            return false;
        }

        ;
        return true;
    }
}