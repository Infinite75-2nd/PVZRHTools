using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

/// <summary>
/// 诅咒免疫补丁 - UltimateHorse
///
/// 逆向分析发现 UltimateHorse 没有 GetDamage 方法（旧代码目标有误），
/// 实际诅咒相关方法为：
/// - ExplodeCurse：对全体已诅咒植物造成伤害并移除诅咒
/// - AttributeEvent：有概率对全体植物施加诅咒
/// - OnTriggerStay2D：对范围内植物施加诅咒
///
/// 其中 ExplodeCurse 是造成大规模诅咒伤害的关键入口，
/// AttributeEvent 和 OnTriggerStay2D 通过 EffectManager.SetEffect 施加诅咒，
/// 由 EffectManagerPatch 统一拦截。
/// </summary>
[HarmonyPatch(typeof(UltimateHorse))]
public static class UltimateHorsePatch
{
    /// <summary>
    /// 阻止终极马僵尸的诅咒爆炸伤害
    /// UltimateHorse.ExplodeCurse 遍历所有植物，对带有 PlantCurseEffect 的植物
    /// 调用 Plant.RealTakeDamage(damage) 造成伤害，然后移除诅咒 buff。
    /// 开启诅咒免疫时直接跳过此方法。
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UltimateHorse.ExplodeCurse))]
    public static bool PreExplodeCurse()
    {
        if (!CurseImmunity) return true;

        try
        {
            // 诅咒免疫开启时，跳过 ExplodeCurse 的完整执行
            return false;
        }
        catch
        {
            return true;
        }
    }

    /// <summary>
    /// 备用方案：在 BodyTakeDamage 中也清理诅咒植物列表（与旧逻辑兼容）
    /// Note: UltimateHorse 的诅咒通过 EffectManager.SetEffect 施加，
    /// 由 EffectManagerPatch 统一拦截，此备选仅作为额外安全措施。
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UltimateHorse.BodyTakeDamage))]
    public static void PostBodyTakeDamage(UltimateHorse __instance)
    {
        if (!CurseImmunity) return;
        try
        {
            if (__instance != null)
            {
                Utils.ClearCursedPlants(__instance);
            }
        }
        catch
        {
        }
    }
}
