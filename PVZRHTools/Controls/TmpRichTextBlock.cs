using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PVZRHTools.Utils;

namespace PVZRHTools.Controls;

public class TmpRichTextBlock : TextBlock
{
    public static readonly StyledProperty<string?> MarkupProperty =
        AvaloniaProperty.Register<TmpRichTextBlock, string?>(nameof(Markup));

    static TmpRichTextBlock()
    {
        TextWrappingProperty.OverrideDefaultValue<TmpRichTextBlock>(TextWrapping.Wrap);
    }

    public TmpRichTextBlock()
    {
        // 主题会给 TextBlock 套 14px，OverrideDefaultValue 盖不过去，必须用本地值。
        FontSize = 18;
    }

    public string? Markup
    {
        get => GetValue(MarkupProperty);
        set => SetValue(MarkupProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == MarkupProperty || change.Property == FontSizeProperty)
            Rebuild();
    }

    private void Rebuild()
    {
        Inlines.Clear();
        foreach (var inline in TmpMarkup.ToInlines(Markup, FontSize))
            Inlines.Add(inline);
    }
}