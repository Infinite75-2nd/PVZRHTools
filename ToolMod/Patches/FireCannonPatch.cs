using HarmonyLib;
using ToolMod.Components;

namespace ToolMod.Patches;

/// <summary>
/// 火焰加农炮无CD装填补丁 - FireCannon.AnimShoot
/// </summary>
[HarmonyPatch(typeof(FireCannon))]
public static class FireCannonPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(FireCannon.AnimShoot))]
    public static void PostAnimShoot(FireCannon __instance)
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