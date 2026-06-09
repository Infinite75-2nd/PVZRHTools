using HarmonyLib;
using ToolMod.Components;

namespace ToolMod.Patches;

/// <summary>
/// 究极冷寂榴弹炮无CD装填补丁 - UltimateMelonCannon.StartShoot
/// UltimateMelonCannon继承自MelonCannon，但有自己的StartShoot方法
/// </summary>
[HarmonyPatch(typeof(UltimateMelonCannon))]
public static class UltimateMelonCannonPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UltimateMelonCannon.StartShoot))]
    public static void PostStartShoot(UltimateMelonCannon __instance)
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