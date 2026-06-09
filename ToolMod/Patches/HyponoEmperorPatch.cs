using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(HyponoEmperor))]
public static class HyponoEmperorPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HyponoEmperor.Update))]
    public static void PostUpdate(HyponoEmperor __instance)
    {
        if (!HyponoEmperorNoCD) return;
        try
        {
            if (__instance != null && __instance.summonZombieTime > 2f)
                __instance.summonZombieTime = 2f;
        }
        catch
        {
        }
    }
}