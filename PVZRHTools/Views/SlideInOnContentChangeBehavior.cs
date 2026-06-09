using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Xaml.Interactivity;

namespace PVZRHTools.Views;

public class SlideInOnContentChangeBehavior : Behavior<ContentControl>
{
    public static readonly StyledProperty<double> FromXProperty =
        AvaloniaProperty.Register<SlideInOnContentChangeBehavior, double>(nameof(FromX), 50);

    public static readonly StyledProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.Register<SlideInOnContentChangeBehavior, TimeSpan>(nameof(Duration),
            TimeSpan.FromMilliseconds(300));

    public double FromX
    {
        get => GetValue(FromXProperty);
        set => SetValue(FromXProperty, value);
    }

    public TimeSpan Duration
    {
        get => GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    protected override void OnAttachedToVisualTree()
    {
        base.OnAttachedToVisualTree();
        if (AssociatedObject != null)
        {
            AssociatedObject.GetObservable(ContentControl.ContentProperty)
                .Subscribe(_ => OnContentChanged());
        }
    }

    private void OnContentChanged()
    {
        if (AssociatedObject == null || AssociatedObject.Content == null)
            return;

        var animation = new Animation
        {
            Duration = Duration,
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0),
                    Setters =
                    {
                        new Setter { Property = Control.OpacityProperty, Value = 0d },
                        new Setter { Property = TranslateTransform.XProperty, Value = FromX }
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters =
                    {
                        new Setter { Property = Control.OpacityProperty, Value = 1d },
                        new Setter { Property = TranslateTransform.XProperty, Value = 0d }
                    }
                }
            }
        };

        if (AssociatedObject.RenderTransform is not TranslateTransform)
        {
            AssociatedObject.RenderTransform = new TranslateTransform();
        }

        animation.RunAsync(AssociatedObject);
    }
}