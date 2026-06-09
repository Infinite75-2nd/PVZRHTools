using HarmonyLib;
using ToolMod.Components;

namespace ToolMod.Patches;

/// <summary>
/// 西瓜加农炮无CD装填补丁 - MelonCannon.AnimShoot
/// </summary>
[HarmonyPatch(typeof(MelonCannon))]
public static class MelonCannonPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(MelonCannon.AnimShoot))]
    public static void PostAnimShoot(MelonCannon __instance)
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