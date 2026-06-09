using HarmonyLib;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(PauseMenu_Btn))]
public static class PauseMenu_BtnPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PauseMenu_Btn.OnMouseUp))]
    public static void PostAwake(PauseMenu_Btn __instance)
    {
        if (!ModCore.Instance.Inited && __instance.buttonNumber is 10)
        {
            ModCore.Instance.LateInit();
        }
    }
}