using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Animation.Easings;
using Avalonia.VisualTree;

namespace SukiUI.Animations;

public static class AnimatedVisibility
    {
        
        private const int AnimationDurationMs = 300;
        public static readonly AttachedProperty<bool> IsVisibleProperty =
            AvaloniaProperty.RegisterAttached<Control, bool>(
                "IsVisibleAnimated",
                typeof(AnimatedVisibility),
                defaultValue: true); 

        static AnimatedVisibility()
        {
            IsVisibleProperty.Changed.AddClassHandler<Control>(OnIsVisibleChanged);
        }
        
        public static void SetIsVisible(Control element, bool value) 
            => element.SetValue(IsVisibleProperty, value);

        public static bool GetIsVisible(Control element) 
            => element.GetValue(IsVisibleProperty);

        private static void OnIsVisibleChanged(Control control, AvaloniaPropertyChangedEventArgs e)
        {
            var newValue = (bool) e.NewValue;

            if (!control.IsAttachedToVisualTree())
            {
                control.IsVisible = newValue;
                control.Opacity = newValue ? 1 : 0;
                return;
            }
            
            if (newValue)
            {
                control.IsVisible = true;
                RunAnimation(control, 0, 1);
            }
            else
            {
                RunAnimation(control, 1, 0, () =>
                {
                    if (!control.GetValue(IsVisibleProperty))
                        control.IsVisible = false;
                });
            }
        }

        private static void RunAnimation(Control control, double from, double to, Action? onComplete = null)
        {
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
                        Setters = { new Setter(Visual.OpacityProperty, from) }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1.0),
                        Setters = { new Setter(Visual.OpacityProperty, to) }
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

