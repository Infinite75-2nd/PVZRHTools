using HarmonyLib;
using static ToolMod.Components.PatchDataCache;


namespace ToolMod.Patches;

[HarmonyPatch(typeof(Bucket))]
public static class BucketPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Bucket.Update))]
    public static void PostUpdate(Bucket __instance)
    {
        if (!ItemExistForever) return;
        try
        {
            if (__instance != null) __instance.existTime = 0.1f;
        }
        catch
        {
        }
    }
}