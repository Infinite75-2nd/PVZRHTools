using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(Plant))]
public static class PlantPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Plant.PlantShootUpdate))]
    public static void PrePlantShootUpdate(Plant __instance)
    {
        // 提前检查开关，避免不必要的 Il2Cpp 对象访问
        if (!FastShooting) return;
        try
        {
            var s = __instance?.TryCast<Shooter>();
            if (s != null) s.AnimShoot();
        }
        catch
        {
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Plant.GetDamage))]
    public static void PostGetDamage(Plant __instance, ref int __result)
    {
        if (HardPlant)
        {
            __result = 0;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Plant.Crashed))]
    public static bool PreCrashed(Plant __instance, int level, int soundID, Zombie zombie)
    {
        // 植物无敌或植物免疫碾压时，阻止碾压
        // 注意：踩踏免疫由 TypeMgrUncrashablePlantPatch 和 ZombieOnTriggerStay2DTramplePatch 处理
        if (HardPlant || CrushImmunity)
        {
            return false;
        }

        return true;
    }


    /// <summary>
    /// 免疫强制扣血补丁 - 通过patch Plant.Die方法来阻止异常死亡
    /// 针对MorePolevaulterZombie等mod中的吞噬效果（直接修改thePlantHealth绕过TakeDamage）
    /// </summary>

    // 记录每个植物上一帧的血量
    private static readonly Dictionary<int, int> LastFrameHealth = new();

    // 记录每个植物是否在本帧通过正常途径受到伤害
    private static readonly HashSet<int> NormalDamageThisFrame = new();

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Plant.Die))]
    public static bool PreDie(Plant __instance)
    {
        if (!ImmuneForceDeduct) return true;
        if (__instance == null) return true;

        try
        {
            var plantId = __instance.GetInstanceID();

            // 如果植物血量还大于0，不应该死亡
            if (__instance.thePlantHealth > 0)
            {
                return true; // 正常死亡流程
            }

            // 检查是否有缓存的血量
            if (LastFrameHealth.TryGetValue(plantId, out var lastHealth))
            {
                // 如果上一帧血量很高，但现在突然死亡，可能是强制扣血
                // 恢复血量并阻止死亡
                if (lastHealth > __instance.thePlantMaxHealth * 0.3f)
                {
                    __instance.thePlantHealth = lastHealth;
                    __instance.UpdateText();
                    return false; // 阻止死亡
                }
            }
        }
        catch
        {
        }

        return true;
    }

    /// <summary>
    /// 更新植物血量缓存（在PatchMgr.Update中调用）
    /// </summary>
    public static void UpdateHealthCache()
    {
        if (!ImmuneForceDeduct)
        {
            if (LastFrameHealth.Count > 0)
                LastFrameHealth.Clear();
            return;
        }

        try
        {
            var allPlants = Lawnf.GetAllPlants();
            if (allPlants == null) return;

            // 收集当前存活植物的ID
            var alivePlantIds = new HashSet<int>();
            foreach (var p in allPlants)
            {
                if (p != null)
                    alivePlantIds.Add(p.GetInstanceID());
            }

            // 清理已死亡植物的缓存
            var deadPlantIds = LastFrameHealth.Keys.Where(id => !alivePlantIds.Contains(id)).ToList();
            foreach (var id in deadPlantIds)
                LastFrameHealth.Remove(id);

            // 更新缓存
            foreach (var plant in allPlants)
            {
                if (plant == null) continue;
                var plantId = plant.GetInstanceID();

                // 只有当植物血量大于0时才更新缓存
                if (plant.thePlantHealth > 0)
                {
                    LastFrameHealth[plantId] = plant.thePlantHealth;
                }
            }
        }
        catch
        {
        }
    }
}