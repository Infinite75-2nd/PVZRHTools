namespace ToolMod.Patches;
/*
[HarmonyPatch(typeof(AbyssManager))]
public static class AbyssManagerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(AbyssManager.ResetAdder))]
    public static void PostResetAdder(AbyssManager __instance)
    {
        OriginalAbyssMaxPlantCount = __instance.maxPlantCount;
        OriginalAbyssMaxSuperCount = __instance.superPlantCount;
        OriginalAbyssMaxUltimateCount = __instance.ultiPlantCount;
        if (AbyssMaxPlantCount >= 0)
        {
            __instance.maxPlantCount = AbyssMaxPlantCount;
        }
        if (AbyssMaxSuperCount >= 0)
        {
            __instance.superPlantCount = AbyssMaxSuperCount;
        }
        if (AbyssMaxUltimateCount >= 0)
        {
            __instance.ultiPlantCount = AbyssMaxUltimateCount;
        }
    }
}*/