using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(Zombie))]
public static class ZombiePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Zombie.Start))]
    public static void PostStart(Zombie __instance)
    {
        try
        {
            if (ZombieHP[__instance.theZombieType] >= 0)
            {
                __instance.theMaxHealth = ZombieHP[__instance.theZombieType];
                __instance.theHealth = __instance.theMaxHealth;
            }

            if (FirstArmorHP[__instance.theFirstArmorType] >= 0 &&
                __instance.theMaxHealth != FirstArmorHP[__instance.theFirstArmorType])
            {
                __instance.theFirstArmorMaxHealth = FirstArmorHP[__instance.theFirstArmorType];
                __instance.theFirstArmorHealth = __instance.theFirstArmorMaxHealth;
            }

            if (SecondArmorHP[__instance.theSecondArmorType] >= 0 &&
                __instance.theMaxHealth != SecondArmorHP[__instance.theSecondArmorType])
            {
                __instance.theSecondArmorMaxHealth = SecondArmorHP[__instance.theSecondArmorType];
                __instance.theSecondArmorHealth = __instance.theSecondArmorMaxHealth;
            }

            __instance.UpdateHealthText();
        }
        catch
        {
        }
    }

    private static System.Reflection.FieldInfo? _cachedCursedPlantsField = null;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Zombie.TakeDamage))]
    public static bool Prefix(Zombie __instance, DamageType theDamageType, ref int theDamage, PlantType reportType,
        bool fix)
    {
        // 僵尸限伤功能 - 限制每次伤害最多为设定值
        if (ZombieDamageLimit > 0 && theDamage > ZombieDamageLimit)
        {
            theDamage = ZombieDamageLimit;
        }

        // 击杀升级功能 - 记录伤害来源植物
        if (KillUpgrade && reportType != PlantType.Nothing && __instance != null)
        {
            try
            {
                int zombieId = __instance.GetInstanceID();
                ZombieLastDamageSource[zombieId] = reportType;
            }
            catch
            {
            }
        }

        if (!CurseImmunity) return true;
        try
        {
            // 性能优化：缓存字段信息
            if (_cachedCursedPlantsField == null)
            {
                _cachedCursedPlantsField = typeof(Zombie).GetField("cursedPlants",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance);
            }

            if (_cachedCursedPlantsField != null)
            {
                var cursedPlants =
                    _cachedCursedPlantsField.GetValue(__instance) as Il2CppSystem.Collections.Generic.List<Plant>;
                if (cursedPlants != null && cursedPlants.Count > 0)
                {
                    cursedPlants.Clear();
                }
            }
        }
        catch
        {
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Zombie.BodyTakeDamage))]
    [HarmonyPatch(nameof(Zombie.FirstArmorTakeDamage))]
    [HarmonyPatch(nameof(Zombie.SecondArmorTakeDamage))]
    public static bool PreBodyTakeDamage(Zombie __instance, ref int theDamage)
    {
        // 僵尸限伤功能 - 限制每次伤害最多为设定值
        if (ZombieDamageLimit > 0 && theDamage > ZombieDamageLimit)
        {
            theDamage = ZombieDamageLimit;
        }

        return true;
    }


    [HarmonyPatch(nameof(Zombie.JalaedExplode))]
    public static bool PreBodyJalaedExplode(Zombie __instance, ref int damage)
    {
        // 僵尸限伤功能 - 限制每次伤害最多为设定值
        if (ZombieDamageLimit > 0 && damage > ZombieDamageLimit)
        {
            damage = ZombieDamageLimit;
        }

        return true;
    }


    [HarmonyPrefix]
    [HarmonyPatch(nameof(Zombie.Update))]
    public static void PreUpdate(Zombie __instance)
    {
        if (ZombieSpeedMultiplier is < 0 or 1.0f) return;
        try
        {
            if (__instance == null) return;

            int instanceId = __instance.GetInstanceID();

            // 如果是第一次处理这个僵尸，记录其原始速度
            if (!ZombieOriginalSpeeds.ContainsKey(instanceId))
            {
                ZombieOriginalSpeeds[instanceId] = __instance.theOriginSpeed;
            }

            float originalSpeed = ZombieOriginalSpeeds[instanceId];
            float newSpeed = originalSpeed * ZombieSpeedMultiplier;

            // 修改僵尸的速度属性
            __instance.theSpeed = newSpeed;
            __instance.theOriginSpeed = newSpeed;

            // 修改动画速度以匹配移动速度
            if (__instance.anim != null)
            {
                __instance.anim.SetFloat("Speed", newSpeed);
            }
        }
        catch
        {
        }
    }

    // 清理已死亡僵尸的记录，避免内存泄漏
    public static void CleanupDeadZombies()
    {
        try
        {
            // 简单的清理逻辑：当字典过大时清空
            if (ZombieOriginalSpeeds.Count > 1000)
            {
                ZombieOriginalSpeeds.Clear();
            }


            if (ZombieOriginalAttackDamages.Count > 1000)
            {
                ZombieOriginalAttackDamages.Clear();
            }
        }
        catch
        {
        }
    }


    [HarmonyPrefix]
    [HarmonyPatch(nameof(Zombie.AttackEffect))]
    public static void PreAttackEffect(Zombie __instance)
    {
        if (ZombieAttackMultiplier is < 0 or 1.0f) return;
        try
        {
            if (__instance == null) return;

            int instanceId = __instance.GetInstanceID();

            // 如果是第一次处理这个僵尸，记录其原始攻击力
            if (!ZombieOriginalAttackDamages.ContainsKey(instanceId))
            {
                ZombieOriginalAttackDamages[instanceId] = __instance.theAttackDamage;
            }

            int originalDamage = ZombieOriginalAttackDamages[instanceId];
            int newDamage = Mathf.RoundToInt(originalDamage * ZombieAttackMultiplier);

            // 修改僵尸的攻击伤害
            __instance.theAttackDamage = newDamage;
        }
        catch
        {
        }
    }
    
    /// <summary>
    /// 僵尸免疫魅惑补丁 - Zombie.SetMindControl
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(nameof(Zombie.SetMindControl))]
    public static bool PreSetMindControl(Zombie __instance)
    {
        if (!ZombieImmuneAllDebuffs && !ZombieImmuneMindControl) return true;
        try
        {
            if (__instance == null) return true;
            // 阻止魅惑效果
            return false;
        }
        catch
        {
            return true;
        }
    }


    /// <summary>
    /// 僵尸免疫冻结补丁 - Zombie.SetFreeze
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(nameof(Zombie.SetFreeze))]
    public static bool PreSetFreeze(Zombie __instance)
    {
        if (!ZombieImmuneAllDebuffs && !ZombieImmuneFreeze) return true;
        try
        {
            if (__instance == null) return true;
            // 阻止冻结效果
            return false;
        }
        catch
        {
            return true;
        }
    }


    /// <summary>
    /// 僵尸免疫减速补丁 - Zombie.SetCold
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(nameof(Zombie.SetCold))]
    public static bool PreSetCold(Zombie __instance)
    {
        if (!ZombieImmuneAllDebuffs && !ZombieImmuneCold) return true;
        try
        {
            if (__instance == null) return true;
            // 阻止减速效果
            return false;
        }
        catch
        {
            return true;
        }
    }


    /// <summary>
    /// 僵尸免疫黄油定身补丁 - Zombie.Buttered
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(nameof(Zombie.Buttered))]
    public static bool PreButtered(Zombie __instance)
    {
        if (!ZombieImmuneAllDebuffs && !ZombieImmuneButter) return true;
        try
        {
            if (__instance == null) return true;
            // 阻止黄油定身效果
            return false;
        }
        catch
        {
            return true;
        }
    }


    /// <summary>
    /// 僵尸免疫中毒补丁 - Zombie.SetPoison
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(nameof(Zombie.SetPoison))]
    public static bool PreSetPoison(Zombie __instance)
    {
        if (!ZombieImmuneAllDebuffs && !ZombieImmunePoison) return true;
        try
        {
            if (__instance == null) return true;
            // 阻止中毒效果
            return false;
        }
        catch
        {
            return true;
        }
    }


    /// <summary>
    /// 僵尸免疫中毒等级增加补丁 - Zombie.AddPoisonLevel
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(nameof(Zombie.AddPoisonLevel))]
    public static bool PreAddPoisonLevel(Zombie __instance)
    {
        if (!ZombieImmuneAllDebuffs && !ZombieImmunePoison) return true;
        try
        {
            if (__instance == null) return true;
            // 阻止中毒等级增加
            return false;
        }
        catch
        {
            return true;
        }
    }


    /// <summary>
    /// 僵尸免疫吃大蒜补丁 - Zombie.EatGarlic
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(nameof(Zombie.EatGarlic))]
    public static bool PreEatGarlic(Zombie __instance)
    {
        if (!ZombieImmuneAllDebuffs && !ZombieImmunePoison) return true;
        try
        {
            if (__instance == null) return true;
            // 阻止吃大蒜效果（蒜毒）
            return false;
        }
        catch
        {
            return true;
        }
    }


    /// <summary>
    /// 僵尸免疫大蒜影响补丁 - Zombie.Garliced
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(nameof(Zombie.Garliced))]
    public static bool PreGarliced(Zombie __instance)
    {
        if (!ZombieImmuneAllDebuffs && !ZombieImmunePoison) return true;
        try
        {
            if (__instance == null) return true;
            // 阻止大蒜影响（换行）
            return false;
        }
        catch
        {
            return true;
        }
    }


    /// <summary>
    /// 僵尸免疫击退补丁 - Zombie.KnockBack
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(nameof(Zombie.KnockBack))]
    public static bool PreKnockBack(Zombie __instance)
    {
        if (!ZombieImmuneAllDebuffs && !ZombieImmuneKnockback) return true;
        try
        {
            if (__instance == null) return true;
            // 阻止击退效果
            return false;
        }
        catch
        {
            return true;
        }
    }


    /// <summary>
    /// 僵尸免疫红温补丁 - Zombie.SetJalaed
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(nameof(Zombie.SetJalaed))]
    public static bool PreSetJalaed(Zombie __instance)
    {
        if (!ZombieImmuneAllDebuffs && !ZombieImmuneJalaed) return true;
        try
        {
            if (__instance == null) return true;
            // 阻止红温效果
            return false;
        }
        catch
        {
            return true;
        }
    }


    /// <summary>
    /// 僵尸免疫余烬补丁 - Zombie.SetEmbered
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(nameof(Zombie.SetEmbered))]
    public static bool PreSetEmbered(Zombie __instance, bool ulti = false)
    {
        if (!ZombieImmuneAllDebuffs && !ZombieImmuneEmbered) return true;
        try
        {
            // 严格的对象有效性检查
            if (__instance == null) return true;

            // 安全地检查对象有效性
            try
            {
                var _ = __instance.theHealth;
            }
            catch
            {
                return true; // 对象可能已销毁
            }

            try
            {
                if (__instance.theHealth <= 0) return true;
            }
            catch
            {
                return true; // 对象可能已销毁
            }

            // 阻止余烬效果
            return false;
        }
        catch
        {
            return true;
        }
    }
}