using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

/// <summary>
/// 矿镐免疫补丁 - HypnoJalapenoPickaxeZombie.ZombieUpdate
/// 阻止魅惑辣椒矿工挖掘植物
/// </summary>
[HarmonyPatch(typeof(HypnoJalapenoPickaxeZombie))]
public static class HypnoJalapenoPickaxeZombieImmunityPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(HypnoJalapenoPickaxeZombie.ZombieUpdate))]
    public static bool PreZombieUpdate(HypnoJalapenoPickaxeZombie __instance)
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