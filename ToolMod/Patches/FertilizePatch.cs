using HarmonyLib;
using static ToolMod.Components.PatchDataCache;


namespace ToolMod.Patches;

[HarmonyPatch(typeof(Fertilize))]
public static class FertilizePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Fertilize.Update))]
    public static void PostUpdate(Fertilize __instance)
    {
        if (!ItemExistForever) return;
        try
        {
            if (__instance != null) __instance.existTime = 0.1f;
        }
        catch
        {
        }
    }
}