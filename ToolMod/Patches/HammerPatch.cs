using HarmonyLib;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(Hammer))]
public static class HammerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Hammer.OnUpdate))]
    public static void PostUpdate(Hammer __instance)
    {
        try
        {
            if (__instance == null) return;
            __instance.gameObject.transform.GetChild(0).GetChild(0).gameObject.SetActive(!HammerNoCD);
            __instance.fullCD = HammerFullCD > 0 ? HammerFullCD : OriginalHammerFullCD;
            if (HammerNoCD) __instance.CD = __instance.fullCD;
            var cdChild = __instance.transform.FindChild("ModifierHammerCD");
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
    [HarmonyPatch(nameof(Hammer.Start))]
    public static void PostStart(Hammer __instance)
    {
        OriginalHammerFullCD = __instance.fullCD;
        GameObject obj = new("ModifierHammerCD");
        var text = obj.AddComponent<TextMeshProUGUI>();
        text.font = Resources.Load<TMP_FontAsset>("Fonts/ContinuumBold SDF");
        text.color = new Color(0.5f, 0.8f, 1f);
        obj.transform.SetParent(__instance.GameObject().transform);
        obj.transform.localScale = new Vector3(2f, 2f, 2f);
        obj.transform.localPosition = new Vector3(107, 0, 0);
    }
}