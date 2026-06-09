using HarmonyLib;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(LevelProgress))]
public static class ProgressMgrPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(LevelProgress.Awake))]
    public static void PostAwake(LevelProgress __instance)
    {
        GameObject obj = new("ModifierGameInfo");
        var text = obj.AddComponent<TextMeshProUGUI>();
        text.font = Resources.Load<TMP_FontAsset>("Fonts/ContinuumBold SDF");
        text.color = new Color(0, 1, 1);
        obj.transform.SetParent(__instance.GameObject().transform);
        obj.transform.localScale = new Vector3(0.4f, 0.2f, 0.2f);
        obj.transform.localPosition = new Vector3(100f, 2.2f, 0);
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 50);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(LevelProgress.Update))]
    public static void PostUpdate(LevelProgress __instance)
    {
        try
        {
            if (__instance == null) return;
            var infoChild = __instance.transform.FindChild("ModifierGameInfo");
            if (infoChild == null) return;
            if (ShowGameInfo)
            {
                infoChild.GameObject().active = true;
                // 使用 timeUntilNextWave 显示刷新CD（3.3.1版本中newZombieWaveCountDown字段已被移除）
                float refreshCD = 0f;
                int currentWave = 0;
                int maxWave = 0;
                if (Board.Instance != null)
                {
                    refreshCD = Board.Instance.timeUntilNextWave;
                    currentWave = Board.Instance.theWave;
                    maxWave = Board.Instance.theMaxWave;

                    // 如果刷新CD为0或负数，但游戏还在进行中（不是最后一波），
                    // 可能是刚刚触发了"生成下一波"，此时等待游戏更新 timeUntilNextWave
                    // 如果游戏还没有更新（通常会在 NewZombieUpdate() 中更新），
                    // 则使用 NewZombieUpdateCD 作为临时显示值
                    if (refreshCD <= 0f && currentWave > 0 && currentWave < maxWave)
                    {
                        // 检查 NewZombieUpdateCD 是否有效（通常在 0-30 秒之间）
                        if (NewZombieUpdateCD > 0f && NewZombieUpdateCD <= 30f)
                        {
                            // 使用 NewZombieUpdateCD 作为临时显示值
                            // 游戏会在 NewZombieUpdate() 中更新 timeUntilNextWave
                            refreshCD = NewZombieUpdateCD;
                        }
                        // 如果 NewZombieUpdateCD 无效，保持 refreshCD 为 0，显示 "N/A"
                    }
                }

                string cdText = refreshCD > 0f ? $"{refreshCD:F1}" : "N/A";
                infoChild.GameObject().GetComponent<TextMeshProUGUI>().text =
                    $"波数: {currentWave}/{maxWave} 刷新CD: {cdText}";
            }
            else
            {
                infoChild.GameObject().active = false;
            }
        }
        catch
        {
        }
    }
}