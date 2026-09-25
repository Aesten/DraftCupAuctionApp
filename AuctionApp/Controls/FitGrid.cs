using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace AuctionApp.Controls;

/// <summary>
/// Lays out team cards in a balanced grid chosen so that every card fits in the available height when possible:
/// 8 teams go 4 × 2, and if their rosters are too tall for that, 8 × 1 on a wide enough screen.
/// Rows take the height of their tallest card, so rosters of any size line up.
/// </summary>
public sealed class FitGrid : Panel
{
    public static readonly DependencyProperty MinItemWidthProperty = DependencyProperty.Register(
        nameof(MinItemWidth), typeof(double), typeof(FitGrid), new FrameworkPropertyMetadata(220.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(
        nameof(Spacing), typeof(double), typeof(FitGrid), new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>The height to fit in, typically the viewport of the surrounding scroll viewer.</summary>
    public static readonly DependencyProperty FitHeightProperty = DependencyProperty.Register(
        nameof(FitHeight), typeof(double), typeof(FitGrid), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure));

    private int _columns = 1;
    private double _itemWidth;
    private double[] _rowHeights = [];

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

    public double FitHeight
    {
        get => (double)GetValue(FitHeightProperty);
        set => SetValue(FitHeightProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var count = InternalChildren.Count;
        if (count == 0)
        {
            _rowHeights = [];
            return default;
        }

        var width = double.IsInfinity(availableSize.Width) ? count * (MinItemWidth + Spacing) - Spacing : availableSize.Width;
        var height = FitHeight > 0 ? FitHeight : availableSize.Height;
        var maxColumns = Math.Clamp((int)((width + Spacing) / (MinItemWidth + Spacing)), 1, count);

        // Work in rows so every row is (nearly) full: 4 teams that can't fit side by side go 2 × 2, not 3 + 1.
        // Start from rows of at most 4–5 cards, then use fewer, wider rows until everything fits in the height.
        var minRows = Rows(count, maxColumns);
        var rows = Math.Max(minRows, count <= 4 ? 1 : (int)Math.Ceiling(count / 5.0));
        var columns = Rows(count, rows);
        var total = MeasureWith(columns, width);
        while (total > height && rows > minRows)
        {
            rows--;
            columns = Rows(count, rows);
            total = MeasureWith(columns, width);
        }

        _columns = columns;
        return new Size(width, total);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var y = 0.0;
        for (var i = 0; i < InternalChildren.Count; i++)
        {
            var row = i / _columns;
            var column = i % _columns;
            if (column == 0 && row > 0)
            {
                y += _rowHeights[row - 1] + Spacing;
            }

            InternalChildren[i].Arrange(new Rect(column * (_itemWidth + Spacing), y, _itemWidth, _rowHeights[row]));
        }

        return finalSize;
    }

    private static int Rows(int count, int columns) => (count + columns - 1) / columns;

    private double MeasureWith(int columns, double width)
    {
        var count = InternalChildren.Count;
        _itemWidth = Math.Max(0, (width - Spacing * (columns - 1)) / columns);
        _rowHeights = new double[Rows(count, columns)];
        for (var i = 0; i < count; i++)
        {
            var child = InternalChildren[i];
            child.Measure(new Size(_itemWidth, double.PositiveInfinity));
            _rowHeights[i / columns] = Math.Max(_rowHeights[i / columns], child.DesiredSize.Height);
        }

        return _rowHeights.Sum() + Spacing * (_rowHeights.Length - 1);
    }
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
