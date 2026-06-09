using HarmonyLib;
using static ToolMod.Components.PatchDataCache;


namespace ToolMod.Patches;

[HarmonyPatch(typeof(SolarSunflower))]
public static class SolarSunflowerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("Start")]
    public static void PreStart(ref Board.BoardTag __state)
    {
        __state = Board.Instance.boardTag;
        if (UnlockRedCardPlants)
        {
            Board.BoardTag boardTag = Board.Instance.boardTag;
            boardTag.isTreasure = true;
            Board.Instance.boardTag = boardTag;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    public static void PostStart(ref Board.BoardTag __state)
    {
        Board.Instance.boardTag = __state;
    }
}