using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(HypnoEmperor))]
public static class HypnoEmperorPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HypnoEmperor.Update))]
    public static void PostUpdate(HypnoEmperor __instance)
    {
        if (!HyponoEmperorNoCD) return;
        try
        {
            if (__instance != null && __instance.AttributeCountdown > 2f)
                __instance.AttributeCountdown = 2f;
        }
        catch
        {
        }
    }
}