using HarmonyLib;
using ToolMod.Components;

namespace ToolMod.Patches;

/// <summary>
/// 究极爆破加农炮无CD装填补丁 - UltimateExplodeCannon.AnimShoot
/// </summary>
[HarmonyPatch(typeof(UltimateExplodeCannon))]
public static class UltimateExplodeCannonPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UltimateExplodeCannon.AnimShoot))]
    public static void PostAnimShoot(UltimateExplodeCannon __instance)
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