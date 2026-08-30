using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(DriverZombie))]
public static class DriverZombiePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(DriverZombie.PositionUpdate))]
    public static void PostPositionUpdate(DriverZombie __instance)
    {
        if (!NoIceRoad) return;
        try
        {
            if (__instance == null || Board.Instance == null) return;
            foreach (var t in Board.Instance.iceRoads)
            {
                t.fadeTimer = 0;
                t.x = 10;
            }
        }
        catch
        {
        }
    }
}