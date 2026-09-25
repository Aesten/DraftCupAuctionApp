using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AuctionApp.Core.Model;

namespace AuctionApp.Controls;

/// <summary>A Fluent icon glyph followed by a label, for button contents.</summary>
public sealed class IconLabel : StackPanel
{
    public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register(
        nameof(Glyph), typeof(string), typeof(IconLabel), new PropertyMetadata(string.Empty, (d, e) => ((IconLabel)d)._glyph.Text = (string)e.NewValue));

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(IconLabel), new PropertyMetadata(string.Empty, (d, e) => ((IconLabel)d).UpdateText()));

    private readonly TextBlock _glyph = new() { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
    private readonly TextBlock _text = new() { Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };

    public IconLabel()
    {
        Orientation = Orientation.Horizontal;
        _glyph.SetResourceReference(TextBlock.FontFamilyProperty, "SymbolThemeFontFamily");
        Children.Add(_glyph);
        Children.Add(_text);
        UpdateText();
    }

    public string Glyph
    {
        get => (string)GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private void UpdateText()
    {
        _text.Text = Text;
        _text.Visibility = string.IsNullOrEmpty(Text) ? Visibility.Collapsed : Visibility.Visible;
    }
}

/// <summary>A row of class icons for a player. Its items are class codes such as "inf".</summary>
public sealed class ClassIconStrip : ItemsControl
{
    public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(
        nameof(IconSize), typeof(double), typeof(ClassIconStrip), new PropertyMetadata(16.0));

    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }
}

public sealed class ClassIconConverter : IValueConverter
{
    private static readonly Dictionary<string, ImageSource> Cache = new();

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string code || !PlayerClasses.All.Contains(code))
        {
            return null;
        }

        if (!Cache.TryGetValue(code, out var image))
        {
            var bitmap = new BitmapImage(new Uri($"pack://application:,,,/Assets/Classes/{code}.png"));
            bitmap.Freeze();
            Cache[code] = image = bitmap;
        }

        return image;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class ClassNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is string code ? PlayerClasses.DisplayName(code) : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

/// <summary>Visible when true, hidden (still taking its space) when false, so layouts don't move.</summary>
public sealed class BoolToHiddenConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? Visibility.Visible : Visibility.Hidden;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

/// <summary>Visible when the value is set (non-null, non-empty string), collapsed otherwise.</summary>
public sealed class NotEmptyToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        var isEmpty = value is null || value is string { Length: 0 } || value is int count && count == 0;
        return isEmpty != Invert ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

/// <summary>Carries a DataContext to places outside the visual tree, such as DataGrid columns.</summary>
public sealed class BindingProxy : Freezable
{
    public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
        nameof(Data), typeof(object), typeof(BindingProxy));

    public object? Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    protected override Freezable CreateInstanceCore() => new BindingProxy();
}

public sealed class Negate : IValueConverter
{
    public static Negate Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is not true;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is not true;
}

/// <summary>True for the grid's empty "type here to add" row, which has no real item behind it.</summary>
public sealed class IsNewItemPlaceholderConverter : IValueConverter
{
    public static IsNewItemPlaceholderConverter Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        ReferenceEquals(value, CollectionView.NewItemPlaceholder);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

/// <summary>Visible when the bound number equals the converter parameter (to show one page out of several).</summary>
public sealed class IndexToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is int index && int.TryParse(parameter?.ToString(), out var expected) && index == expected ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

/// <summary>Each class's color, used to tint the frame around its icon (parameter: opacity, e.g. "0.25").</summary>
public sealed class ClassBrushConverter : IValueConverter
{
    private static readonly Dictionary<string, Color> Colors = new()
    {
        [PlayerClasses.Infantry] = Color.FromRgb(0xE0, 0x5A, 0x4F),
        [PlayerClasses.Archer] = Color.FromRgb(0x4C, 0xAF, 0x6A),
        [PlayerClasses.Cavalry] = Color.FromRgb(0x4A, 0x8F, 0xE0),
    };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var color = value is string code && Colors.TryGetValue(code, out var known) ? known : System.Windows.Media.Colors.Gray;
        var opacity = double.TryParse(parameter?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var o) ? o : 1.0;
        var brush = new SolidColorBrush(color) { Opacity = opacity };
        brush.Freeze();
        return brush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

/// <summary>A padding proportional to a size (parameter: the fraction), e.g. the space around a class icon in its frame.</summary>
public sealed class ProportionalThicknessConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var fraction = double.TryParse(parameter?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var f) ? f : 0.15;
        return value is double size ? new Thickness(Math.Round(size * fraction)) : new Thickness(0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

/// <summary>Maps a bool to 0/1, to bind a two-option segmented switch's selected index.</summary>
public sealed class BoolToIndexConverter : IValueConverter
{
    public static BoolToIndexConverter Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is true ? 1 : 0;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is 1;
}
