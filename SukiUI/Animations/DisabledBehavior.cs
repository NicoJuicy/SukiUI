using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using SukiUI.Animations;
using SukiUI.Helpers;

namespace SukiUI.Animations;

public static class DisabledBehavior
{
    private const int AnimationDurationMs = 400;
    
    private static readonly Dictionary<Avalonia.Controls.Control, double> _originalOpacities = new();
    
    public static readonly AttachedProperty<double?> OpacityProperty =
        AvaloniaProperty.RegisterAttached<Avalonia.Controls.Control, double?>(
            "Opacity",
            typeof(DisabledBehavior),
            defaultValue: null);

    static DisabledBehavior()
    {
        Avalonia.Controls.Control.IsEnabledProperty.Changed.AddClassHandler<Avalonia.Controls.Control>(OnIsEnabledChanged);
    }

    public static void SetOpacity(Avalonia.Controls.Control element, double? value)
        => element.SetValue(OpacityProperty, value);

    public static double? GetOpacity(Avalonia.Controls.Control element)
        => element.GetValue(OpacityProperty);

    private static void OnIsEnabledChanged(Avalonia.Controls.Control control, AvaloniaPropertyChangedEventArgs e)
    {
        var isEnabled = (bool)e.NewValue!;
        
        if (!isEnabled)
        {
            ApplyCustomOpacity(control);
        }
        else
        {
            RestoreOpacity(control);
        } 
    }

    private static void ApplyCustomOpacity(Avalonia.Controls.Control control)
    {
        var customOpacity = GetOpacity(control);
        
        if (customOpacity.HasValue && customOpacity.Value != 1.0)
        {
            _originalOpacities[control] = control.Opacity;
            
            ((Visual)control).Animate(Visual.OpacityProperty)
                .From(control.Opacity)
                .To(customOpacity.Value)
                .WithDuration(TimeSpan.FromMilliseconds(AnimationDurationMs))
                .Start();
        }
    }

    private static void RestoreOpacity(Avalonia.Controls.Control control)
    {
        if (_originalOpacities.TryGetValue(control, out var originalOpacity))
        {
            ((Visual)control).Animate(Visual.OpacityProperty)
                .From(control.Opacity)
                .To(originalOpacity)
                .WithDuration(TimeSpan.FromMilliseconds(AnimationDurationMs))
                .Start();
            
            _originalOpacities.Remove(control);
        }
    }
}
