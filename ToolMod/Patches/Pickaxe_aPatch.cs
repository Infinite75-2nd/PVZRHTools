using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

/// <summary>
/// 矿镐免疫补丁 - Pickaxe_a.ZombieUpdate
/// 阻止第一种矿工挖掘植物
/// </summary>
[HarmonyPatch(typeof(Pickaxe_a))]
public static class Pickaxe_aPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Pickaxe_a.ZombieUpdate))]
    public static bool PreZombieUpdate(Pickaxe_a __instance)
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