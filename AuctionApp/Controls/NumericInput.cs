using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AuctionApp.Controls;

/// <summary>
/// Keeps a text box to amounts as they're typed: digits, one decimal separator ("." or ","), at most two digits
/// before it and one after (so up to "30.0"). Anything else, typed or pasted, is ignored. Whether the amount is in
/// range is checked by whoever uses it.
/// </summary>
public static partial class NumericInput
{
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled", typeof(bool), typeof(NumericInput), new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);

    [GeneratedRegex(@"^\d{0,2}([.,]\d?)?$")]
    private static partial Regex Amount();

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox box)
        {
            return;
        }

        if (e.NewValue is true)
        {
            box.PreviewTextInput += OnPreviewTextInput;
            box.PreviewKeyDown += OnPreviewKeyDown;
            DataObject.AddPastingHandler(box, OnPasting);
        }
        else
        {
            box.PreviewTextInput -= OnPreviewTextInput;
            box.PreviewKeyDown -= OnPreviewKeyDown;
            DataObject.RemovePastingHandler(box, OnPasting);
        }
    }

    /// <summary>The text the box would hold once <paramref name="input"/> replaces the selection.</summary>
    private static string After(TextBox box, string input) =>
        box.Text.Remove(box.SelectionStart, box.SelectionLength).Insert(box.SelectionStart, input);

    private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is TextBox box && !Amount().IsMatch(After(box, e.Text)))
        {
            e.Handled = true;
        }
    }

    /// <summary>Space doesn't go through text input.</summary>
    private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            e.Handled = true;
        }
    }

    private static void OnPasting(object sender, DataObjectPastingEventArgs e)
    {
        var pasted = e.DataObject.GetDataPresent(DataFormats.UnicodeText) ? (e.DataObject.GetData(DataFormats.UnicodeText) as string)?.Trim() : null;
        if (sender is not TextBox box || pasted == null || !Amount().IsMatch(After(box, pasted)))
        {
            e.CancelCommand();
        }
    }
}
