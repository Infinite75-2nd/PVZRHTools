using HarmonyLib;
using static ToolMod.Components.PatchDataCache;
using static ToolMod.Utils;


namespace ToolMod.Patches;

/// <summary>
/// 巨人盲盒僵尸：覆盖 FirstArmorFall，在盔甲掉落时按 PvE 槽6 指定僵尸生成并让自身死亡
/// </summary>
[HarmonyPatch(typeof(RandomGargantuar))]
public static class RandomGargantuarPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(RandomGargantuar.FirstArmorFall))]
    public static bool PreFirstArmorFall(RandomGargantuar __instance)
    {
        if (!InGame || Board.Instance == null || CreateZombie.Instance == null)
            return true;

        int instId = __instance.GetInstanceID();
        if (!PveBlindBoxSlotByInstance.TryGetValue(instId, out int slot) || slot != 6)
            return true; // 只处理 PvE 布阵中的那一个巨人盲盒

        if (ZombieSlot6Index < 0)
            return true; // 未配置则保持原逻辑

        // 用完就移除，避免后续其它逻辑再次误用
        PveBlindBoxSlotByInstance.Remove(instId);

        var axis = __instance.axis;
        if (axis == null) return true;

        var pos = axis.position;
        int row = __instance.theZombieRow;

        if (__instance.isMindControlled)
            CreateZombie.Instance.SetZombieWithMindControl(row, ZombieSlot6Index, pos.x);
        else
            CreateZombie.Instance.SetZombie(row, ZombieSlot6Index, pos.x);

        // 模拟原逻辑的结尾：让巨人死亡（reason=2）
        __instance.Die(2);

        // 不再调用原始 FirstArmorFall（其中的随机逻辑和特效大部分为美术表现，可忽略）
        return false;
    }
}