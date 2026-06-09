using HarmonyLib;
using ToolMod.Components;

namespace ToolMod.Patches;

/// <summary>
/// 寒冰加农炮无CD装填补丁 - IceCannon.AnimShoot
/// </summary>
[HarmonyPatch(typeof(IceCannon))]
public static class IceCannonPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(IceCannon.AnimShoot))]
    public static void PostAnimShoot(IceCannon __instance)
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