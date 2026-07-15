using HarmonyLib;
using ZenGarden;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(GardenData))]
public static class GardenDataPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GardenData.GetData))]
    public static void PostGetData(ref GardenUnifiedData __result)
    {
        if (__result == null)
        {
            __result = new GardenUnifiedData
            {
                inventory = new Inventory()
            };
            GardenData.MigrateToUnifiedData();
        }
    }
}