using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(PlantDataManager))]
public class PlantDataManagerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlantDataManager.CheckIfPlantUnlock))]
    public static void PreCheckIfPlantUnlock(ref bool __result)
    {
        if (EnableAllCards) __result = true;
    }
}