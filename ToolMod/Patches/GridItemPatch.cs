using HarmonyLib;
using static ToolMod.Components.PatchDataCache;


namespace ToolMod.Patches;

[HarmonyPatch(typeof(GridItem))]
public static class GridItemPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GridItem.SetGridItem))]
    public static bool Prefix(ref GridItemType theType)
    {
        return (int)theType >= 3 || !NoHole;
    }
}