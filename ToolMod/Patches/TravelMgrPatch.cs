using System;
using System.Collections;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using UnityEngine;
using static ToolMod.Utils;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

/// <summary>
/// 监听游戏词条状态变化，实时同步到修改器
/// </summary>
[HarmonyPatch(typeof(TravelMgr))]
public static class TravelMgrPatch
{
    /// <summary>
    /// GetNormalBuff 后置补丁：解锁高级词条后实时同步到修改器
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(TravelMgr.GetNormalBuff))]
    public static void PostGetNormalBuff(TravelMgr __instance, AdvBuff __0)
    {
        if (OperatingBuff) return;
        try
        {
            // 延迟一小段时间后同步，确保游戏状态已更新
            __instance.StartCoroutine(SyncBuffsDelayed());
        }
        catch (System.Exception ex)
        {
            ModCore.Instance.Log?.LogWarning($"[PVZRHTools] PostGetNormalBuff 异常: {ex.Message}");
        }
    }

    /// <summary>
    /// GetUltiBuff 后置补丁：解锁究极词条后实时同步到修改器
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(TravelMgr.GetUltiBuff))]
    public static void PostGetUltiBuff(TravelMgr __instance, UltiBuff __0, bool __1)
    {
        if (OperatingBuff) return;
        try
        {
            // 延迟一小段时间后同步，确保游戏状态已更新
            __instance.StartCoroutine(SyncBuffsDelayed());
        }
        catch (System.Exception ex)
        {
            ModCore.Instance.Log?.LogWarning($"[PVZRHTools] PostGetUltiBuff 异常: {ex.Message}");
        }
    }

    /// <summary>
    /// GetDebuff 后置补丁：解锁负面词条后实时同步到修改器
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(TravelMgr.GetDebuff))]
    public static void PostGetDebuff(TravelMgr __instance, TravelDebuff __0)
    {
        if (OperatingBuff) return;
        try
        {
            // 延迟一小段时间后同步，确保游戏状态已更新
            __instance.StartCoroutine(SyncBuffsDelayed());
        }
        catch (System.Exception ex)
        {
            ModCore.Instance.Log?.LogWarning($"[PVZRHTools] PostGetDebuff 异常: {ex.Message}");
        }
    }

    /// <summary>
    /// GetInvestBuff 后置补丁：解锁投资词条后实时同步到修改器
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(TravelMgr.GetInvestBuff))]
    public static void PostGetInvestBuff(TravelMgr __instance, InvestBuff __0)
    {
        if (OperatingBuff) return;
        try
        {
            // 延迟一小段时间后同步，确保游戏状态已更新
            __instance.StartCoroutine(SyncBuffsDelayed());
        }
        catch (System.Exception ex)
        {
            ModCore.Instance.Log?.LogWarning($"[PVZRHTools] PostGetInvestBuff 异常: {ex.Message}");
        }
    }

    /// <summary>
    /// UnlockPlant 后置补丁：解锁植物后实时同步到修改器
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(TravelMgr.UnlockPlant))]
    public static void PostUnlockPlant(TravelMgr __instance, TravelUnlocks __0)
    {
        if (OperatingBuff) return;
        try
        {
            // 延迟一小段时间后同步，确保游戏状态已更新
            __instance.StartCoroutine(SyncBuffsDelayed());
        }
        catch (System.Exception ex)
        {
            ModCore.Instance.Log?.LogWarning($"[PVZRHTools] PostUnlockPlant 异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 延迟同步词条状态，确保游戏状态已更新
    /// </summary>
    private static IEnumerator SyncBuffsDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        SyncGameBuffsToModifier();
    }

    /// <summary>
    /// TravelMgr 安全兜底补丁：
    /// - 在 TravelMgr.GetNormalBuff 与 UpdateSynergies 发生早期空引用时，吞掉异常并记录告警，避免崩溃
    /// - 原因：某些情况下（例如极早期恢复、MOD词条延迟注册），TravelMgr 内部数据结构未就绪
    /// </summary>
    [HarmonyFinalizer]
    [HarmonyPatch(nameof(TravelMgr.GetNormalBuff))]
    public static Exception Finalizer_GetNormalBuff(Exception __exception)
    {
        if (__exception != null)
        {
            try
            {
                ModCore.Instance.Log?.LogWarning(
                    $"[PVZRHTools] GetNormalBuff 发生异常，已忽略：{__exception.GetType().Name} - {__exception.Message}");
            }
            catch
            {
            }

            return null; // 吞掉异常，防止崩溃
        }

        return null;
    }

    [HarmonyFinalizer]
    [HarmonyPatch(nameof(TravelMgr.UpdateSynergies))]
    public static Exception Finalizer_UpdateSynergies(Exception __exception)
    {
        if (__exception != null)
        {
            try
            {
                ModCore.Instance.Log?.LogWarning(
                    $"[PVZRHTools] UpdateSynergies 发生异常，已忽略：{__exception.GetType().Name} - {__exception.Message}");
            }
            catch
            {
            }

            return null; // 吞掉异常
        }

        return null;
    }
}