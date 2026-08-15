using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(SlowTrigger))]
public static class SlowTriggerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(SlowTrigger.OnMouseUpAsButton))]
    public static void PreOnMouseUpAsButton(SlowTrigger __instance)
    {
        TimeStop = false;
        TimeSlow = !TimeSlow;
    }
}