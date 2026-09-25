using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AuctionApp.Controls;

/// <summary>
/// Scales its content to fill the available height: down when it doesn't fit (never below <see cref="MinScale"/>),
/// and up to <see cref="MaxScale"/> when there's room, so it reads well on a shared screen. Used for the team cards,
/// which must all be visible without scrolling whatever the window size.
/// </summary>
public sealed class ScaleToFit : Decorator
{
    public static readonly DependencyProperty MinScaleProperty = DependencyProperty.Register(
        nameof(MinScale), typeof(double), typeof(ScaleToFit), new FrameworkPropertyMetadata(0.5, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty MaxScaleProperty = DependencyProperty.Register(
        nameof(MaxScale), typeof(double), typeof(ScaleToFit), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    private double _scale = 1;

    public double MaxScale
    {
        get => (double)GetValue(MaxScaleProperty);
        set => SetValue(MaxScaleProperty, value);
    }

    public double MinScale
    {
        get => (double)GetValue(MinScaleProperty);
        set => SetValue(MinScaleProperty, value);
    }

    protected override Size MeasureOverride(Size constraint)
    {
        if (Child == null)
        {
            return default;
        }

        Child.Measure(new Size(constraint.Width, double.PositiveInfinity));
        var height = Child.DesiredSize.Height;
        _scale = 1;
        if (!double.IsInfinity(constraint.Height) && height > 0)
        {
            // Scaled, the content gets a different width, which can change its layout: measure again once.
            _scale = Math.Clamp(constraint.Height / height, MinScale, MaxScale);
            if (Math.Abs(_scale - 1) > 0.001)
            {
                Child.Measure(new Size(constraint.Width / _scale, double.PositiveInfinity));
                height = Child.DesiredSize.Height;
                _scale = Math.Clamp(Math.Min(_scale, constraint.Height / height), MinScale, MaxScale);
            }
        }

        return new Size(
            double.IsInfinity(constraint.Width) ? Child.DesiredSize.Width * _scale : constraint.Width,
            Math.Min(height * _scale, double.IsInfinity(constraint.Height) ? double.MaxValue : constraint.Height));
    }

    protected override Size ArrangeOverride(Size arrangeSize)
    {
        if (Child == null)
        {
            return arrangeSize;
        }

        Child.Arrange(new Rect(0, 0, arrangeSize.Width / _scale, Child.DesiredSize.Height));
        if (Child.RenderTransform is not ScaleTransform current || Math.Abs(current.ScaleX - _scale) > 0.001)
        {
            Child.RenderTransform = Math.Abs(_scale - 1) > 0.001 ? new ScaleTransform(_scale, _scale) : Transform.Identity;
        }

        return arrangeSize;
    }
}
