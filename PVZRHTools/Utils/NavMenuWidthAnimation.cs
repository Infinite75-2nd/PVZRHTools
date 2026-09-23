using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Styling;
using Ursa.Helpers;

namespace PVZRHTools.Utils;

public static class NavMenuWidthAnimation
{
    public static readonly SizeAnimationHelperAnimationGeneratorDelegate Generator = Create;

    private static Animation Create(Control _, Size oldDesiredSize, Size newDesiredSize)
    {
        var from = oldDesiredSize.Width;
        var to = newDesiredSize.Width;
        if (from > to)
            to += 20;

        return new Animation
        {
            Duration = TimeSpan.FromMilliseconds(300),
            Easing = new CubicEaseInOut(),
            FillMode = FillMode.None,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters = { new Setter(Layoutable.WidthProperty, from) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters = { new Setter(Layoutable.WidthProperty, to) }
                }
            }
        };
    }
}
