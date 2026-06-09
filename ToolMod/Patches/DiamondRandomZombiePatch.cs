using HarmonyLib;
using UnityEngine;
using static ToolMod.Components.PatchDataCache;
using static ToolMod.Utils;


namespace ToolMod.Patches;

/// <summary>
/// 钻石盲盒僵尸：覆盖 SetRandomZombie，实现按 PvE 槽1 指定僵尸生成
/// </summary>
[HarmonyPatch(typeof(DiamondRandomZombie), nameof(DiamondRandomZombie.SetRandomZombie))]
public static class DiamondRandomZombiePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(DiamondRandomZombie.SetRandomZombie))]
    public static bool PreSetRandomZombie(DiamondRandomZombie __instance, ref Zombie __result, Vector3 pos)
    {
        if (!InGame || Board.Instance == null || CreateZombie.Instance == null)
            return true;

        int instId = __instance.GetInstanceID();
        if (!PveBlindBoxSlotByInstance.TryGetValue(instId, out int slot) || slot != 1)
            return true; // 只处理 PvE 布阵中的那一个钻石盲盒

        if (ZombieSlot1Index < 0)
            return true; // 未配置则保持原逻辑

        // 用完就移除，避免后续其它逻辑再次误用
        PveBlindBoxSlotByInstance.Remove(instId);

        float x = pos.x;

        if (!__instance.isMindControlled)
            __result = CreateZombie.Instance.SetZombie(__instance.theZombieRow, ZombieSlot1Index, x);
        else
            __result = CreateZombie.Instance.SetZombieWithMindControl(__instance.theZombieRow, ZombieSlot1Index, x);

        // 不再走原始随机逻辑
        return false;
    }
}