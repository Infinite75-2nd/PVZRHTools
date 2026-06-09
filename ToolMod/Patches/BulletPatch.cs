using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(Bullet))]
public class BulletPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Bullet.Update))]
    public static void PostUpdate(Bullet __instance)
    {
        try
        {
            if (__instance == null) return;
            var bulletType = __instance.theBulletType;
            if (!BulletDamage.TryGetValue(bulletType, out var damage)) return;
            if (damage >= 0 && __instance.Damage != damage)
                __instance.Damage = damage;
        }
        catch
        {
        }
    }

    public static bool IsFromZombie(Bullet bullet)
    {
        if (bullet == null) return false;
        try
        {
            return bullet.shootByZombie || bullet.from_zombie != null;
        }
        catch
        {
            return false;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Bullet.Die))]
    public static bool PreDie(Bullet __instance)
    {
        if (__instance == null) return true;

        // 老版黑曜石子弹：前两次命中不销毁，实现“穿透两次”。
        // 性能优化：先做最廉价判定，再做来源判断，避免在高频 Die 上产生额外开销。
        if (OldObsidianBullet &&
            __instance.theBulletType == BulletType.Bullet_steelPea &&
            __instance.hitTimes < 2 &&
            !__instance.shootByZombie &&
            __instance.from_zombie == null)
        {
            __instance.hit = false;
            return false;
        }

        if (UndeadBullet && !__instance.shootByZombie && __instance.from_zombie == null)
        {
            __instance.hit = false;
            __instance.penetrationTimes = int.MaxValue;
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Bullet.Die))]
    public static bool PreDie2(Bullet __instance)
    {
        if (!MagnetNutUnlimited) return true;

        try
        {
            if (__instance == null || ShouldExcludeBullet(__instance)) return true;

            // 检查是否是因为存在时间过长要死亡
            if (__instance.theExistTime > 20.0f)
            {
                // 重置存在时间，阻止死亡
                __instance.theExistTime = 0.0f;
                return false; // 阻止死亡
            }

            return true;
        }
        catch
        {
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Bullet.OnTriggerEnter2D))]
    public static bool PreOnTriggerEnter2D(Bullet __instance, Collider2D collision)
    {
        if (ZombieBulletReflect <= 0) return true;

        try
        {
            // 只处理植物发射的子弹（非僵尸子弹）
            if (__instance == null || IsFromZombie(__instance)) return true;

            // 检查子弹是否已经命中过
            if (__instance.hit) return true;

            // 检查碰撞对象是否是僵尸
            if (collision == null) return true;
            var zombie = collision.GetComponent<Zombie>();
            if (zombie == null) return true;

            // 跳过魅惑僵尸（友方单位）
            if (zombie.isMindControlled) return true;

            // 跳过已死亡的僵尸
            if (zombie.theHealth <= 0) return true;

            // 概率判断
            float randomValue = Random.Range(0f, 100f);
            if (randomValue >= ZombieBulletReflect) return true;

            // 标记子弹已命中，防止后续处理
            __instance.hit = true;

            // 创建反弹的铁豆子弹
            CreateReflectedBullet(__instance, zombie);

            // 直接销毁子弹对象，不调用Die()方法（Die可能会触发伤害）
            Object.Destroy(__instance.gameObject);

            // 阻止原始的碰撞处理，僵尸不受伤
            return false;
        }
        catch
        {
            return true;
        }
    }

    /// <summary>
    /// 创建反弹的铁豆子弹
    /// </summary>
    private static void CreateReflectedBullet(Bullet originalBullet, Zombie zombie)
    {
        try
        {
            if (CreateBullet.Instance == null) return;

            // 获取原子弹的位置和行
            Vector3 pos = originalBullet.transform.position;
            int row = originalBullet.theBulletRow;

            // 创建一个铁豆子弹，向左飞行
            // fromEnermy/isZombieBullet = true 表示这是僵尸子弹，可以伤害植物
            var newBullet = CreateBullet.Instance.SetBullet(
                pos.x,
                pos.y,
                row,
                BulletType.Bullet_ironPea,
                BulletMoveWay.Left, // 向左飞行
                true // 这是僵尸子弹
            );

            if (newBullet != null)
            {
                // 设置子弹伤害（使用原子弹的伤害）
                newBullet.Damage = originalBullet.Damage;
            }
        }
        catch
        {
            // 忽略错误
        }
    }

    // 需要排除的子弹类型（这些子弹使用原始逻辑）
    private static readonly HashSet<string> _excludedBulletNames =
    [
        "Bullet_star", "Bullet_cactusStar", "Bullet_superStar", "Bullet_ultimateStar",
        "Bullet_lanternStar", "Bullet_seaStar", "Bullet_jackboxStar", "Bullet_pickaxeStar",
        "Bullet_magnetStar", "Bullet_ironStar", "Bullet_threeSpike",
        "Bullet_magicTrack", "Bullet_normalTrack", "Bullet_iceTrack", "Bullet_fireTrack",
        "Bullet_doom", "Bullet_doom_throw", "Bullet_endoSun", "Bullet_extremeSnowPea",
        "Bullet_iceSword", "Bullet_lourCactus", "Bullet_melonCannon",
        "Bullet_shulkLeaf_ultimate", "Bullet_smallGoldCannon", "Bullet_smallSun",
        "Bullet_springMelon", "Bullet_sunCabbage", "Bullet_ultimateSun"
    ];

    private static bool ShouldExcludeBullet(Bullet bullet)
    {
        if (bullet == null) return true;
        string className = bullet.GetType().Name;
        if (_excludedBulletNames.Contains(className)) return true;
        // 激进排除：包含特定关键词的子弹
        return className.Contains("Star") || className.Contains("Spike") ||
               className.Contains("Track") || className.Contains("Doom") ||
               className.Contains("Extreme") || className.Contains("Melon") ||
               className.Contains("Sun") || className.Contains("Cactus") ||
               className.Contains("Sword") || className.Contains("Cannon") ||
               className.Contains("Ultimate") || className.Contains("Super");
    }
}