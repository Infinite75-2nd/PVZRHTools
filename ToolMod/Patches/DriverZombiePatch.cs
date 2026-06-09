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
            for (var i = 0; i < Board.Instance.iceRoads.Count; i++)
                if (Board.Instance.iceRoads[i].theRow == __instance.theZombieRow)
                    Board.Instance.iceRoads[i].fadeTimer = 0;
        }
        catch
        {
        }
    }
}