using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(CreateZombie))]
public static class CreateZombiePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(CreateZombie.SetZombie))]
    public static void PostSetZombie(ref Zombie __result)
    {
        if (__result != null && ZombieHealthMultiplier > 0)
        {
            __result.theHealth = (int)(__result.theHealth * ZombieHealthMultiplier);
            __result.theFirstArmorHealth = (int)(__result.theFirstArmorHealth * ZombieHealthMultiplier);
            __result.theSecondArmorHealth = (int)(__result.theSecondArmorHealth * ZombieHealthMultiplier);
            __result.theMaxHealth = (int)(__result.theMaxHealth * ZombieHealthMultiplier);
            __result.theFirstArmorMaxHealth = (int)(__result.theFirstArmorMaxHealth * ZombieHealthMultiplier);
            __result.theSecondArmorMaxHealth = (int)(__result.theSecondArmorMaxHealth * ZombieHealthMultiplier);
        }
    }
}