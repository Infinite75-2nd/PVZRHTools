using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(TravelRefresh))]
public static class TravelRefreshPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(TravelRefresh.Awake))]
    [HarmonyPatch(nameof(TravelRefresh.OnMouseUpAsButton))]
    public static void PostAwake(TravelRefresh __instance)
    {
        if (UnlimitedRefresh)
        {
            __instance.refreshTimes = 99999999;
        }
    }
}