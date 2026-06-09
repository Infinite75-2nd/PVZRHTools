using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

/// <summary>
/// 砸罐子修复补丁 - 核心补丁类
/// 功能：
/// 1. 多个罐子重叠时只砸开第一个罐子
/// 2. 小丑类的爆炸和巨人的砸击无法破坏罐子
/// 3. 土豆炸弹和大炸弹等AOE攻击无法破坏罐子
/// 4. 巨人僵尸忽略罐子，直接向前走
/// </summary>
[HarmonyPatch]
public static class PotSmashingPatches
{
    // 跟踪当前锤击事件中已经砸开的罐子
    private static readonly HashSet<ScaryPot> _hitPotsInCurrentSwing = new HashSet<ScaryPot>();

    // 跟踪当前锤击事件中已经处理的罐子（包括被阻止的）
    private static readonly HashSet<ScaryPot> _processedPotsInCurrentSwing = new HashSet<ScaryPot>();

    // 跟踪通过ScaryPot.Hitted调用的罐子
    private static readonly HashSet<ScaryPot> _hittedPots = new HashSet<ScaryPot>();

    // 标记当前是否正在处理僵尸爆炸（Lawnf.ZombieExplode）
    private static bool _isProcessingZombieExplode = false;

    // 标记当前是否正在处理小丑爆炸
    private static bool _isProcessingJackboxExplosion = false;

