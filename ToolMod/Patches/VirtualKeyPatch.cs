using HarmonyLib;
using UnityEngine;

namespace ToolMod.Patches;

/// <summary>
/// 虚拟按键：启动器浮窗发送 VirtualKey 命令，把一次 KeyCode 按下注入游戏。
/// 在 Input.GetKeyDown(KeyCode) 前缀拦截并消费一次，不依赖真实键盘。
/// </summary>
[HarmonyPatch(typeof(Input))]
public static class VirtualKeyPatch
{
    private static KeyCode _pending = KeyCode.None;

    public static void Press(KeyCode key) => _pending = key;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.GetKeyDown), new[] { typeof(KeyCode) })]
    public static bool PreGetKeyDown(KeyCode key, ref bool __result)
    {
        if (_pending != KeyCode.None && key == _pending)
        {
            __result = true;
            _pending = KeyCode.None;
            return false;
        }
        return true;
    }
}
