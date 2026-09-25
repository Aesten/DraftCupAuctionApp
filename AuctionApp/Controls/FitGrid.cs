using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace AuctionApp.Controls;

/// <summary>
/// Lays out team cards in a balanced grid, sized for the usual 8 teams (4 × 2): up to 4 teams go on one row,
/// up to 10 on two rows, more on three. Fewer columns are used when the cards would get narrower than
/// <see cref="MinItemWidth"/>. All cards get the same height so rosters line up. Wrap it in a
/// <see cref="ScaleToFit"/> so it never needs scrolling.
/// </summary>
public sealed class FitGrid : Panel
{
    public static readonly DependencyProperty MinItemWidthProperty = DependencyProperty.Register(
        nameof(MinItemWidth), typeof(double), typeof(FitGrid), new FrameworkPropertyMetadata(220.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(
        nameof(Spacing), typeof(double), typeof(FitGrid), new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    private int _columns = 1;
    private double _itemWidth;
    private double _itemHeight;

    public double MinItemWidth
    {
        get => (double)GetValue(MinItemWidthProperty);
        set => SetValue(MinItemWidthProperty, value);
    }

    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var count = InternalChildren.Count;
        if (count == 0)
        {
            return default;
        }

        var width = double.IsInfinity(availableSize.Width) ? count * (MinItemWidth + Spacing) - Spacing : availableSize.Width;
        var columns = count <= 4 ? count : count <= 10 ? Rows(count, 2) : Rows(count, 3);
        var maxColumns = Math.Max(1, (int)((width + Spacing) / (MinItemWidth + Spacing)));
        if (columns > maxColumns)
        {
            // Too narrow: more rows, kept as even as possible (e.g. 2 × 2 rather than 3 + 1).
            columns = Rows(count, Rows(count, maxColumns));
        }

        _columns = columns;
        _itemWidth = Math.Max(0, (width - Spacing * (columns - 1)) / columns);
        _itemHeight = 0;
        foreach (UIElement child in InternalChildren)
        {
            child.Measure(new Size(_itemWidth, double.PositiveInfinity));
            _itemHeight = Math.Max(_itemHeight, child.DesiredSize.Height);
        }

        var rows = Rows(count, columns);
        return new Size(width, rows * _itemHeight + (rows - 1) * Spacing);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        for (var i = 0; i < InternalChildren.Count; i++)
        {
            var row = i / _columns;
            var column = i % _columns;
            InternalChildren[i].Arrange(new Rect(column * (_itemWidth + Spacing), row * (_itemHeight + Spacing), _itemWidth, _itemHeight));
        }

        return finalSize;
    }

    private static int Rows(int count, int columns) => (count + columns - 1) / columns;
}

/// <summary>The line showing where a dragged row will be dropped.</summary>
public sealed class InsertionAdorner : Adorner
{
    private readonly Pen _pen;
    private double _y;

    public InsertionAdorner(UIElement adornedElement, Brush brush)
        : base(adornedElement)
    {
        IsHitTestVisible = false;
        _pen = new Pen(brush, 2);
        _pen.Freeze();
    }

    public void MoveTo(double y)
    {
        _y = y;
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext drawingContext) =>
        drawingContext.DrawLine(_pen, new Point(4, _y), new Point(AdornedElement.RenderSize.Width - 4, _y));
}
