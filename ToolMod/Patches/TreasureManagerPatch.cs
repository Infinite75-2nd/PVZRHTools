using HarmonyLib;
using ToolMod.Components;
using static ToolMod.Components.PatchDataCache;
using static ToolMod.Utils;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(TreasureManager))]
public static class TreasureManagerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(TreasureManager.Update))]
    public static void PostUpdate(TreasureManager __instance)
    {
        if (TreasureMaxTime > 0)
        {
            __instance.maxTimer = TreasureMaxTime;
        }
        else
        {
            __instance.maxTimer = OriginalTreasureMaxTime;
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(nameof(TreasureManager.Awake))]
    public static void PostAwake(TreasureManager __instance)
    {
        OriginalTreasureMaxTime=__instance.maxTimer;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(nameof(TreasureManager.Evacuate))]
    public static bool PreEvacuate(TreasureManager __instance)
    {
        if (TreasureFreeWithdraw)
        {
            var uiManager = GameAPP.UIManager;
            if (uiManager != null)
            {
                var menu = GameAPP.UIManager.Push((UIType)51, GameAPP.canvasUp).Cast<TreasureEvacuateMenu>();
                if (menu != null)
                {
                    menu.manager = __instance;
                }
            }
            return false;
        }
        return true;
    }
    
}