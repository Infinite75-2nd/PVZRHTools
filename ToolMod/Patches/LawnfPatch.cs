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

}