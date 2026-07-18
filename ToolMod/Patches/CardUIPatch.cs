using HarmonyLib;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(CardUI))]
public static class CardUIPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(CardUI.Start))]
    public static void PostStart(CardUI __instance)
    {
        GameObject obj = new("ModifierCardCD");
        var text = obj.AddComponent<TextMeshProUGUI>();
        text.font = Resources.Load<TMP_FontAsset>("Fonts/ContinuumBold SDF");
        text.color = new Color(0.5f, 0.8f, 1f);
        obj.transform.SetParent(__instance.transform);
        obj.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        obj.transform.localPosition = new Vector3(39f, 0, 0);

        // 卡片无限制：将maxUsedTimes设置为一个很大的值
        if (UnlimitedCardSlots)
        {
            __instance.maxUsedTimes = 9999;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(CardUI.Update))]
    public static void PostUpdate(CardUI __instance)
    {
        try
        {
            if (__instance is null) return;

            // 卡片无限制：动态检查并设置maxUsedTimes
            if (UnlimitedCardSlots && __instance.maxUsedTimes < 9999)
            {
                __instance.maxUsedTimes = 9999;
            }
            if(CardFreeCD)
            {
                __instance.CD = __instance.fullCD;
            }

            var child = __instance.transform.FindChild("ModifierCardCD");
            if (child == null) return;
            if (__instance.isAvailable || !ShowGameInfo)
            {
                child.GameObject().active = false;
            }
            else
            {
                child.GameObject().active = true;
                child.GameObject().GetComponent<TextMeshProUGUI>().text = $"{__instance.CD:N1}/{__instance.fullCD}";
            }
        }
        catch
        {
        }
    }

    /// <summary>
    /// 卡片无限制补丁 - CardUI.LevelLim
    /// 当启用时，阻止LevelLim方法执行，取消卡片选取数量限制
    /// LevelLim方法是在CardUI.Start中被调用来设置卡片的选取限制
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(CardUI.LevelLim))]
    public static bool PreLevelLim()
    {
        // 当启用卡片无限制时，阻止LevelLim方法执行
        if (UnlimitedCardSlots)
        {
            return false;
        }

        return true;
    }


    /// <summary>
    /// 卡片无限制补丁 - CardUI.OnMouseDown
    /// 当点击选取卡片时，复制一张新卡片
    /// </summary>

    // 记录复制出来的卡片，用于退出选卡时清除
    [HarmonyPostfix]
    [HarmonyPatch(nameof(CardUI.OnMouseDown))]
    public static void PostOnMouseDown(CardUI __instance)
    {
        if (!UnlimitedCardSlots) return;

        try
        {
            // 只在选卡界面（卡片被选中时）复制
            if (!__instance.isSelected) return;

            // 检查父对象是否存在
            if (__instance.transform.parent == null) return;

            // 复制卡片对象
            GameObject go = Object.Instantiate(__instance.gameObject, __instance.transform.parent);
            go.transform.position = __instance.transform.position;

            // 设置新卡片的CD
            var newCard = go.GetComponent<CardUI>();
            if (newCard != null)
            {
                newCard.CD = newCard.fullCD;
                newCard.isSelected = false; // 新卡片未被选中
            }

            // 记录复制的卡片
            CopiedCards.Add(go);
        }
        catch
        {
        }
    }

    /// <summary>
    /// 卡片无限制补丁 - CardUI.Awake
    /// 当启用时，将maxUsedTimes设置为一个很大的值，取消卡片使用次数限制
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(CardUI.Awake))]
    public static void PostAwake(CardUI __instance)
    {
        // 卡片无限制：将maxUsedTimes设置为一个很大的值
        if (UnlimitedCardSlots)
        {
            __instance.maxUsedTimes = 9999;
        }
    }

    /// <summary>
    /// 清除所有复制的卡片（关闭功能时调用）
    /// </summary>
    public static void ClearAllCopiedCards()
    {
        try
        {
            foreach (var card in CopiedCards)
            {
                if (card != null)
                {
                    Object.Destroy(card);
                }
            }

            CopiedCards.Clear();
        }
        catch
        {
        }
    }
}