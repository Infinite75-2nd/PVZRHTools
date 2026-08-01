using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(TalentNode))]
public static class TalentNodePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(TalentNode.OnPointerDown))]
    public static void PreOnPointerDown(TalentNode __instance)
    {
        if (StarAdvFreeBuff)
        {
            __instance.data.cost = int.MinValue;
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(nameof(TalentNode.OnPointerDown))]
    public static void PostOnPointerDown(TalentNode __instance)
    {
        if (StarAdvFreeBuff)
        {
            __instance.data.cost = 0;
        }
    }
}