using HarmonyLib;
using ToolMod.Components;
using Unity.VisualScripting;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(PlantDataMenu))]
public static class PlantDataMenuPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlantDataMenu.Start))]
    public static void PostStart(PlantDataMenu __instance)
    {
        if(__instance.plant.board.boardTag.superCustomEditorMode)return;

        foreach (var info in __instance.infoText)
        {
            info.gameObject.active = false;
        }
        __instance.AddComponent<PlantStatisticsModifier>();
    }
}