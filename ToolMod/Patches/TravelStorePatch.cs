using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(TravelStore))]
public static class TravelStorePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(TravelStore.RefreshBuff))]
    public static void PostRefreshBuff(TravelStore __instance)
    {
        if (UnlimitedRefresh)
        {
            __instance.refreshCount = 0;
        }
    }
}