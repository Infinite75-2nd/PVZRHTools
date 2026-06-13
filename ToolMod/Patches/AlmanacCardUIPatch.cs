using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(AlmanacCardUI))]
public static class AlmanacCardUIPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(AlmanacCardUI.OnPointerDown))]
    public static void PostOnPointerDown(AlmanacCardUI __instance)
    {
        try
        {
            // 获取菜单名称来判断是植物还是僵尸图鉴
            string menuName = __instance.menu?.name ?? "";

            var plantId = __instance.PlantType;
            var zombieId = __instance.ZombieType;

            if (menuName.Contains("Plant"))
            {
                AlmanacSeedType = plantId;
            }
            else if (menuName.Contains("Zombie"))
            {
                AlmanacZombieType = zombieId;
            }
            else
            {
                // 备用判断：根据ID值判断
                if (plantId > 0)
                {
                    AlmanacSeedType = plantId;
                }
                else if (zombieId > 0)
                {
                    AlmanacZombieType = zombieId;
                }
            }
        }
        catch
        {
        }
    }
}