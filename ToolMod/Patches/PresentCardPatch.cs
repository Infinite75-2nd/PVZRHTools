using HarmonyLib;
using UnityEngine;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

/// <summary>
/// 卡片无限制补丁 - PresentCard.Start
/// 当启用时，阻止PresentCard.Start()方法执行，取消礼盒卡片的数量限制
/// 参考：AllPresentCard插件
/// </summary>
[HarmonyPatch(typeof(PresentCard))]
public static class PresentCardPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(PresentCard.Start))]
    public static bool PreStart(PresentCard __instance)
    {
        // 当启用卡片无限制时，阻止Start方法执行，取消卡片数量限制
        // 注意：这里直接销毁PresentCard组件，而不是阻止Start方法执行
        // 这样可以确保在任何时候启用"卡片无限制"功能都能生效
        if (UnlimitedCardSlots)
        {
            Object.Destroy(__instance);
            return false;
        }

        return true;
    }
}

/// <summary>
/// 卡片无限制补丁 - CardUI.Awake
/// 当启用时，将maxUsedTimes设置为一个很大的值，取消卡片使用次数限制
/// </summary>
[HarmonyPatch(typeof(CardUI), "Awake")]
public static class UnlimitedCardAwakePatch
{
    [HarmonyPostfix]
    public static void Postfix(CardUI __instance)
    {
        // 卡片无限制：将maxUsedTimes设置为一个很大的值
        if (UnlimitedCardSlots)
        {
            __instance.maxUsedTimes = 9999;
        }
    }
}