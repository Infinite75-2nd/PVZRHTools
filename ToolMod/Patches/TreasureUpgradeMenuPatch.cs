using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(TreasureUpgradeMenu))]
public static class TreasureUpgradeMenuPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(TreasureUpgradeMenu.UpgradeWareHouse))]
    public static bool PreUpgradeWareHouse(TreasureUpgradeMenu __instance)
    {
        if (TreasureFreeUpgrade)
        {
            if (TreasureData.wareHouseLevel >= 5)
            {
                Core.InGameText.Instance.ShowText("仓库已升满", 3f);
                GameAPP.PlaySound(26); // 错误提示音
                return false;
            }
            Core.InGameText.Instance.ShowText("成功升级仓库", 3f);
            GameAPP.PlaySound(125); // 成功音效

            // 仓库等级+1，保存数据，刷新UI
            TreasureData.wareHouseLevel++;
            SaveInfo.Instance.SavePlayerData();
            __instance.InitWareHouseUpgrade();
            return false;
        }
        return true;
    }
    
}