using System;
using System.Linq.Expressions;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ToolMod.Components;

/// <summary>
/// 按键绑定控件 MonoBehaviour。
/// 挂载在游戏内 TheButton 预制体的同一 GameObject 上，实现点击后检测按键并绑定。
///
/// 使用方式：
///   btn.Bind(() => PatchDataCache.KeySpeedStop);
///
/// Bind 方法通过表达式树创建读写委托，与 Utils.SimpleSyncKeyCode 同理。
/// </summary>
public class KeyBindingButton : MonoBehaviour
{
    private TextMeshProUGUI _label;
    private bool _isListening;
    private KeyCode[] _allKeys;
    private Func<KeyCode> _boundGetter;

    #region 静态属性

    /// <summary>
    /// 当前正处于按键检测状态的 KeyBindingButton 实例。
    /// 同一时刻最多只有一个按钮在检测，可用于外部查询或 UI 状态指示。
    /// 为 null 表示没有任何按钮处于检测状态。
    /// </summary>
    public static KeyBindingButton CurrentlyListening { get; private set; }

    #endregion

    #region 公开属性

    /// <summary>当前绑定的按键</summary>
    public KeyCode BoundKey { get; set; } = KeyCode.None;

    /// <summary>
    /// 按键绑定变化时的回调。
    /// </summary>
    [HideFromIl2Cpp]
    public Action<KeyCode> OnKeyBindingChanged { get; set; }

    /// <summary>是否允许按 Escape 取消检测（默认 true）</summary>
    public bool AllowCancelWithEscape { get; set; } = true;

    /// <summary>是否将鼠标按键也视为合法绑定（默认 false，仅绑定键盘按键）</summary>
    public bool IncludeMouseButtons { get; set; } = false;

    #endregion

    #region 构造函数（Il2CppInterop 注入必需）

    public KeyBindingButton() : base(ClassInjector.DerivedConstructorPointer<KeyBindingButton>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    public KeyBindingButton(IntPtr ptr) : base(ptr)
    {
    }

    #endregion

    #region Unity 生命周期

    public void Awake()
    {
        _label = GetComponentInChildren<TextMeshProUGUI>(true);
        _allKeys = (KeyCode[])Enum.GetValues(typeof(KeyCode));
        // ★ Awake 时 BoundKey 还是 KeyCode.None（属性默认值），这里显示 "None"
        UpdateLabel();
    }

    public void Start()
    {
        // ★ 调用 getter 从 PatchDataCache 读取当前值作为初始显示
        if (_boundGetter != null)
        {
            BoundKey = _boundGetter();
            UpdateLabel();
        }

        var button = GetComponent<TheButton>();
        if (button != null)
            SubscribeToTheEvent(button);
    }

    public void Update()
    {
        if (!_isListening || _allKeys == null) return;

        if (AllowCancelWithEscape && Input.GetKeyDown(KeyCode.Escape))
        {
            CancelListening();
            return;
        }

        foreach (var key in _allKeys)
        {
            if (!Input.GetKeyDown(key)) continue;

            if (key == KeyCode.None) continue;
            if (key == KeyCode.Escape) continue;
            if (!IncludeMouseButtons && IsMouseButton(key)) continue;

            BoundKey = key;
            _isListening = false;
            if (CurrentlyListening == this)
                CurrentlyListening = null;
            UpdateLabel();
            OnKeyBindingChanged?.Invoke(key);
            HotKeysLoader.Save();
            return;
        }
    }

    #endregion

    #region 表达式树绑定

    /// <summary>
    /// 绑定到 PatchDataCache 中的 KeyCode 静态属性。
    /// 通过表达式树同时创建 getter（读初始值）和 setter（写回调）。
    ///
    /// ---- 预期初始化流程（从 MakeKeyBindingUI 调用） ----
    /// (1) new GameObject / Instantiate → Awake() → 显示 "None"
    /// (2) 外部调用 btn.Bind(() => KeySpeedStop)
    /// (3) expression.Compile() → getter 指向 PatchDataCache.KeySpeedStop 的 get 方法
    /// (4) BoundKey = getter() → BoundKey = PatchDataCache.KeySpeedStop（★ 此处应读取到当前值）
    /// (5) UpdateLabel() → 显示应变为 KeySpeedStop 的值（如 "Alpha6"）
    /// (6) CreateSetter → setter 指向 PatchDataCache.KeySpeedStop 的 set 方法
    /// (7) OnKeyBindingChanged = key => setter(key)
    /// ---- 预期完成 ----
    ///
    /// 如果 (4) 之后 BoundKey 仍为 KeyCode.None，说明：
    ///   - getter() 读取到的 PatchDataCache.KeySpeedStop 值不是预期值，或
    ///   - Bind() 未被调用（组件实例是克隆出来的新实例），或
    ///   - BoundKey = getter() 执行后又被其他地方覆盖了
    /// </summary>
    /// <param name="propertyExpression">指向 KeyCode 属性的表达式，如 () => KeySpeedStop</param>
    public void Bind(Expression<Func<KeyCode>> propertyExpression)
    {
        if (propertyExpression == null) return;

        _boundGetter = propertyExpression.Compile();
        OnKeyBindingChanged = CreateSetter(propertyExpression);
    }

    private static Action<T> CreateSetter<T>(Expression<Func<T>> propertyExpression)
    {
        if (propertyExpression == null)
            throw new ArgumentNullException(nameof(propertyExpression));

        var body = propertyExpression.Body;
        if (body is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unary)
            body = unary.Operand;

        if (body is not MemberExpression memberExpr)
            throw new ArgumentException("表达式必须指向属性或字段。", nameof(propertyExpression));

        var valueParam = Expression.Parameter(typeof(T), "value");
        var assign = Expression.Assign(memberExpr, valueParam);
        var setterLambda = Expression.Lambda<Action<T>>(assign, valueParam);
        return setterLambda.Compile();
    }

    #endregion

    #region 点击事件挂载

    private void SubscribeToTheEvent(TheButton button)
    {
        if (button.theEvent_up != null)
            button.theEvent_up.AddListener((UnityAction)(Action)OnButtonClicked);
    }

    public void OnButtonClicked()
    {
        ToggleListening();
    }

    #endregion

    #region 公开方法

    public void ToggleListening()
    {
        if (_isListening)
            CancelListening();
        else
            StartListening();
    }

    public void StartListening()
    {
        if (CurrentlyListening != null && CurrentlyListening != this)
            CurrentlyListening.CancelListening();

        _isListening = true;
        CurrentlyListening = this;
        UpdateLabel();
    }

    public void CancelListening()
    {
        if (!_isListening) return;
        _isListening = false;
        if (CurrentlyListening == this)
            CurrentlyListening = null;
        UpdateLabel();
    }

    public void SetKey(KeyCode key, bool suppressCallback = false)
    {
        var changed = BoundKey != key;
        BoundKey = key;
        UpdateLabel();
        if (changed && !suppressCallback)
            OnKeyBindingChanged?.Invoke(key);
    }

    #endregion

    #region 内部方法

    private void UpdateLabel()
    {
        if (_label == null) return;

        _label.text = _isListening ? $">{BoundKey}<" : BoundKey.ToString();
    }

    private static bool IsMouseButton(KeyCode key)
    {
        return key is KeyCode.Mouse0 or KeyCode.Mouse1 or KeyCode.Mouse2 or KeyCode.Mouse3
            or KeyCode.Mouse4 or KeyCode.Mouse5 or KeyCode.Mouse6;
    }

    #endregion
}
