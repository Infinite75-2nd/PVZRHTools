using HarmonyLib;
using UnityEngine;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

/// <summary>
/// 超级机枪射手无限开大补丁 - SuperSnowGatling.Update
/// 通过设置 keepShooting = true 使植物持续保持射击状态
/// 同时重置 timer 确保大招持续触发
/// </summary>
[HarmonyPatch(typeof(SuperSnowGatling))]
public static class SuperSnowGatlingPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(SuperSnowGatling.Update))]
    public static void PreUpdate(SuperSnowGatling __instance, out bool __state)
    {
        __state = false;
        if (__instance == null) return;

        int plantId = __instance.GetInstanceID();

        if (UltimateSuperGatling)
        {
            try
            {
                __instance.keepShooting = true;
                ModifiedPlants.Add(plantId);

                // 首次触发：植物未初始化且timer为0时，需要手动触发射击
                if (!InitializedPlants.Contains(plantId))
                {
                    if (__instance.timer <= 0f)
                    {
                        __state = true;
                        InitializedPlants.Add(plantId);
                    }
                }
                // 后续触发：timer即将归零时触发
                else if (__instance.timer > 0 && __instance.timer - Time.deltaTime <= 0f)
                {
                    __state = true;
                }
            }
            catch
            {
            }
        }
        else
        {
            // 功能关闭：恢复被修改过的植物
            if (ModifiedPlants.Contains(plantId))
            {
                try
                {
                    __instance.keepShooting = false;
                    ModifiedPlants.Remove(plantId);
                    InitializedPlants.Remove(plantId);
                }
                catch
                {
                }
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(SuperSnowGatling.Update))]
    public static void PostUpdate(SuperSnowGatling __instance, bool __state)
    {
        if (!UltimateSuperGatling || __instance == null) return;

        try
        {
            __instance.timer = 0.1f;
            if (__state && __instance.anim != null)
            {
                __instance.anim.SetTrigger("shoot");
            }
        }
        catch
        {
        }
    }

    /// <summary>
    /// 清理记录（切换关卡时调用）
    /// </summary>
    public static void ClearAll()
    {
        ModifiedPlants.Clear();
        InitializedPlants.Clear();
    }


    /// <summary>
    /// 超级机枪射手无限开大补丁 - SuperSnowGatling.Shoot1
    /// 在每次射击后立即触发 AttributeEvent 重置大招状态
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(SuperSnowGatling.Shoot1))]
    public static void PostShoot1(SuperSnowGatling __instance)
    {
        if (!UltimateSuperGatling) return;
        try
        {
            if (__instance != null) __instance.AttributeEvent();
        }
        catch
        {
        }
    }
}