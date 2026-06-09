using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(CreatePlant))]
public static class CreatePlantPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(CreatePlant.SetPlant))]
    public static void PreSetPlant(ref bool isFreeSet)
    {
        isFreeSet = FreePlanting || isFreeSet;
    }
}