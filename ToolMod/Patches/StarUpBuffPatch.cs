using HarmonyLib;
using static ToolMod.Components.PatchDataCache;
using StarUpBuff = GameLevel.RogueShooting.StarUpBuff;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(StarUpBuff))]
public static class StarUpBuffPatch
{
    [HarmonyPatch(nameof(StarUpBuff.AppearWeight), MethodType.Getter)]
    [HarmonyPostfix]
    public static void PostAppearWeight(StarUpBuff __instance,ref float __result)
    {
        if(GodEvolutionForceStarUpBuff)__result = 1;
    }
}