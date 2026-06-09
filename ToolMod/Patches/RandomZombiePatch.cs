using HarmonyLib;
using Unity.VisualScripting;
using UnityEngine;
using static ToolMod.Components.PatchDataCache;
using static ToolMod.Utils;


namespace ToolMod.Patches;

/// <summary>
/// 黄金盲盒僵尸：覆盖 FirstArmorFall，在开盒时按 PvE 槽2~5 指定僵尸生成并让自身死亡
/// </summary>
[HarmonyPatch(typeof(RandomZombie))]
public static class RandomZombiePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(RandomZombie.FirstArmorFall))]
    public static bool PreFirstArmorFall(RandomZombie __instance)
    {
        if (!InGame || Board.Instance == null || CreateZombie.Instance == null)
            return true;

        if (__instance == null)
            return true;

        int instId = __instance.GetInstanceID();
        if (!PveBlindBoxSlotByInstance.TryGetValue(instId, out int slot))
            return true;

        // 只处理 PvE 布阵中记录的 4 个黄金盲盒 (槽2~5)
        if (slot < 2 || slot > 5)
            return true;

        ZombieType targetId = slot switch
        {
            2 => ZombieSlot2Index,
            3 => ZombieSlot3Index,
            4 => ZombieSlot4Index,
            5 => ZombieSlot5Index,
            _ => ZombieType.Nothing
        };

        // 未配置则走原逻辑
        if ((int)targetId < 0)
            return true;

        // 用完就移除，避免后续其它逻辑再次误用
        PveBlindBoxSlotByInstance.Remove(instId);

        var axis = __instance.axis;
        if (axis == null)
            return true;

        var pos = axis.position;
        int row = __instance.theZombieRow;

        if (__instance.isMindControlled)
            CreateZombie.Instance.SetZombieWithMindControl(row, targetId, pos.x);
        else
            CreateZombie.Instance.SetZombie(row, targetId, pos.x);

        // 模拟原逻辑的结尾：播放粒子并让自身死亡
        try
        {
            // 原逻辑中在 FirstArmorFall 里会调用 CreateParticle.SetParticle(11, pos+偏移, row, true)
            // 这里简单复用 Lawnf/Board 的通用粒子接口（如果可用），否则忽略粒子表现
            var go = __instance.GameObject();
            if (go != null)
            {
                var p = go.transform.position;
                CreateParticle.SetParticle(11, new Vector2(p.x, p.y + 1f), row);
            }
        }
        catch
        {
            // 粒子失败不影响主要逻辑
        }

        __instance.Die(2);
        return false;
    }
}