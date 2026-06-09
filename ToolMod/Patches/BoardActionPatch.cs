using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

/// <summary>
/// 禁用全屏冰冻特效的 Harmony 补丁
/// 拦截 Board.CreateFreeze 全屏冰冻特效，同时为全场僵尸添加冻结效果并造成伤害，为雪原植物恢复充能
/// </summary>
[HarmonyPatch(typeof(BoardAction))]
public static class BoardActionPatch
{
    // 雪原植物类型ID列表（从反汇编代码中提取）
    // 38: SnowPea, 913: ?, 925: ?, 947: ?, 1039: ?, 1218-1220: ?, 1227: ?, 1259: ?
    private static readonly HashSet<int> SnowPlantTypes = new HashSet<int>
    {
        38, // SnowPea
        913, // 
        925, // 
        947, // 
        1039, // 
        1218, 1219, 1220, // 
        1227, // 
        1259 // 
    };

    /// <summary>
    /// 拦截 Board.CreateFreeze 方法，阻止全屏冰冻特效
    /// 同时为全场僵尸添加冻结效果并造成伤害，为雪原植物恢复充能
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(BoardAction.CreateFreeze))]
    public static bool PreCreateFreeze(BoardAction __instance, Vector2 pos)
    {
        // 功能关闭时，执行原版逻辑
        if (!DisableIceEffect)
            return true;

        // 为全场僵尸添加冻结效果
        ApplyFreezeToAllZombies(__instance?.board);

        return false; // 阻止全屏冰冻特效
    }

    /// <summary>
    /// 为全场非魅惑僵尸添加冻结效果并造成伤害，同时为雪原植物恢复充能
    /// 魅惑僵尸（友方单位）将被跳过，既不冻结也不伤害
    /// </summary>
    private static void ApplyFreezeToAllZombies(Board board)
    {
        try
        {
            const int damageAmount = 20; // 伤害值：20点
            const int chargeAmount = 14; // 充能值：14点（与原版一致）

            // 遍历所有僵尸
            foreach (var zombie in Board.Instance.zombieArray)
            {
                if (zombie != null && zombie.gameObject.activeInHierarchy)
                {
                    // 跳过魅惑僵尸（友方单位）
                    if (zombie.isMindControlled)
                        continue;

                    // 为非魅惑僵尸添加冻结效果
                    zombie.SetFreeze(4f); // 冻结4秒
                    // 对非魅惑僵尸造成伤害
                    zombie.ApplyDamage(DamageType.Normal, damageAmount);
                }
            }

            // 为全场雪原植物恢复充能
            var allPlants = Lawnf.GetAllPlants();
            if (allPlants != null)
            {
                foreach (var plant in allPlants)
                {
                    if (plant != null && plant.gameObject.activeInHierarchy)
                    {
                        // 检查是否为雪原植物（使用 TypeMgr.IsSnowPlant 或检查植物类型ID）
                        int plantTypeId = (int)plant.thePlantType;
                        if (TypeMgr.IsSnowPlant(plant.thePlantType) || SnowPlantTypes.Contains(plantTypeId))
                        {
                            try
                            {
                                // 直接增加 attributeCount 属性（与原版 Board.CreateFreeze 一致）
                                plant.attributeCount += chargeAmount;

                                // 调用 UpdateText 方法更新显示
                                plant.UpdateText();
                            }
                            catch
                            {
                                // 忽略充能失败
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // 忽略错误
        }
    }
}