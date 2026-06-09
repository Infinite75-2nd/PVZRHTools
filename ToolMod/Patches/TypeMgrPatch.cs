using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(TypeMgr))]
public static class TypeMgrPatch
{
    /// <summary>
    /// 踩踏免疫补丁 - TypeMgr.UncrashablePlant
    /// 这是游戏判断植物是否免疫碾压的核心方法
    /// Boss类领袖等僵尸会调用此方法来判断是否可以碾压植物
    /// 参考 SuperMachinePot 的 TypeMgrUncrashablePlantPatch 实现
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(TypeMgr.UncrashablePlant))]
    public static bool PreUncrashablePlant(ref Plant plant, ref bool __result)
    {
        if (!TrampleImmunity) return true;

        try
        {
            if (plant == null)
                return true;

            // 当踩踏免疫开启时，所有植物都免疫碾压
            __result = true;
            return false; // 不执行原方法
        }
        catch
        {
        }

        return true;
    }
}