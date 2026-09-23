using System.Collections.Generic;
using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(Mouse))]
public static class MousePatch
{
    private static Plant? aa;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Mouse.TryToSetPlantByGlove))]
    public static bool PreTryToSetPlantByGlove(Mouse __instance)
    {
        if (ColumnGlove && __instance.thePlantOnGlove.thePlantRow == __instance.theMouseRow)
        {
            aa = __instance.thePlantOnGlove;
            var vcol = __instance.theMouseColumn - __instance.thePlantOnGlove.thePlantColumn;
            var newCol = __instance.theMouseColumn;
            var plants = new List<Plant>();
            var allPlants = Lawnf.GetAllPlants();
            if (allPlants != null)
                foreach (var plant in allPlants)
                {
                    if (plant == null || plant.gameObject == null) continue;
                    if (plant.thePlantColumn == __instance.thePlantOnGlove.thePlantColumn)
                    {
                        if (plant == __instance.thePlantOnGlove)
                        {
                        }
                        else
                        {
                            if (plant.thePlantType == __instance.thePlantOnGlove.thePlantType)
                                plants.Add(plant);
                        }
                    }
                }

            foreach (var plant in plants)
            {
                var gameObject =
                    CreatePlant.Instance.SetPlant(newCol, plant.thePlantRow, plant.thePlantType);
                if (Board.Instance.boardTag.isColumn)
                {
                    for (var i = 0; i < 5; i++)
                        CreatePlant.Instance.SetPlant(__instance.thePlantOnGlove.thePlantColumn, i, plant.thePlantType);
                }
                else
                {
                    if (gameObject != null) plant.Die(Plant.DieReason.ByMix);
                }
            }
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Mouse.TryToSetPlantByGlove))]
    public static void PostTryToSetPlantByGlove(Mouse __instance)
    {
        if (ColumnGlove)
            if (Board.Instance.boardTag.isColumn && aa != null)
                CreatePlant.Instance.SetPlant(aa.thePlantColumn, aa.thePlantRow, aa.thePlantType);

        OriginalGloveFullCD = Lawnf.GetGloveCD();
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Mouse.TryToSetZombieByCard))]
    public static void PostTryToSetZombieByCard(Mouse __instance)
    {
        OriginalGloveFullCD = Lawnf.GetGloveCD();
    }
}