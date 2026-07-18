using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Linq;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

/// <summary>
/// 卡片无限制补丁 - TreasureData.GetCardLevel
/// 当启用时，将所有卡片的等级返回为White（最低等级），取消普通卡片"只能带两张"的限制
/// 卡片等级决定了选卡界面中同类型卡片的数量限制：
/// - White(0): 无限制
/// - Green(1) ~ Red(5): 有不同程度的限制
/// </summary>
[HarmonyPatch(typeof(TreasureData))]
public static class TreasureDataPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(TreasureData.GetCardLevel))]
    public static void PostGetCardLevel(ref CardLevel __result)
    {
        // 当启用卡片无限制时，将所有卡片等级设为White（无限制）
        if (UnlimitedCardSlots)
        {
            __result = CardLevel.White;
        }
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(nameof(TreasureData.GetLastPool))]
    public static bool PreGetLastPool(ref List<PlantType> __result)
    {
        if (TreasureAllRedCard&&UnityEngine.Random.RandomRangeInt(0,2) is 0)//50%
        {
            __result = TypeMgr.RedPlant.Cast<IEnumerable<PlantType>>().ToList();
            return false;
        }
        return true;
    }
}