    public static void SetProcessingZombieExplode(bool value) => _isProcessingZombieExplode = value;
    public static bool IsProcessingZombieExplode() => _isProcessingZombieExplode;
    public static void SetProcessingJackboxExplosion(bool value) => _isProcessingJackboxExplosion = value;
    public static bool IsProcessingJackboxExplosion() => _isProcessingJackboxExplosion;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ScaryPot), nameof(ScaryPot.Hitted))]
    public static bool Prefix_ScaryPotHitted(ScaryPot __instance)
    {
        if (!PotSmashingFix) return true;

        if (IsAnyProjectileZombieRelatedInStack() || IsProjectileZombieAttackInStack() ||
            IsBombingAttack() || IsAnyProjectileZombieRelatedAttack())
            return false;

        if (_processedPotsInCurrentSwing.Contains(__instance))
            return false;

        if (_hitPotsInCurrentSwing.Count > 0)
        {
            _processedPotsInCurrentSwing.Add(__instance);
            return false;
        }

        _hitPotsInCurrentSwing.Add(__instance);
        _processedPotsInCurrentSwing.Add(__instance);
        _hittedPots.Add(__instance);
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ScaryPot), nameof(ScaryPot.OnHitted))]
    public static bool Prefix_ScaryPotOnHitted(ScaryPot __instance)
    {
        if (!PotSmashingFix) return true;

        try
        {
            if (_isProcessingZombieExplode || _isProcessingJackboxExplosion)
                return false;

            if (_hittedPots.Contains(__instance))
            {
                _hittedPots.Remove(__instance);
                return true;
            }

            return false;
        }
        catch
        {
            return true;
        }
    }

    private static bool IsProjectileZombieAttackInStack()
    {
        try
        {
            var stackTrace = new System.Diagnostics.StackTrace();
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                var method = frame?.GetMethod();
                var methodName = method?.Name ?? "";
                var className = method?.DeclaringType?.Name ?? "";
                if (className.Contains("PotSmashingPatches")) continue;
                if (className.Contains("ProjectileZombie") ||
                    (className.Contains("Bullet") && methodName.Contains("OnTriggerEnter2D")) ||
                    className.Contains("Submarine_b") || className.Contains("Submarine_c"))
                    return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsBombingAttack()
    {
        try
        {
            var stackTrace = new System.Diagnostics.StackTrace();
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                var method = frame?.GetMethod();
                var methodName = method?.Name ?? "";
                var className = method?.DeclaringType?.Name ?? "";
                if (className.Contains("PotSmashingPatches")) continue;
                if ((methodName.Contains("Explode") || methodName.Contains("Bomb") ||
                     methodName.Contains("HitLand") || methodName.Contains("HitZombie")) &&
                    (className.Contains("Bullet") || className.Contains("ProjectileZombie") ||
                     className.Contains("Submarine")))
                    return true;
                if (className.Contains("ProjectileZombie") &&
                    (methodName.Contains("Update") || methodName.Contains("FixedUpdate") ||
                     methodName.Contains("RbUpdate")))
                    return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsAnyProjectileZombieRelatedAttack()
    {
        try
        {
            var stackTrace = new System.Diagnostics.StackTrace();
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                var method = frame?.GetMethod();
                var methodName = method?.Name ?? "";
                var className = method?.DeclaringType?.Name ?? "";
                if (className.Contains("PotSmashingPatches")) continue;
                if (className.Contains("ProjectileZombie") ||
                    className.Contains("Submarine_b") || className.Contains("Submarine_c") ||
                    (className.Contains("Bullet") && (methodName.Contains("OnTriggerEnter2D") ||
                                                      methodName.Contains("HitLand") ||
                                                      methodName.Contains("HitZombie"))))
                    return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsAnyProjectileZombieRelatedInStack()
    {
        try
        {
            var stackTrace = new System.Diagnostics.StackTrace();
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                var method = frame?.GetMethod();
                var methodName = method?.Name ?? "";
                var className = method?.DeclaringType?.Name ?? "";
                if (className.Contains("PotSmashingPatches")) continue;
                if (className.Contains("ProjectileZombie") || className.Contains("Submarine") ||
                    methodName.Contains("SetBullet") || methodName.Contains("AnimShoot") ||
                    methodName.Contains("ProjectileZombie"))
                    return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Board), nameof(Board.Update))]
    public static void Postfix_BoardUpdate()
    {
        if (!PotSmashingFix) return;
        _hitPotsInCurrentSwing.Clear();
        _processedPotsInCurrentSwing.Clear();
    }
}

/// <summary>
/// 巨人僵尸忽略罐子补丁
/// </summary>
[HarmonyPatch]
public static class GargantuarIgnorePotPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(IronGargantuar), nameof(IronGargantuar.OnTriggerEnter2D))]
    public static bool Prefix_IronGargantuarOnTriggerEnter2D(IronGargantuar __instance, Collider2D collision)
    {
        if (!PotSmashingFix) return true;
        try
        {
            if (collision == null) return true;
            var scaryPot = collision.GetComponent<ScaryPot>();
            if (scaryPot != null) return false;
            return true;
        }
        catch
        {
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Gargantuar), nameof(Gargantuar.GargantuarAttackUpdate))]
    public static bool Prefix_GargantuarAttackUpdate(Gargantuar __instance)
    {
        if (!PotSmashingFix) return true;
        try
        {
            if (IsGargantuarAttackingPot(__instance)) return false;
            return true;
        }
        catch
        {
            return true;
        }
    }

    private static bool IsGargantuarAttackingPot(Gargantuar gargantuar)
    {
        try
        {
            var zombie = gargantuar.GetComponent<Zombie>();
            if (zombie == null) return false;
            var rigidbody = gargantuar.GetComponent<Rigidbody2D>();
            if (rigidbody != null && rigidbody.velocity.magnitude < 0.1f)
            {
                var colliders = Physics2D.OverlapCircleAll(gargantuar.transform.position, 5.0f);
                foreach (var collider in colliders)
                    if (collider.GetComponent<ScaryPot>() != null)
                        return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// 小丑僵尸爆炸保护补丁 - 让小丑可以爆炸，但爆炸不影响罐子
/// </summary>
[HarmonyPatch]
public static class JackboxZombieProtectionPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(JackboxZombie), nameof(JackboxZombie.Explode))]
    public static bool Prefix_JackboxZombieExplode()
    {
        if (PotSmashingFix) PotSmashingPatches.SetProcessingJackboxExplosion(true);
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(JackboxZombie), nameof(JackboxZombie.Explode))]
    public static void Postfix_JackboxZombieExplode()
    {
        if (PotSmashingFix) PotSmashingPatches.SetProcessingJackboxExplosion(false);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(JackboxZombie), nameof(JackboxZombie.AnimExplode))]
    public static bool Prefix_JackboxZombieAnimExplode()
    {
        if (PotSmashingFix) PotSmashingPatches.SetProcessingJackboxExplosion(true);
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(JackboxZombie), nameof(JackboxZombie.AnimExplode))]
    public static void Postfix_JackboxZombieAnimExplode()
    {
        if (PotSmashingFix) PotSmashingPatches.SetProcessingJackboxExplosion(false);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SuperJackboxZombie), nameof(SuperJackboxZombie.AnimExplode))]
    public static bool Prefix_SuperJackboxZombieAnimExplode()
    {
        if (PotSmashingFix) PotSmashingPatches.SetProcessingJackboxExplosion(true);
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SuperJackboxZombie), nameof(SuperJackboxZombie.AnimExplode))]
    public static void Postfix_SuperJackboxZombieAnimExplode()
    {
        if (PotSmashingFix) PotSmashingPatches.SetProcessingJackboxExplosion(false);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(UltimateJackboxZombie), nameof(UltimateJackboxZombie.AnimPop))]
    public static bool Prefix_UltimateJackboxZombieAnimPop()
    {
        if (PotSmashingFix) PotSmashingPatches.SetProcessingJackboxExplosion(true);
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UltimateJackboxZombie), nameof(UltimateJackboxZombie.AnimPop))]
    public static void Postfix_UltimateJackboxZombieAnimPop()
    {
        if (PotSmashingFix) PotSmashingPatches.SetProcessingJackboxExplosion(false);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(JackboxJumpZombie), nameof(JackboxJumpZombie.DieEvent))]
    public static bool Prefix_JackboxJumpZombieDieEvent()
    {
        if (PotSmashingFix) PotSmashingPatches.SetProcessingJackboxExplosion(true);
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(JackboxJumpZombie), nameof(JackboxJumpZombie.DieEvent))]
    public static void Postfix_JackboxJumpZombieDieEvent()
    {
        if (PotSmashingFix) PotSmashingPatches.SetProcessingJackboxExplosion(false);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Jackbox_a), nameof(Jackbox_a.LoseHeadEvent))]
    public static bool Prefix_Jackbox_aLoseHeadEvent()
    {
        if (PotSmashingFix) PotSmashingPatches.SetProcessingJackboxExplosion(true);
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Jackbox_a), nameof(Jackbox_a.LoseHeadEvent))]
    public static void Postfix_Jackbox_aLoseHeadEvent()
    {
        if (PotSmashingFix) PotSmashingPatches.SetProcessingJackboxExplosion(false);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Jackbox_c), nameof(Jackbox_c.LoseHeadEvent))]
    public static bool Prefix_Jackbox_cLoseHeadEvent()
    {
        if (PotSmashingFix) PotSmashingPatches.SetProcessingJackboxExplosion(true);
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Jackbox_c), nameof(Jackbox_c.LoseHeadEvent))]
    public static void Postfix_Jackbox_cLoseHeadEvent()
    {
        if (PotSmashingFix) PotSmashingPatches.SetProcessingJackboxExplosion(false);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SuperJackboxZombie), nameof(SuperJackboxZombie.DieEvent))]
    public static bool Prefix_SuperJackboxZombieDieEvent()
    {
        if (PotSmashingFix) PotSmashingPatches.SetProcessingJackboxExplosion(true);
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SuperJackboxZombie), nameof(SuperJackboxZombie.DieEvent))]
    public static void Postfix_SuperJackboxZombieDieEvent()
    {
        if (PotSmashingFix) PotSmashingPatches.SetProcessingJackboxExplosion(false);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(UltimateJackboxZombie), nameof(UltimateJackboxZombie.DieEvent))]
    public static bool Prefix_UltimateJackboxZombieDieEvent()
    {
        if (PotSmashingFix) PotSmashingPatches.SetProcessingJackboxExplosion(true);
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UltimateJackboxZombie), nameof(UltimateJackboxZombie.DieEvent))]
    public static void Postfix_UltimateJackboxZombieDieEvent()
    {
        if (PotSmashingFix) PotSmashingPatches.SetProcessingJackboxExplosion(false);
    }
}

/// <summary>
/// Lawnf.ZombieExplode 补丁 - 阻止僵尸爆炸破坏罐子
/// </summary>
[HarmonyPatch]
public static class ZombieExplodeProtectionPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Lawnf), nameof(Lawnf.ZombieExplode))]
    public static bool Prefix_LawnfZombieExplode()
    {
        if (PotSmashingFix) PotSmashingPatches.SetProcessingZombieExplode(true);
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Lawnf), nameof(Lawnf.ZombieExplode))]
    public static void Postfix_LawnfZombieExplode()
    {
        if (PotSmashingFix) PotSmashingPatches.SetProcessingZombieExplode(false);
    }
}