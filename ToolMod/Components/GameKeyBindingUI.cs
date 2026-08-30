using System;
using Il2CppInterop.Runtime.Injection;
using TMPro;
using UnityEngine;

namespace ToolMod.Components;

/// <summary>
/// 游戏原版按键绑定 UI 构造器。
/// 挂载在 KeyBindingUI 预制体（UIType 998）上，Start 时动态构建所有游戏原版按键绑定行。
///
/// 与 KeyBindingUI 的区别：
/// 绑定目标是 KeyCodeManager 的静态属性（游戏自身的热键），
/// 修改后通过 GameKeysLoader 保存到 GameKeys.json，
/// 直接写入 KeyCodeManager 后游戏内立即生效。
/// </summary>
public class GameKeyBindingUI : MonoBehaviour
{
    /// <summary>每条按键绑定行的配置，表达式指向 KeyCodeManager 的静态属性</summary>
    private readonly (string Label, Func<System.Linq.Expressions.Expression<Func<KeyCode>>> BindingExpr)[] _bindings =
    {
        ("铁铲",             () => () => KeyCodeManager.Shovel),
        ("手套",             () => () => KeyCodeManager.Glove),
        ("游戏慢放",         () => () => KeyCodeManager.SlowTrigger),
        ("锤子",             () => () => KeyCodeManager.Hammer),
        ("手推车",           () => () => KeyCodeManager.Wheel),
        ("显示植物血量",     () => () => KeyCodeManager.ShowPlantHealth),
        ("显示僵尸血量",     () => () => KeyCodeManager.ShowZombieHealth),
        ("使用金豆",         () => () => KeyCodeManager.UseGoldBean),
        ("查看植物图鉴",     () => () => KeyCodeManager.CheckPlantAlmanac),
        ("显示子弹伤害",     () => () => KeyCodeManager.ShowBulletDamage),
        ("僵尸手套",         () => () => KeyCodeManager.ZombieGlove),
        ("查看植物数据",     () => () => KeyCodeManager.LookPlantData),
        ("全屏",             () => () => KeyCodeManager.FullScreen),
        ("窗口化",           () => () => KeyCodeManager.NormalScreen),
        ("RA2音效",          () => () => KeyCodeManager.Ra2Sound),
        ("隐藏UI",           () => () => KeyCodeManager.HideUI),
    };

    #region 构造函数（Il2CppInterop 注入必需）

    public GameKeyBindingUI() : base(ClassInjector.DerivedConstructorPointer<GameKeyBindingUI>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    public GameKeyBindingUI(IntPtr ptr) : base(ptr)
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
        title.GetComponent<TextMeshProUGUI>().text = "游戏原版按键绑定";

        // 布局容器
        var layout = transform.GetChild(2);

        KeyBindingUI.BuildRows(layout, _bindings, GameKeysLoader.Save);
    }

    #endregion
}
