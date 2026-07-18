namespace ToolMod.Patches;
/*
[HarmonyPatch(typeof(AbyssSelectBuffMenu))]
public static class AbyssSelectBuffMenuPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(AbyssSelectBuffMenu.Refresh))]
    public static void PostRefresh(AbyssSelectBuffMenu __instance)
    {
        if (AbyssLimitlessRefresh)
        {
            __instance.refreshCount = 2;
        }
    }
}*/