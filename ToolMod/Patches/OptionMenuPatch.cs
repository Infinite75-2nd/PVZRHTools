using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(OptionMenu))]
public static class OptionMenuPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(OptionMenu.Start))]
    public static void PostStart(OptionMenu __instance)
    {
        var keyBinding= Object.Instantiate(__instance.transform.FindChild("Buttons").GetChild(5).gameObject,
            __instance.transform.FindChild("Buttons"));
        keyBinding.transform.localPosition = new(410, -438.7f, 0);
        keyBinding.GetComponent<UIButton>().clickEvent=new UnityEvent();
        keyBinding.GetComponent<UIButton>().clickEvent.AddListener((UnityAction)(() => GameAPP.UIManager.Push((UIType)999)));
        keyBinding.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "PVZRHTools\n按键绑定";
        keyBinding.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "PVZRHTools\n按键绑定";
        __instance.collider2Ds.Add(keyBinding.GetComponent<BoxCollider2D>());
    }
}