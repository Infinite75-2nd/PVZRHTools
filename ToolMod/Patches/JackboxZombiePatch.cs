using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(JackboxZombie))]
public static class JackboxZombiePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(JackboxZombie.Update))]
    public static void PostUpdate(JackboxZombie __instance)
    {
        if (!JackboxNotExplode) return;
        try
        {
            if (__instance != null)
                __instance.popCountDown = __instance.originalCountDown;
        }
        catch
        {
        }
    }
}