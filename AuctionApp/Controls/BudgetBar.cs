using System.Windows;
using System.Windows.Media;

namespace AuctionApp.Controls;

/// <summary>
/// A team's budget from 0 (left) to its starting budget (right), filled up to what's left. While the half budget cap
/// is on, the reserved half (from 0) is drawn hatched: that money can't be spent.
/// </summary>
public sealed class BudgetBar : FrameworkElement
{
    public static readonly DependencyProperty MaximumProperty = Register(nameof(Maximum), 1.0);
    public static readonly DependencyProperty ValueProperty = Register(nameof(Value), 0.0);
    public static readonly DependencyProperty ReservedProperty = Register(nameof(Reserved), 0.0);
    public static readonly DependencyProperty ShowReservedProperty = Register(nameof(ShowReserved), false);
    public static readonly DependencyProperty FillProperty = Register<Brush?>(nameof(Fill), null);
    public static readonly DependencyProperty TrackProperty = Register<Brush?>(nameof(Track), null);
    public static readonly DependencyProperty HatchProperty = Register<Brush?>(nameof(Hatch), null);

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>The part of the budget kept in reserve by the half budget cap.</summary>
    public double Reserved
    {
        get => (double)GetValue(ReservedProperty);
        set => SetValue(ReservedProperty, value);
    }

    public bool ShowReserved
    {
        get => (bool)GetValue(ShowReservedProperty);
        set => SetValue(ShowReservedProperty, value);
    }

    public Brush? Fill
    {
        get => (Brush?)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    public Brush? Track
    {
        get => (Brush?)GetValue(TrackProperty);
        set => SetValue(TrackProperty, value);
    }

    /// <summary>Color of the stripes over the reserved part.</summary>
    public Brush? Hatch
    {
        get => (Brush?)GetValue(HatchProperty);
        set => SetValue(HatchProperty, value);
    }

    protected override void OnRender(DrawingContext drawing)
    {
        var width = ActualWidth;
        var height = ActualHeight;
        if (width <= 0 || height <= 0)
        {
            return;
        }

        var radius = height / 2;
        var bar = new RectangleGeometry(new Rect(0, 0, width, height), radius, radius);
        drawing.PushClip(bar);

        drawing.PushOpacity(0.35);
        drawing.DrawRectangle(Track, null, new Rect(0, 0, width, height));
        drawing.Pop();

        var max = Maximum > 0 ? Maximum : 1;
        var filled = Math.Clamp(Value / max, 0, 1) * width;

        // The locked part keeps the empty track's color, with stripes: the money is there but can't be spent.
        var locked = ShowReserved && Reserved > 0 ? Math.Min(filled, Math.Clamp(Reserved / max, 0, 1) * width) : 0;
        if (filled > locked)
        {
            drawing.DrawRectangle(Fill, null, new Rect(locked, 0, filled - locked, height));
        }

        if (locked > 0)
        {
            drawing.PushOpacity(0.75);
            drawing.DrawRectangle(HatchBrush(), null, new Rect(0, 0, locked, height));
            drawing.Pop();
        }

        drawing.Pop();
    }

    /// <summary>Diagonal stripes, tiled every 6 pixels.</summary>
    private Brush HatchBrush()
    {
        var pen = new Pen(Hatch ?? Fill ?? Brushes.Gray, 2);
        var stripes = new GeometryDrawing(null, pen, new GeometryGroup
        {
            Children =
            {
                new LineGeometry(new Point(0, 6), new Point(6, 0)),
                new LineGeometry(new Point(-3, 3), new Point(3, -3)),
                new LineGeometry(new Point(3, 9), new Point(9, 3)),
            },
        });
        var brush = new DrawingBrush(stripes)
        {
            TileMode = TileMode.Tile,
            Viewport = new Rect(0, 0, 6, 6),
            ViewportUnits = BrushMappingMode.Absolute,
            Viewbox = new Rect(0, 0, 6, 6),
            ViewboxUnits = BrushMappingMode.Absolute,
            Stretch = Stretch.None,
        };
        // Not frozen: the stripe color can be a theme brush that follows the Windows accent color.
        return brush;
    }

    private static DependencyProperty Register<T>(string name, T defaultValue) =>
        DependencyProperty.Register(name, typeof(T), typeof(BudgetBar), new FrameworkPropertyMetadata(defaultValue, FrameworkPropertyMetadataOptions.AffectsRender));
}
