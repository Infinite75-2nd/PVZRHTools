using System;
using GameLevel.RogueShooting;
using HarmonyLib;
using static ToolMod.Components.PatchDataCache;
using static ToolMod.Utils;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(MultipleChoiceMenu))]
public static class MultipleChoiceMenuRefreshPatch
{
    private static System.Reflection.FieldInfo? _refreshCountField;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(MultipleChoiceMenu.SetRefreshable), typeof(bool), typeof(int), typeof(bool), typeof(bool))]
    public static void PrefixSetRefreshable(ref int refreshCount, ref bool interactable)
    {
        if (!ShouldFixGodEvolutionRefreshButton) return;
        refreshCount = GetGodEvolutionMenuRefreshCount();
        interactable = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(MultipleChoiceMenu.SetRefreshable), typeof(bool), typeof(int), typeof(bool), typeof(bool))]
    public static void PostfixSetRefreshable(MultipleChoiceMenu __instance)
    {
        if (!ShouldFixGodEvolutionRefreshButton) return;
        try
        {
            if (__instance?.refreshButton != null)
                __instance.refreshButton.Interactable = true;
        }
        catch { }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(MultipleChoiceMenu.Refresh))]
    public static void PrefixRefresh(MultipleChoiceMenu __instance)
    {
        if (!ShouldFixGodEvolutionRefreshButton) return;
        try
        {
            _refreshCountField ??= typeof(MultipleChoiceMenu).GetField("refreshCount",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            _refreshCountField?.SetValue(__instance, GetGodEvolutionMenuRefreshCount());
        }
        catch { }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(MultipleChoiceMenu.Refresh))]
    public static void PostfixRefresh(MultipleChoiceMenu __instance)
    {
        if (!ShouldFixGodEvolutionRefreshButton) return;
        try
        {
            _refreshCountField ??= typeof(MultipleChoiceMenu).GetField("refreshCount",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var count = GetGodEvolutionMenuRefreshCount();
            _refreshCountField?.SetValue(__instance, count);
            if (__instance?.refreshButton != null)
                __instance.refreshButton.Interactable = true;
        }
        catch{}
    }


}