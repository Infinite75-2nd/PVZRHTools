using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

/// <summary>
/// 加农炮无CD装填补丁 - CobCannon.AnimShoot
/// 在加农炮发射后立即触发charge动画并重置冷却时间，实现无冷却装填
/// </summary>
[HarmonyPatch(typeof(CobCannon))]
public static class CobCannonPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(CobCannon.AnimShoot))]
    public static void PostAnimShoot(CobCannon __instance)
    {
        if (!CobCannonNoCD) return;
        try
        {
            if (__instance != null)
            {
                // 重置冷却时间，使加农炮可以立即再次发射
                __instance.attributeCountdown = 0.05f;
                // 触发charge动画
                if (__instance.anim != null)
                    __instance.anim.SetTrigger("charge");
            }
        }
        catch
        {
        }
    }
}