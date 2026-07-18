using HarmonyLib;
using TMPro;
using ToolData;
using UnityEngine;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(UIMgr))]
public static class UIMgrPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIMgr.EnterMainMenu))]
    public static void PostEnterMainMenu()
    {
        GameObject obj1 = new("ModifierInfo");
        var text1 = obj1.AddComponent<TextMeshProUGUI>();
        text1.font = Resources.Load<TMP_FontAsset>("Fonts/ContinuumBold SDF");
        text1.color = new Color(0.5f, 0f, 1f, 1);
        text1.text = $"PVZRHTools {Strings.GameVersion}-{Strings.ModifierVersion}\n作者@Infinite75@梧萱梦汐X@墨染荷韵\n已加载{BepInEx.Unity.IL2CPP.IL2CPPChainloader.Instance.Plugins.Count}个Mod";
        obj1.transform.SetParent(GameObject.Find("Leaves").transform);
        obj1.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        obj1.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 50);
        obj1.transform.localPosition = new Vector3(-345.5f, -15f, 0);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIMgr.EnterChallengeMenu))]
    public static void PostEnterChallengeMenu()
    {

        try
        {
            GameObject abyss=GameObject.Find("FirstBtns").transform.GetChild(9).gameObject;
            abyss.active = true;
            abyss.GetComponentInChildren<TextMeshProUGUI>().text = "冒险秘境（未完成）";
        }
        catch 
        {
        }
    }
}