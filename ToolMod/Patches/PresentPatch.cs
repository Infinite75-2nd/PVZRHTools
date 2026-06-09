using HarmonyLib;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(Present))]
public static class PresentPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Present.RandomPlant))]
    public static bool PreRandomPlant(Present __instance)
    {
        if (LockPresent >= 0)
        {
            CreatePlant.Instance.SetPlant(__instance.thePlantColumn, __instance.thePlantRow, (PlantType)LockPresent);
            if (CreatePlant.Instance.IsPuff((PlantType)LockPresent))
            {
                CreatePlant.Instance.SetPlant(__instance.thePlantColumn, __instance.thePlantRow,
                    (PlantType)LockPresent);
                CreatePlant.Instance.SetPlant(__instance.thePlantColumn, __instance.thePlantRow,
                    (PlantType)LockPresent);
            }

            return false;
        }

        if (SuperPresent)
        {
            __instance.SuperRandomPlant();
            return false;
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Present.Start))]
    public static void PostStart(Present __instance)
    {
        if (PresentFastOpen && (int)__instance.thePlantType != 245) __instance.AnimEvent();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Present.AnimEvent))]
    public static bool PreAnimEvent(Present __instance)
    {
        // 检查是否是PvE布阵的礼盒（第3行，第1-5列）
        if (__instance.thePlantRow is 2)
        {
            var lockPlantType = __instance.thePlantColumn switch
            {
                0 => Present1PlantIndex,
                1 => Present2PlantIndex,
                2 => Present3PlantIndex,
                3 => Present4PlantIndex,
                4 => Present5PlantIndex,
                _ => PlantType.Nothing
            };

            if (lockPlantType >= 0)
            {
                var col = __instance.thePlantColumn;
                var row = __instance.thePlantRow;
                var pos = __instance.transform.position;

                // 创建粒子效果
                CreateParticle.SetParticle(11, pos, row);

                // 先销毁礼盒，释放位置
                __instance.Die();

                // 再创建指定植物
                CreatePlant.Instance.SetPlant(col, row, lockPlantType);
                if (CreatePlant.Instance.IsPuff(lockPlantType))
                {
                    CreatePlant.Instance.SetPlant(col, row, lockPlantType);
                    CreatePlant.Instance.SetPlant(col, row, lockPlantType);
                }

                return false; // 阻止原始AnimEvent执行
            }
        }

        return true; // 继续执行原始AnimEvent
    }
}