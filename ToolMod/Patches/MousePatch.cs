using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using ToolMod.Components;
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
        if (ColumnGlove&&__instance.thePlantOnGlove.thePlantRow==__instance.theMouseRow)
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

        OriginalGloveFullCD = Lawnf.GetGloveCD();
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Mouse.TryToSetZombieByCard))]
    public static void PostTryToSetZombieByCard(Mouse __instance)
    {
        OriginalGloveFullCD = Lawnf.GetGloveCD();
    }

    // 图鉴种植 / 星辉 / 植物升级的点击处理必须放在 Mouse.Update 之后：此时
    // theMouseRow/Column 已更新到当帧，否则读到上一帧的行列（表现为要点两下才生效）。
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Mouse.Update))]
    public static void PostUpdate(Mouse __instance)
    {
        try
        {
            bool almanac = (AlmanacPlacePlant && AlmanacSeedType is not PlantType.Nothing) ||
                           (AlmanacPlaceZombie && AlmanacZombieType is not ZombieType.Nothing);
            if (!almanac && !PatchDataCache.StarUpBuff && !PatchDataCache.PlantUpgrade) return;
            if (Board.Instance == null) return;
            if (!Input.GetMouseButtonDown(0)) return;
            if (!Components.ToolsUpdater.IsMouseInsideGrid()) return;

            int column = __instance.theMouseColumn;
            int row = __instance.theMouseRow;

            // 图鉴种植优先（不需要格子已有植物）
            if (AlmanacPlacePlant && AlmanacSeedType is not PlantType.Nothing)
            {
                if (CreatePlant.Instance != null)
                    CreatePlant.Instance.SetPlant(column, row, AlmanacSeedType);
                return;
            }

            if (AlmanacPlaceZombie && AlmanacZombieType is not ZombieType.Nothing)
            {
                if (CreateZombie.Instance != null)
                {
                    if (AlmanacZombieMindCtrl)
                        CreateZombie.Instance.SetZombieWithMindControl(row, AlmanacZombieType, __instance.mouseX);
                    else
                        CreateZombie.Instance.SetZombie(row, AlmanacZombieType, __instance.mouseX);
                }
                return;
            }

            if (!PatchDataCache.StarUpBuff && !PatchDataCache.PlantUpgrade) return;

            var plants = Lawnf.Get1x1Plants(column, row);
            if (plants == null || plants.Count == 0) return;
            var plant = plants[0];
            if (plant == null) return;

            if (PatchDataCache.StarUpBuff && !plant.isCrashed && plant.thePlantHealth > 0)
                Utils.ApplyStarUpBuff(plant);

            // 升级：Android 无右键，改用左键点击（对齐 hook 版 mod_handle_grid_click）
            if (PatchDataCache.PlantUpgrade && plant.theLevel < 3)
                plant.Upgrade(plant.theLevel + 1, true, false);
        }
        catch
        {
        }
    }
}