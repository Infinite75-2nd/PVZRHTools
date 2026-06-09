using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(CreateBullet))]
public static class CreateBulletPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(CreateBullet.SetBullet))]
    public static void PreSetBullet(ref BulletType theBulletType)
    {
        if (LockBullet is -1 || RandomBullet)
            theBulletType = GameAPP.resourcesManager.allBullets.GetRandom();
        if (LockBullet >= 0) theBulletType = (BulletType)LockBullet;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(CreateBullet.SetBullet))]
    public static void PostSetBullet(Bullet __result)
    {
        try
        {
            if (!OldObsidianBullet || __result == null) return;
            if (__result.theBulletType != BulletType.Bullet_steelPea) return;
            if (__result.shootByZombie || __result.from_zombie != null) return;

            // 老版黑曜石子弹：至少穿透两次
            if (__result.penetrationTimes < 2)
                __result.penetrationTimes = 2;
        }
        catch
        {
        }
    }
}