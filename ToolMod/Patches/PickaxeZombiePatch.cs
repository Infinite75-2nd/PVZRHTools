using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

/// <summary>
/// 矿镐免疫补丁 - PickaxeZombie.ZombieUpdate
/// 阻止第二种矿工挖掘植物
/// </summary>
[HarmonyPatch(typeof(PickaxeZombie))]
public static class PickaxeZombiePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(PickaxeZombie.ZombieUpdate))]
    public static bool PreZombieUpdate(PickaxeZombie __instance)
    {
        if (!PickaxeImmunity) return true;
        try
        {
            // 检查矿工是否有攻击目标
            if (__instance?.theAttackTarget != null)
            {
                // 阻止挖掘任何植物
                return false;
            }
        }
        catch
        {
        }

        return true;
    }
}