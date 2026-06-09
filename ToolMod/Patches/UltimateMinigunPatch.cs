using HarmonyLib;
using static ToolMod.Components.PatchDataCache;


namespace ToolMod.Patches;

[HarmonyPatch(typeof(UltimateMinigun), MethodType.Constructor)]
public static class UltimateMinigunPatch
{
    [HarmonyPrefix]
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
    public static void PostStart(ref Board.BoardTag __state)
    {
        Board.Instance.boardTag = __state;
    }
}