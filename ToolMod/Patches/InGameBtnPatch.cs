using HarmonyLib;
using UnityEngine;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(InGameBtn))]
public static class InGameBtnPatch
{
    public static bool BottomEnabled { get; set; }

    [HarmonyPatch("OnMouseUpAsButton")]
    [HarmonyPostfix]
    public static void PostOnMouseUpAsButton(InGameBtn __instance)
    {
        if (__instance.buttonNumber is 3)
        {
            TimeSlow = !TimeSlow;
            TimeStop = false;
        }

        if (__instance.buttonNumber == 13) BottomEnabled = GameObject.Find("Bottom") != null;
    }
}