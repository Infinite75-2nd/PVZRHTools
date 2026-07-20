using System;
using System.Collections.Generic;
using System.Reflection;
using Il2CppInterop.Runtime.Injection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ToolMod.Components;

public class PlantStatisticsModifier : MonoBehaviour
{
    public PlantStatisticsModifier() : base(ClassInjector.DerivedConstructorPointer<PlantStatisticsModifier>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    public PlantStatisticsModifier(IntPtr ptr) : base(ptr)
    {
    }

    public Plant TargetPlant => GetComponent<PlantDataMenu>().plant;

    public static GameObject InputFieldPrefab { get; set; } =
        Resources.Load<GameObject>("ui\\prefabs\\UIConfigMenu").transform.GetChild(2).GetChild(0).GetChild(0).gameObject;

    public static GameObject TogglePrefab { get; set; } =
        Resources.Load<GameObject>("ui\\prefabs\\sample\\Toggle");

    public void Start()
    {
        // ── 仿 PlantDamageMenu.prefab 的 ScrollRect 结构 ────────────
        var scrollView = new GameObject("PlantStatsScrollView");
        scrollView.transform.SetParent(transform.GetChild(1));
        var svRt = scrollView.AddComponent<RectTransform>();
        svRt.sizeDelta = new Vector2(5.5f, 5.5f);
        svRt.anchoredPosition = Vector2.zero;

        // ScrollRect
        var scrollRect = scrollView.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 50;

        // Viewport (仿 prefab: anchor=0,0 pivot=0,1 sizeDelta=0,0)
        var viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform);

        var vpRt = viewport.AddComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.zero;
        vpRt.pivot = new Vector2(0, 1);
        vpRt.sizeDelta = Vector2.zero;
        vpRt.anchoredPosition = Vector2.zero;

        viewport.AddComponent<Mask>();
        var vpImage = viewport.AddComponent<Image>();
        vpImage.color = new Color(0, 0, 0, 0.5f);
        vpImage.raycastTarget = true;

        // Content (仿 prefab: anchor=0,1~1,1 pivot=0,1)
        var content = new GameObject("Content");
        content.transform.SetParent(viewport.transform);

        var contentRt = content.AddComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = Vector2.one;
        contentRt.pivot = new Vector2(0, 1);
        contentRt.sizeDelta = Vector2.zero;
        contentRt.anchoredPosition = Vector2.zero;

        scrollRect.viewport = vpRt;
        scrollRect.content = contentRt;

        // ── GridLayoutGroup 直接挂 Content 上 ───────────────────────
        // 使用 PlantDamageMenu 的已验证值: cellSize=100x140, spacing=0x10
        var glg = content.AddComponent<GridLayoutGroup>();
        glg.constraint = GridLayoutGroup.Constraint.Flexible;
        glg.spacing = new Vector2(0, 10);
        glg.padding = new RectOffset { left = 0, top = 0, right = 0, bottom = 0 };
        glg.childAlignment = TextAnchor.UpperLeft;
        glg.cellSize = new Vector2(100, 40);

        // ── 填充属性行 ────────────────────────────────────────────────
        BuildUi(content.transform);

        // ── 手动设置 content 高度 (仿 prefab, 不用 ContentSizeFitter) ─
        int rows = _bindings.Count;
        if (rows > 0)
        {
            float h = rows * 40 + (rows - 1) * 10;
            contentRt.sizeDelta = new Vector2(0, h);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  UI 构建
    // ═══════════════════════════════════════════════════════════════════

    private void BuildUi(Transform contentParent)
    {
        var plant = TargetPlant;
        if (plant == null) return;

        var props = typeof(Plant).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        if (props == null || props.Length == 0) return;

        foreach (var prop in props)
        {
            var pt = prop.PropertyType;
            if (pt != typeof(int) && pt != typeof(float) && pt != typeof(bool)) continue;
            if (!prop.CanRead || !prop.CanWrite) continue;
            if (prop.GetIndexParameters().Length > 0) continue;
            if (IsUnityBaseProperty(prop)) continue;

            AddPropertyRow(contentParent, prop);
        }
    }

    private static bool IsUnityBaseProperty(PropertyInfo prop) => prop.Name switch
    {
        "name" or "hideFlags" or "enabled" or "isActiveAndEnabled" or
        "gameObject" or "transform" or "tag" or "rigidbody" or
        "rigidbody2D" or "camera" or "light" or "animation" or
        "renderer" or "audio" or "particleSystem" or "particleEmitter" or
        "collider" or "collider2D" or "hingeJoint" or "runInEditMode" or
        "hasTransform" or "canvas" or "allowPrefabMode" or "isPartOfPrefabInstance" or
        "Component" or "TheArchitect" or "WasMoved" => true,
        _ => false,
    };

    private void AddPropertyRow(Transform parent, PropertyInfo prop)
    {
        var row = new GameObject($"Row_{prop.Name}");
        row.transform.SetParent(parent);

        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment         = TextAnchor.MiddleLeft;
        hlg.childControlWidth      = true;
        hlg.childControlHeight     = true;
        hlg.childForceExpandWidth  = true;
        hlg.childForceExpandHeight = true;
        hlg.spacing = 20;
        hlg.padding = new RectOffset { left = 0, top = 0, right = 0, bottom = 0 };

        // Label
        var labelObj = new GameObject("Label");
        labelObj.transform.SetParent(row.transform);

        var labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text              = prop.Name;
        labelText.fontSize          = 20;
        labelText.color             = Color.white;
        labelText.alignment         = TextAlignmentOptions.Left;

        var labelLe = labelObj.AddComponent<LayoutElement>();
        labelLe.minWidth       = 40;
        labelLe.preferredWidth = 50;
        labelLe.flexibleWidth  = 0;

        // Value
        if (prop.PropertyType == typeof(bool))
        {
            var tObj = Object.Instantiate(TogglePrefab, row.transform);
            if (tObj == null) return;
            tObj.name = $"Value_{prop.Name}";
            var toggle = tObj.GetComponent<Toggle>();
            if (toggle == null) return;

            try { toggle.isOn = (bool)prop.GetValue(TargetPlant); }
            catch { toggle.isOn = false; }

            toggle.onValueChanged.AddListener((UnityAction<bool>)(v =>
            {
                try { if (TargetPlant != null) prop.SetValue(TargetPlant, v); }
                catch { }
            }));

            _bindings.Add(new PropBinding(prop, toggle: toggle));
        }
        else
        {
            var iObj = Object.Instantiate(InputFieldPrefab, row.transform);
            if (iObj == null) return;
            iObj.name = $"Value_{prop.Name}";
            var inputField = iObj.GetComponent<TMP_InputField>();
            if (inputField == null) return;

            try
            {
                var val = prop.GetValue(TargetPlant);
                inputField.text = val?.ToString() ?? "0";
            }
            catch { inputField.text = "0"; }

            inputField.contentType = prop.PropertyType == typeof(int)
                ? TMP_InputField.ContentType.IntegerNumber
                : TMP_InputField.ContentType.DecimalNumber;

            inputField.onValueChanged.AddListener((UnityAction<string>)(s =>
            {
                if (TargetPlant == null) return;
                try
                {
                    if (prop.PropertyType == typeof(int))
                        { if (int.TryParse(s, out var v))  prop.SetValue(TargetPlant, v); }
                    else if (prop.PropertyType == typeof(float))
                        { if (float.TryParse(s, out var v)) prop.SetValue(TargetPlant, v); }
                }
                catch { }
            }));

            var inputLe = iObj.AddComponent<LayoutElement>();
            inputLe.minWidth      = 40;
            inputLe.flexibleWidth = 1;

            _bindings.Add(new PropBinding(prop, inputField: inputField));
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  值同步
    // ═══════════════════════════════════════════════════════════════════

    public void Update()
    {
        var plant = TargetPlant;
        if (plant == null) return;

        foreach (var b in _bindings)
        {
            try
            {
                var cur = b.prop.GetValue(plant);
                if (Equals(cur, b.lastValue)) continue;
                b.lastValue = cur;

                if (b.toggle != null)
                {
                    if (b.toggle.isOn != (bool)cur) b.toggle.isOn = (bool)cur;
                }
                else if (b.inputField != null)
                {
                    var str = cur?.ToString() ?? "";
                    if (b.inputField.text != str) b.inputField.text = str;
                }
            }
            catch { }
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  Binding 记录
    // ═══════════════════════════════════════════════════════════════════

    private readonly List<PropBinding> _bindings = new();

    private sealed class PropBinding
    {
        public readonly PropertyInfo prop;
        public readonly TMP_InputField? inputField;
        public readonly Toggle? toggle;
        public object? lastValue;

        public PropBinding(PropertyInfo prop, TMP_InputField? inputField = null, Toggle? toggle = null)
        {
            this.prop       = prop;
            this.inputField = inputField;
            this.toggle     = toggle;
        }
    }
}
