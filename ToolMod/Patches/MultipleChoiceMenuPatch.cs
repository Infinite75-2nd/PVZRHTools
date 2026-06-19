using System;
using System.Collections.Generic;
using System.Reflection;
using GameLevel.RogueShooting;
using HarmonyLib;
using UnityEngine;
using static ToolMod.Components.PatchDataCache;
using static ToolMod.Utils;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(MultipleChoiceMenu))]
public static class MultipleChoiceMenuRefreshPatch
{
    private static FieldInfo? _refreshCountField;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(MultipleChoiceMenu.SetRefreshable))]
    public static void PrefixSetRefreshable(ref int refreshCount, ref bool interactable)
    {
        if (!ShouldFixGodEvolutionRefreshButton) return;
        refreshCount = GetGodEvolutionMenuRefreshCount();
        interactable = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(MultipleChoiceMenu.SetRefreshable))]
    public static void PostfixSetRefreshable(MultipleChoiceMenu __instance)
    {
        if (!ShouldFixGodEvolutionRefreshButton) return;
        try
        {
            if (__instance?.refreshButton != null)
                __instance.refreshButton.Interactable = true;
        }
        catch
        {
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(MultipleChoiceMenu.Refresh))]
    public static void PrefixRefresh(MultipleChoiceMenu __instance)
    {
        if (!ShouldFixGodEvolutionRefreshButton) return;
        try
        {
            _refreshCountField ??= typeof(MultipleChoiceMenu).GetField("refreshCount",
                BindingFlags.Instance | BindingFlags.NonPublic);
            _refreshCountField?.SetValue(__instance, GetGodEvolutionMenuRefreshCount());
        }
        catch
        {
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(MultipleChoiceMenu.Refresh))]
    public static void PostfixRefresh(MultipleChoiceMenu __instance)
    {
        if (!ShouldFixGodEvolutionRefreshButton) return;
        try
        {
            _refreshCountField ??= typeof(MultipleChoiceMenu).GetField("refreshCount",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var count = GetGodEvolutionMenuRefreshCount();
            _refreshCountField?.SetValue(__instance, count);
            if (__instance?.refreshButton != null)
                __instance.refreshButton.Interactable = true;
        }
        catch
        {
        }
    }

    #region 词条多重选择

    // 多选状态跟踪
    private static readonly HashSet<int> _selectedIndices = new();
    private static readonly Dictionary<int, BaseWindow> _multiSelectWindows = new();
    private static bool _isApplyingMultiSelect;

    // 缓存反射方法 + 点击跟踪
    private static MethodInfo? _confirmMethod;
    private static UIButton? _clickedWindow;

    /// <summary>判断当前是否在游戏内 Buff 选择环境（而非难度选择）</summary>
    private static bool IsInGameBuffContext()
    {
        try { return GodEvolutionMultiSelectBuff&&Board.Instance!=null&&Board.Instance.boardTag.rogueShooting && ShootingManager.Instance != null; }
        catch { return false; }
    }

    private static void ClearMultiSelectState()
    {
        // 关闭所有窗口的 selectedLight 高亮
        foreach (var kvp in _multiSelectWindows)
            SetSelectedLight(kvp.Value, false);
        _selectedIndices.Clear();
        _multiSelectWindows.Clear();
    }

    private static void ToggleSelection(MultipleChoiceMenu menu, int index)
    {
        if (!_selectedIndices.Remove(index))
            _selectedIndices.Add(index);
        var windows = menu?.windows;
        if (windows != null && index < windows.Count)
            UpdateWindowVisual(windows[index], index);
    }

    /// <summary>通过反射调用 UIButton.Confirm() 触发 clickEvent</summary>
    private static void InvokeWindowClickEvent(BaseWindow window)
    {
        if (window == null) return;
        try
        {
            _confirmMethod ??= AccessTools.Method(typeof(UIButton), "Confirm", Type.EmptyTypes);
            if (_confirmMethod == null)
            {
                ModCore.Instance.Log?.LogWarning("[MultiSelect] UIButton.Confirm method not found");
                return;
            }
            _confirmMethod.Invoke(window, null);
        }
        catch (Exception ex)
        {
            ModCore.Instance.Log?.LogError("[MultiSelect] InvokeClickEvent error: " + ex);
        }
    }

    /// <summary>
    /// 设置窗口高亮选中态（使用 selectedLight GameObject）
    /// 注意：必须用直接 Il2CppInterop 字段访问（btn.selectedLight），反射 GetValue + as 在 Il2Cpp 运行时失效
    /// </summary>
    private static void SetSelectedLight(BaseWindow window, bool active)
    {
        if (window == null) return;
        try
        {
            // 方案1：直接 Il2CppInterop 字段访问（绕过反射 As 转换问题）
            if (window is UIButton btn)
            {
                var light = btn.selectedLight;
                if (light != null && light)
                {
                    light.SetActive(active);
                    return;
                }
            }

            // 方案2：通过 Transform 层级查找（fallback）
            var t = window.transform;
            if (t != null)
            {
                for (int i = 0; i < t.childCount; i++)
                {
                    var child = t.GetChild(i);
                    if (child != null && child.gameObject != null && child.gameObject)
                    {
                        var name = child.name;
                        if (name != null && name.IndexOf("selected", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            child.gameObject.SetActive(active);
                            return;
                        }
                    }
                }
            }

            // 方案3：反射后备（可能失效）
            var field = typeof(UIButton).GetField("selectedLight",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field?.GetValue(window) is GameObject go && go)
                go.SetActive(active);
        }
        catch (Exception ex)
        {
            ModCore.Instance.Log?.LogError("[MultiSelect] SetSelectedLight error: " + ex.Message);
        }
    }

    /// <summary>
    /// 多选模式：替换 Update 键盘处理
    /// 1. 键盘 1-5 → SelectWindow + 切换选中
    /// 2. 鼠标点击窗口 → OnClicked 拦截 → 切换选中
    /// 3. 空格/确认按钮 → 应用所有选中词条并关闭菜单
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(MultipleChoiceMenu.Update))]
    public static bool PrefixUpdateMultiSelect(MultipleChoiceMenu __instance)
    {
        if (!IsInGameBuffContext() || __instance == null) return true;
        try
        {
            // 数字键 1-5：选中并切换选中状态
            int pressedIndex = -1;
            if (Input.GetKeyDown(KeyCode.Alpha1)) pressedIndex = 0;
            else if (Input.GetKeyDown(KeyCode.Alpha2)) pressedIndex = 1;
            else if (Input.GetKeyDown(KeyCode.Alpha3)) pressedIndex = 2;
            else if (Input.GetKeyDown(KeyCode.Alpha4)) pressedIndex = 3;
            else if (Input.GetKeyDown(KeyCode.Alpha5)) pressedIndex = 4;

            if (pressedIndex >= 0)
            {
                __instance.SelectWindow(pressedIndex);
                ToggleSelection(__instance, pressedIndex);
                _clickedWindow = null; // 清除键盘触发的 OnClicked 标记
            }

            // 鼠标点击检测（通过 OnClicked 前缀拦截器）
            if (pressedIndex < 0 && _clickedWindow != null)
            {
                var windows = __instance.windows;
                if (windows != null)
                {
                    for (int i = 0; i < windows.Count; i++)
                    {
                        if (windows[i] != null && windows[i].Pointer == _clickedWindow.Pointer)
                        {
                            ToggleSelection(__instance, i);
                            break;
                        }
                    }
                }
                _clickedWindow = null;
            }

            // 空格键：调用每个选中窗口的 clickEvent 并关闭菜单
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_selectedIndices.Count == 0)
                {
                    GameAPP.PlaySound(26, 0.5f, 1.0f);
                    Core.InGameText.Instance.ShowText("你还没有选择选项", 3.0f, false);
                }
                else
                {
                    ApplyMultiSelectAndClose(__instance);
                }
            }
        }
        catch (Exception ex)
        {
            ModCore.Instance.Log?.LogError("[MultiSelect] Update error: " + ex);
        }
        return false;
    }

    /// <summary>拦截 UIButton.OnClicked，记录键盘选择窗口</summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(UIButton), nameof(UIButton.OnClicked))]
    public static void PrefixOnClickedMultiSelect(UIButton __instance)
    {
        if (!IsInGameBuffContext()) return;
        _clickedWindow = __instance;
    }

    /// <summary>
    /// 拦截 UIButton.OnMouseUpAsButton，记录鼠标点击窗口
    /// 注意：游戏鼠标点击走的是 OnMouseUpAsButton（MonoBehaviour 物理消息），而非 OnClicked！
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(UIButton), nameof(UIButton.OnMouseUpAsButton))]
    public static void PrefixOnMouseUpAsButtonMultiSelect(UIButton __instance)
    {
        if (!IsInGameBuffContext()) return;
        _clickedWindow = __instance;
    }

    /// <summary>多选模式：替换 Confirm 方法，同时通知所有选中词条并关闭菜单</summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(MultipleChoiceMenu.Confirm))]
    public static bool PrefixConfirmMultiSelect(MultipleChoiceMenu __instance)
    {
        if (!IsInGameBuffContext() || __instance == null) return true;
        try
        {
            if (_selectedIndices.Count == 0)
            {
                // 未选择任何词条：播放错误音效 + 显示提示字幕，不执行原始 Confirm
                GameAPP.PlaySound(26, 0.5f, 1.0f);
                Core.InGameText.Instance.ShowText("你还没有选择选项", 3.0f, false);
                return false;
            }
            ApplyMultiSelectAndClose(__instance);
        }
        catch (Exception ex)
        {
            ModCore.Instance.Log?.LogError("[MultiSelect] Confirm error: " + ex);
        }
        return false;
    }

    /// <summary>执行多重选择确认并关闭菜单</summary>
    private static void ApplyMultiSelectAndClose(MultipleChoiceMenu __instance)
    {
        _isApplyingMultiSelect = true;
        foreach (var index in new List<int>(_selectedIndices))
        {
            if (_multiSelectWindows.TryGetValue(index, out var win) && win != null)
                InvokeWindowClickEvent(win);
        }
        _isApplyingMultiSelect = false;

        // 关闭菜单（与原始 OnSelect 逻辑一致）
        Time.timeScale = GameAPP.config.gameSpeed;
        GameAPP.theGameStatus = (GameStatus)0;
        var actionOnExit = __instance.actionOnExit;
        __instance.PopMenu();
        actionOnExit?.Invoke();
        ClearMultiSelectState();
    }

    /// <summary>多选模式 UpdateWindow 后置处理：存储词条回调 + 初始化视觉状态</summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(MultipleChoiceMenu.UpdateWindow))]
    public static void PostfixUpdateWindowMultiSelect(
        MultipleChoiceMenu __instance,
        BaseWindow window,
        MultipleChoiceMenu.OptionData optionData)
    {
        if (!IsInGameBuffContext() || __instance == null || window == null || optionData == null) return;
        try
        {
            var windows = __instance.windows;
            if (windows == null) return;

            int index = -1;
            for (int i = 0; i < windows.Count; i++)
            {
                if (windows[i] == window)
                {
                    index = i;
                    break;
                }
            }
            if (index < 0) return;

            // 新菜单首次打开时清除旧状态
            if (index == 0 && _multiSelectWindows.Count > 0)
                ClearMultiSelectState();

            // 存储窗口引用
            _multiSelectWindows[index] = window;

            // 初始化视觉状态（默认不高亮）
            SetSelectedLight(window, false);
        }
        catch (Exception ex)
        {
            ModCore.Instance.Log?.LogError("[MultiSelect] UpdateWindow error: " + ex);
        }
    }

    /// <summary>更新窗口高亮：使用 selectedLight 游戏对象显示选中边框效果</summary>
    private static void UpdateWindowVisual(BaseWindow window, int index)
    {
        if (window == null) return;
        SetSelectedLight(window, _selectedIndices.Contains(index));
    }

    /// <summary>多选模式：拦截 OnSelect，仅在多选确认循环期间阻止菜单关闭</summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(MultipleChoiceMenu.OnSelect))]
    public static bool PrefixOnSelectMultiSelect(MultipleChoiceMenu __instance)
    {
        if (!IsInGameBuffContext() || __instance == null) return true;
        if (!_isApplyingMultiSelect) return true;
        return false;
    }

    /// <summary>多选模式：拦截 UIButton.OnDisSelect，保护多选窗口的高亮不被游戏单选框清除</summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(UIButton), nameof(UIButton.OnDisSelect))]
    public static bool PrefixOnDisSelectMultiSelect(UIButton __instance)
    {
        if (!IsInGameBuffContext()) return true;
        // 检查当前窗口是否在多选选中集合中
        foreach (var kvp in _multiSelectWindows)
        {
            if (kvp.Value != null && kvp.Value.Pointer == __instance.Pointer)
            {
                if (_selectedIndices.Contains(kvp.Key))
                    return false; // 跳过 OnDisSelect，保持高亮
                break;
            }
        }
        return true;
    }

    /// <summary>多选模式：拦截 Cancel，清除所有选中状态和高亮</summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(MultipleChoiceMenu.Cancel))]
    public static void PrefixCancelMultiSelect()
    {
        if (!IsInGameBuffContext()) return;
        ClearMultiSelectState();
    }

    #endregion
}