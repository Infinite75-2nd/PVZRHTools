using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(Lawnf))]
public static class LawnfPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Lawnf.BannedInAbyss))]
    public static bool PreBannedInAbyss(ref bool __result)
    {
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Lawnf.CheckIfPlantUnlock))]
    public static void PreCheckIfPlantUnlock(ref UnlockType __result)
    {
        if (EnableAllCards)
        {
            __result = UnlockType.Unlocked;
        }
    }
}