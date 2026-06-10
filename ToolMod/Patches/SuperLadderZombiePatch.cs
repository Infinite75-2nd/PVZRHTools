using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

/// <summary>
/// 诅咒免疫补丁 - SuperLadderZombie.GetDamage
/// 阻止超级梯子僵尸的诅咒效果
/// </summary>
[HarmonyPatch(typeof(Zombie))]
public static class SuperLadderZombiePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(SuperLadderZombie.GetDamage))]
    public static bool PreGetDamage(SuperLadderZombie __instance, ref int theDamage, ref int __result)
    {
        if (!CurseImmunity) return true;
        try
        {
            if (__instance is not SuperLadderZombie) return true;

            _ladderField ??= typeof(SuperLadderZombie).GetField("ladder",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);
            if (_ladderField?.GetValue(__instance) != null)
            {
                return false; // 阻止原方法执行
                __result = theDamage;
                return false;
            }
        }
        catch
        {
        }

        return true;
    }

    private static System.Reflection.FieldInfo? _ladderField;
}