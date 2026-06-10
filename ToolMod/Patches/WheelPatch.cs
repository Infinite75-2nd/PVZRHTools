using HarmonyLib;
using TMPro;
using Unity.VisualScripting;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(Wheel))]
public static class WheelPatch
{
    public static float OriginalFullCD{get; set;}
    
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Wheel.OnUpdate))]
    public static void PostOnUpdate(Wheel __instance)
    {
        try
        {
            if (__instance == null) return;
            __instance.gameObject.transform.GetChild(0).gameObject.SetActive(!WheelNoCD);
            if (WheelFullCD > 0)
                __instance.fullCD = WheelFullCD;
            else
                __instance.fullCD = OriginalFullCD;
            if (WheelNoCD) __instance.CD = __instance.fullCD;
            var cdChild = __instance.transform.FindChild("ModifierWheelCD");
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
}