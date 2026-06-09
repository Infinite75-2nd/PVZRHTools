using HarmonyLib;
using UnityEngine;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(FruitObject))]
public static class FrFruitObjectPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(FruitObject.FixedUpdate))]
    public static void PostFixedUpdate(FruitObject __instance)
    {
        if (!AutoCutFruit) return;
        try
        {
            if (__instance == null || __instance.gameObject == null) return;
            __instance.gameObject.TryGetComponent<Rigidbody2D>(out var rb);
            if (rb != null)
            {
                float screenHeight = Camera.main.orthographicSize;
                if (__instance.transform.position.y < -screenHeight && rb.velocity.y < 0f)
                {
                    __instance.Slice();
                }
            }
        }
        catch
        {
        }
    }
}