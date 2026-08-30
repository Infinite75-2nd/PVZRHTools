using HarmonyLib;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(Glove))]
public static class GlovePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Glove.OnUpdate))]
    public static void PostOnUpdate(Glove __instance)
    {
        try
        {
            if (__instance == null || Board.Instance.boardTag.isShooting) return;
            __instance.gameObject.transform.GetChild(0).gameObject.SetActive(!GloveNoCD);

            // 仅当主动开启"无CD"或"自定义全CD"时才覆盖 fullCD；
            // 否则交由游戏按模式实时计算，避免把陈旧的 OriginalGloveFullCD(可能为0)写回导致错误无CD
            if (GloveNoCD || GloveFullCD >= 0)
            {
                __instance.fullCD = GloveFullCD >= 0 ? GloveFullCD : OriginalGloveFullCD;
                if (GloveNoCD) __instance.CD = __instance.fullCD;
            }
            var cdChild = __instance.transform.FindChild("ModifierGloveCD");
            if (cdChild == null) return;
            if (__instance.avaliable || !ShowGameInfo)
            {
                cdChild.GameObject().active = false;
            }
            else
            {
                cdChild.GameObject().active = true;
                cdChild.GameObject().GetComponent<TextMeshProUGUI>().text =
                    $"{__instance.CD:N1}/{__instance.fullCD}";
            }
        }
        catch
        {
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Glove.Start))]
    public static void PostStart(Glove __instance)
    {
        // 采集开局基线，避免 OriginalGloveFullCD 为 0 导致错误无CD
        OriginalGloveFullCD = __instance.fullCD;
        GameObject obj = new("ModifierGloveCD");
        var text = obj.AddComponent<TextMeshProUGUI>();
        text.font = Resources.Load<TMP_FontAsset>("Fonts/ContinuumBold SDF");
        text.color = new Color(0.5f, 0.8f, 1f);
        obj.transform.SetParent(__instance.GameObject().transform);
        obj.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        obj.transform.localPosition = new Vector3(27.653f, 0, 0);
    }
}