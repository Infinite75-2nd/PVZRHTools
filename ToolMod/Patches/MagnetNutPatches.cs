using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

/// <summary>
/// 磁力坚果无限吸引补丁 - 取消100个子弹存储限制
/// </summary>
[HarmonyPatch(typeof(MagnetNut))]
public static class MagnetNutPatches
{
    /// <summary>
    /// 补丁 FixedUpdate 方法，取消子弹存储上限（100个限制）
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(MagnetNut.FixedUpdate))]
    public static bool PreFixedUpdate(MagnetNut __instance)
    {
        if (!MagnetNutUnlimited) return true;

        try
        {
            if (__instance == null) return true;
            // 强制调用 SearchBullet，无视100个子弹限制
            __instance.SearchBullet();
            return true;
        }
        catch
        {
            return true;
        }
    }
}