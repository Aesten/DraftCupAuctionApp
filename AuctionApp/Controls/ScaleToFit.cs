using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AuctionApp.Controls;

/// <summary>
/// Scales its content to fill the available height: down when it doesn't fit (never below <see cref="MinScale"/>),
/// and up to <see cref="MaxScale"/> when there's room, so it reads well on a shared screen. Used for the team cards,
/// which must all be visible without scrolling whatever the window size, and for the pick board.
/// </summary>
public sealed class ScaleToFit : Decorator
{
    public static readonly DependencyProperty MinScaleProperty = DependencyProperty.Register(
        nameof(MinScale), typeof(double), typeof(ScaleToFit), new FrameworkPropertyMetadata(0.5, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty MaxScaleProperty = DependencyProperty.Register(
        nameof(MaxScale), typeof(double), typeof(ScaleToFit), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    private double _scale = 1;
    private Size _lastConstraint = Size.Empty;
    private double _lastHeight;

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

        if (double.IsInfinity(constraint.Height))
        {
            _scale = 1;
            var natural = HeightAt(constraint.Width, 1);
            return new Size(double.IsInfinity(constraint.Width) ? Child.DesiredSize.Width : constraint.Width, natural);
        }

        // Fast path, the usual case during an auction (a sale, a player picked...): same space as last time, and the
        // content still fits at the current scale without having shrunk (which could leave room to grow). One measure
        // instead of a search; the search runs again when the window is resized or the content grows or shrinks.
        if (constraint == _lastConstraint)
        {
            var current = HeightAt(constraint.Width, _scale) * _scale;
            if (current <= constraint.Height + 0.5 && current >= _lastHeight - 1)
            {
                _lastHeight = current;
                return new Size(constraint.Width, Math.Min(current, constraint.Height));
            }
        }

        _lastConstraint = constraint;
        _scale = 1;
        var height = HeightAt(constraint.Width, 1);
        if (height > 0)
        {
            // Scaled, the content gets a different width, which can change its layout (wrapping, columns): look for
            // the largest scale at which it fits, between the limits.
            var target = Math.Clamp(constraint.Height / height, MinScale, MaxScale);
            if (Math.Abs(target - 1) > 0.001)
            {
                var (low, high) = target > 1 ? (1.0, target) : (MinScale, 1.0);
                if (Fits(constraint, high))
                {
                    low = high;
                }
                else
                {
                    for (var i = 0; i < 7 && high - low > 0.01; i++)
                    {
                        var middle = (low + high) / 2;
                        (low, high) = Fits(constraint, middle) ? (middle, high) : (low, middle);
                    }
                }

                _scale = low;
                height = HeightAt(constraint.Width, _scale);
            }
        }

        _lastHeight = height * _scale;
        return new Size(
            double.IsInfinity(constraint.Width) ? Child.DesiredSize.Width * _scale : constraint.Width,
            Math.Min(height * _scale, constraint.Height));
    }

    /// <summary>The content's height, unscaled, when laid out for this width at this scale.</summary>
    private double HeightAt(double width, double scale)
    {
        Child!.Measure(new Size(width / scale, double.PositiveInfinity));
        return Child.DesiredSize.Height;
    }

    private bool Fits(Size constraint, double scale) => HeightAt(constraint.Width, scale) * scale <= constraint.Height + 0.5;

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
