using Core;
using HarmonyLib;
using static ToolMod.Utils;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(InGameText))]
public static class InGameTextPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(InGameText.ShowText))]
    public static void PostShowText()
    {
        try
        {
            var travelMgr = ResolveTravelMgr(autoCreate: true);
            if (travelMgr == null) return;
            SyncInGameBuffs();
        }
        catch (System.Exception ex)
        {
            ModCore.Instance.Log?.LogError($"[PVZRHTools] InGameTextPatch 异常: {ex.Message}\n{ex.StackTrace}");
        }
    }
}