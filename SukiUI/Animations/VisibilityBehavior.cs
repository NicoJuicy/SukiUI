using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Animation.Easings;
using Avalonia.VisualTree;

namespace SukiUI.Animations;

public static class VisibilityBehavior
{
    private const int AnimationDurationMs = 300;
    
    public static readonly AttachedProperty<bool> IsVisibleProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>(
            "IsVisible",
            typeof(VisibilityBehavior),
            defaultValue: true);

    public static readonly AttachedProperty<double> HiddenScaleProperty =
        AvaloniaProperty.RegisterAttached<Control, double>(
            "HiddenScale",
            typeof(VisibilityBehavior),
            defaultValue: 1.0);

    static VisibilityBehavior()
    {
        IsVisibleProperty.Changed.AddClassHandler<Control>(OnIsVisibleChanged);
    }

    public static void SetIsVisible(Control element, bool value)
        => element.SetValue(IsVisibleProperty, value);

    public static bool GetIsVisible(Control element)
        => element.GetValue(IsVisibleProperty);

    public static void SetHiddenScale(Control element, double value)
        => element.SetValue(HiddenScaleProperty, value);

    public static double GetHiddenScale(Control element)
        => element.GetValue(HiddenScaleProperty);

    private static void OnIsVisibleChanged(Control control, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not bool newValue) return;
        var hiddenScale = GetHiddenScale(control);

        if (!control.IsAttachedToVisualTree())
        {
            control.IsVisible = newValue;
            control.Opacity = newValue ? 1 : 0;
            SetScale(control, newValue ? 1.0 : hiddenScale);
            return;
        }

        if (newValue)
        {
            control.IsVisible = true;
            RunAnimation(control, 0, 1, hiddenScale, 1.0);
        }
        else
        {
            RunAnimation(control, 1, 0, 1.0, hiddenScale, () =>
            {
                if (!control.GetValue(IsVisibleProperty))
                    control.IsVisible = false;
            });
        }
    }

    private static void SetScale(Control control, double scale)
    {
        var transform = control.RenderTransform as ScaleTransform;
        if (transform == null)
        {
            transform = new ScaleTransform(scale, scale);
            control.RenderTransform = transform;
            control.RenderTransformOrigin = RelativePoint.Center;
        }
        else
        {
            transform.ScaleX = scale;
            transform.ScaleY = scale;
        }
    }

    private static void RunAnimation(Control control, double opacityFrom, double opacityTo, 
        double scaleFrom, double scaleTo, Action? onComplete = null)
    {
        SetScale(control, scaleFrom);

        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(AnimationDurationMs),
            Easing = new CubicEaseInOut(),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0.0),
                    Setters =
                    {
                        new Setter(Visual.OpacityProperty, opacityFrom),
                        new Setter(ScaleTransform.ScaleXProperty, scaleFrom),
                        new Setter(ScaleTransform.ScaleYProperty, scaleFrom)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1.0),
                    Setters =
                    {
                        new Setter(Visual.OpacityProperty, opacityTo),
                        new Setter(ScaleTransform.ScaleXProperty, scaleTo),
                        new Setter(ScaleTransform.ScaleYProperty, scaleTo)
                    }
                }
            }
        };

        animation.RunAsync(control).ContinueWith(_ =>
        {
            if (onComplete != null)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(onComplete);
            }
        });
    }
}

