using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

/// <summary>
/// 诅咒免疫补丁 - EffectManager.SetEffect
/// 阻止所有来源的诅咒效果（EffectType 103 = PlantCurseEffect）施加到植物上
///
/// 诅咒机制分析（IDA逆向结果）：
/// - 诅咒使用 EffectType 103 (PlantCurseEffect)
/// - PlantCurseEffect.OnUpdate 随时间累积诅咒值，达到植物生命值时触发 Die()
/// - UltimateHorse.ExplodeCurse 遍历所有植物，对带诅咒的植物造成伤害
/// - BlackJackbox.CursePlant → SetEffect(plant, 103, 1.0, 10.0, 0)
/// - Bullet_ironPea_curse.HitPlant → SetEffect(plant, 103, 1.0, 0.1, 0)
/// - UltimateHorse.AttributeEvent → SetEffect(plant, 103, 1.0, 0.2, 0)
/// - UltimateHorse.OnTriggerStay2D → SetEffect(plant, 103, 1.0, 10.0, 0)
/// - BlackHorse.CrashEntity → SetEffect(plant, 103, 1.0, 1.0/5.0, 0)
/// - SuperBlackHorse.CrashEntity → SetEffect(plant, 103, 1.0, 1.0, 0)
/// - SuperLadderZombie.AnimSetLadder → SetEffect(plant, 103, 1.0, 10.0, 0)
///
/// 注意：EffectManager.SetEffect 有两个重载（Plant 和 Zombie 版本），
/// 必须显式指定参数类型来消除歧义，否则 Harmony 会报 AmbiguousMatchException。
/// </summary>
[HarmonyPatch(typeof(EffectManager))]
public static class EffectManagerPatch
{
    /// <summary>
    /// 诅咒对应的 EffectType 枚举值（PlantCurseEffect）
    /// </summary>
    private static readonly EffectType CurseEffectType = (EffectType)103;

    /// <summary>
    /// 拦截 EffectManager.SetEffect(Plant, EffectType, float, float)
    /// 当目标为诅咒效果且诅咒免疫开启时阻止。
    ///
    /// 显式指定参数类型消除 SetEffect(Plant, ...) 和 SetEffect(Zombie, ...) 的重载歧义。
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(EffectManager.SetEffect), typeof(Plant), typeof(EffectType), typeof(float), typeof(float))]
    public static bool PreSetEffect(Plant plant, EffectType effectType, float duration, float value)
    {
        if (!CurseImmunity) return true;

        // EffectType 103 = PlantCurseEffect（诅咒）
        if (effectType == CurseEffectType)
        {
            return false; // 阻止诅咒效果施加
        }

        return true;
    }
}
