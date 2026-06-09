using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(Bullet_steelPea), "HitZombie")]
public static class Bullet_steelPeaPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Bullet_steelPea.HitZombie))]
    public static void PostHitZombie(Bullet_steelPea __instance, Zombie zombie)
    {
        try
        {
            if (!OldObsidianBullet || __instance == null || zombie == null) return;
            if (__instance.shootByZombie || __instance.from_zombie != null) return;
            if (zombie.theHealth <= 0) return;

            // 命中计数在不同流程里的更新时机不完全一致，这里用 <= 1 兼容首段命中。
            if (__instance.hitTimes <= 1)
            {
                zombie.KnockBack(0.1f);
            }
        }
        catch
        {
        }
    }
}