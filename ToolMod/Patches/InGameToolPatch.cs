using HarmonyLib;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(InGameTool))]
public static class InGameToolPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(InGameTool.Start))]
    public static void PostStart(InGameTool __instance)
    {
        if (__instance.TryCast<Wheel>())
        {
            WheelPatch.OriginalFullCD = __instance.fullCD;
            GameObject obj = new("ModifierWheelCD");
            var text = obj.AddComponent<TextMeshProUGUI>();
            text.font = Resources.Load<TMP_FontAsset>("Fonts/ContinuumBold SDF");
            text.color = new Color(0.5f, 0.8f, 1f);
            obj.transform.SetParent(__instance.GameObject().transform);
            obj.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            obj.transform.localPosition = new Vector3(25, 0, 0);
        }
    }
}