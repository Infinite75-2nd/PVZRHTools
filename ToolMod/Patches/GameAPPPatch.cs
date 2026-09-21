using HarmonyLib;

namespace ToolMod.Patches;

//[HarmonyPatch(typeof(GameAPP))]
public class GameAPPPatch
{
    //[HarmonyPostfix]
    //[HarmonyPatch(nameof(GameAPP.Start))]
    public static void PostStart()
    {
        if (!ModCore.Instance.Inited)
        {
            ModCore.Instance.LateInit();
        }
    }
}