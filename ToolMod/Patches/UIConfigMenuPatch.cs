using HarmonyLib;
using ToolMod.Components;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(UIConfigMenu))]
public static class UIConfigMenuPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIConfigMenu.OnExit))]
    public static bool PreOnExit(UIConfigMenu __instance) => !__instance.TryGetComponent<KeyBindingUI>(out _);
}