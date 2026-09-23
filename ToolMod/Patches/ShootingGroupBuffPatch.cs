using GameLevel.RogueShooting;
using HarmonyLib;
using Il2CppSystem;
using UI;
using UnityEngine.Events;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(ShootingGroupBuff))]
public class ShootingGroupBuffPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ShootingGroupBuff.RegisterGeneralBuff))]
    public static bool PreRegisterGeneralBuff(ShootingGroupBuff __instance, MultipleChoiceMenu menu,
        ShootingManager manager)
    {
        if (!GodEvolutionForceTacticalBuff) return true;
        var available = ShootingGroupBuff.GetAvailableWeights();
        if (available.Count == 0)
            return false;

        menu.RegisterOption(
            "通用战术",
            "从多个选项中自选一种战术词条",
            (UnityAction)(() => menu.actionOnExit += (Action)ShootingGroupBuff.ShowGeneralBuffMenu),
            PlantType.EndoFlame,
            (ZombieType)(-1),
            Quality.diamond);

        var name = available.GetRandomKeyByWeight();
        AdvBuff[] buffs = ShootingGroupBuff.NameToBuffs[name];
        ShootingGroupBuff.RegisterGeneralBuffGroup(menu, buffs[0], buffs[1], buffs[2], buffs[3]);
        return false;
    }
}