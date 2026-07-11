using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(AbyssSwordStar))]
public static class AbyssSwordStarPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("Awake")]
    public static void PreAwake(ref GameStatus __state)
    {
        __state = GameAPP.theGameStatus;
        if (UnlockRedCardPlants)
        {
            GameAPP.theGameStatus = (GameStatus)(-1);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("Awake")]
    public static void PostAwake(ref GameStatus __state)
    {
        GameAPP.theGameStatus = __state;
    }

    [HarmonyPrefix]
    [HarmonyPatch("Awake")]
    public static void PreStart(ref LevelType __state)
    {
        __state = GameAPP.theBoardType;
        if (UnlockRedCardPlants)
        {
            GameAPP.theBoardType = LevelType.AbyssRealm;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("Awake")]
    public static void PostStart(ref LevelType __state)
    {
        GameAPP.theBoardType = __state;
    }
}