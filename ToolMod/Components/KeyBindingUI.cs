using System;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static ToolMod.Components.PatchDataCache;

namespace ToolMod.Components;

/// <summary>
/// 按键绑定 UI 构造器。
/// 挂载在 KeyBindingUI 预制体上，Start 时动态构建所有按键绑定行。
///
/// 所有 KeyBindingButton 的 Bind() 调用都在 Start() 中执行，
/// 此时组件实例是 UIManager 最终展示的活跃实例，不会因预制体克隆丢失配置。
/// </summary>
public class KeyBindingUI : MonoBehaviour
{
    /// <summary>每条按键绑定行的配置</summary>
    private readonly (string Label, Func<System.Linq.Expressions.Expression<Func<KeyCode>>> BindingExpr)[] _bindings =
    {
        ("高级时停",               () => () => KeySpeedStop),
        ("显示游戏信息",           () => () => KeyShowGameInfo),
        ("置顶卡槽",               () => () => KeyTopMostCardBank),
        ("随机卡牌",               () => () => KeyRandomCard),
        ("图鉴种植植物",           () => () => KeyAlmanacCreatePlant),
        ("图鉴种植植物(花瓶)",     () => () => KeyAlmanacCreatePlantVase),
        ("图鉴种植僵尸",           () => () => KeyAlmanacCreateZombie),
        ("图鉴种植僵尸(花瓶)",     () => () => KeyAlmanacCreateZombieVase),
        ("图鉴僵尸魅惑控制",       () => () => KeyAlmanacZombieMindCtrl),
    };

    #region 构造函数（Il2CppInterop 注入必需）

    public KeyBindingUI() : base(ClassInjector.DerivedConstructorPointer<KeyBindingUI>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    public KeyBindingUI(IntPtr ptr) : base(ptr)
    {
    }

    #endregion

    #region Unity 生命周期

    public void Start()
    {
        // 禁用原 UIConfigMenu 脚本
        GetComponent<UIConfigMenu>().enabled = false;
        
        // 标题
        var title = transform.GetChild(0).GetChild(0);
        title.GetComponent<TextMeshProUGUI>().text = "PVZRHTools按键绑定";

        // 布局容器
        var layout = transform.GetChild(2);

        // 隐藏布局中的其他默认字段
        foreach (var field in layout)
            field.TryCast<Transform>()?.gameObject.SetActive(false);

        // 构建模板（第一个子项为基准，替换 Input 为按键绑定按钮）
        var template = BuildTemplate(layout);

        // 遍历所有绑定项，逐行创建
        foreach (var (label, exprFactory) in _bindings)
        {
            var row = Instantiate(template, layout.transform);
            row.SetActive(true);
            row.GetComponent<TextMeshProUGUI>().text = label;
            row.transform.GetChild(1).GetComponent<KeyBindingButton>().Bind(exprFactory());
        }

        // 清理模板
        Destroy(template);

    }

    #endregion

    #region 模板构建

    /// <summary>
    /// 从布局的第一个子项构建按键绑定行模板：
    /// 隐藏 Input 字段，放入带 KeyBindingButton 的按钮预制体实例。
    /// </summary>
    private static GameObject BuildTemplate(Transform layout)
    {
        var template = Instantiate(layout.GetChild(0).gameObject);

        // 隐藏默认的 Input 子对象
        var input = template.transform.FindChild("Input");
        if (input != null)
            input.gameObject.SetActive(false);

        // 实例化按钮预制体
        var buttonPrefab = Resources.Load<GameObject>("ui\\prefabs\\sample\\Button");
        var button = Instantiate(buttonPrefab);
        button.transform.SetParent(template.transform);

        // 添加按键绑定组件
        button.AddComponent<KeyBindingButton>();

        // 调整按钮布局
        var buttonRect = button.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(-40, 60);
        buttonRect.anchoredPosition = new Vector2(0, -30);
        button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().fontSizeMax = 32;

        return template;
    }

    #endregion
}
