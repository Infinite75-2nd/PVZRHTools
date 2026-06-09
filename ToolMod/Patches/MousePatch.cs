using System.Collections.Generic;
using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(Mouse))]
public static class MousePatch
{
    private static Plant? aa = null;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Mouse.TryToSetPlantByGlove))]
    public static bool PreTryToSetPlantByGlove(Mouse __instance)
    {
        if (ColumnGlove)
        {
            aa = __instance.thePlantOnGlove;
            int vcol = __instance.theMouseColumn - __instance.thePlantOnGlove.thePlantColumn;
            int newCol = __instance.theMouseColumn;
            List<Plant> plants = new List<Plant>();
            var allPlants = Lawnf.GetAllPlants();
            if (allPlants != null)
            {
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
            }

            foreach (var plant in plants)
            {
                Plant gameObject =
                    CreatePlant.Instance.SetPlant(newCol, plant.thePlantRow, plant.thePlantType);
                if (Board.Instance.boardTag.isColumn)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        CreatePlant.Instance.SetPlant(__instance.thePlantOnGlove.thePlantColumn, i, plant.thePlantType);
                    }
                }
                else
                {
                    if (gameObject != null)
                    {
                        plant.Die(Plant.DieReason.ByMix);
                    }
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
        {
            if (Board.Instance.boardTag.isColumn && aa != null)
            {
                CreatePlant.Instance.SetPlant(aa.thePlantColumn, aa.thePlantRow, aa.thePlantType);
            }
        }
    }
}