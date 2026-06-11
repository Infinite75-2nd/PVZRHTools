using HarmonyLib;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(Glove))]
public static class GlovePatch
{
    public static float OriginalFullCD { get; set; }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Glove.OnUpdate))]
    public static void PostOnUpdate(Glove __instance)
    {
        try
        {
            if (__instance == null||Board.Instance.boardTag.isShooting) return;
            __instance.gameObject.transform.GetChild(0).gameObject.SetActive(!GloveNoCD);
            __instance.fullCD = GloveFullCD >= 0 ? GloveFullCD : OriginalFullCD;
            if (GloveNoCD) __instance.CD = __instance.fullCD;
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
        OriginalFullCD = __instance.fullCD;
        GameObject obj = new("ModifierGloveCD");
        var text = obj.AddComponent<TextMeshProUGUI>();
        text.font = Resources.Load<TMP_FontAsset>("Fonts/ContinuumBold SDF");
        text.color = new Color(0.5f, 0.8f, 1f);
        obj.transform.SetParent(__instance.GameObject().transform);
        obj.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        obj.transform.localPosition = new Vector3(27.653f, 0, 0);
    }
}