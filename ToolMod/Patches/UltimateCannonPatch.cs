using HarmonyLib;
using ToolMod.Components;

namespace ToolMod.Patches;

/// <summary>
/// 究极加农炮无CD装填补丁 - UltimateCannon.AnimShoot
/// </summary>
[HarmonyPatch(typeof(UltimateCannon))]
public static class UltimateCannonPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UltimateCannon.AnimShoot))]
    public static void PostAnimShoot(UltimateCannon __instance)
    {
        if (!PatchDataCache.CobCannonNoCD) return;
        try
        {
            if (__instance != null)
            {
                __instance.attributeCountdown = 0.05f;
                if (__instance.anim != null)
                    __instance.anim.SetTrigger("charge");
            }
        }
        catch
        {
        }
    }
}