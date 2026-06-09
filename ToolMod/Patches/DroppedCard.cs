using HarmonyLib;
using static ToolMod.Components.PatchDataCache;


namespace ToolMod.Patches;

[HarmonyPatch(typeof(DroppedCard))]
public static class DroppedCardPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(DroppedCard.Update))]
    public static void PostUpdate(DroppedCard __instance)
    {
        if (!ItemExistForever) return;
        try
        {
            if (__instance != null) __instance.existTime = 0;
        }
        catch
        {
        }
    }
}