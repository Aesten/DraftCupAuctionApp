using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using AuctionApp.Core.Engine;

namespace AuctionApp.Controls;

/// <summary>
/// An amount in millions, in steps of 0.1, between <see cref="Minimum"/> and <see cref="Maximum"/>: − / + buttons,
/// Up / Down keys and the mouse wheel change it by 0.1. Typed text is taken when Enter is pressed or the box is left:
/// "1,5" and "1.5" both work, it's rounded to 0.1 and kept in range, and anything else puts the last value back.
/// </summary>
public sealed class NumberBox : Grid
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(decimal), typeof(NumberBox),
        new FrameworkPropertyMetadata(0m, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, _) => ((NumberBox)d).ShowValue()));

    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
        nameof(Minimum), typeof(decimal), typeof(NumberBox), new PropertyMetadata(0m));

    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        nameof(Maximum), typeof(decimal), typeof(NumberBox), new PropertyMetadata(100m));

    private readonly TextBox _box;

    public NumberBox()
    {
        ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        _box = new TextBox { Margin = new Thickness(4, 0, 4, 0) };
        _box.SetResourceReference(StyleProperty, "NumericTextBox");
        _box.PreviewKeyDown += Box_PreviewKeyDown;
        _box.LostKeyboardFocus += (_, _) => Commit();
        _box.GotKeyboardFocus += (_, _) => _box.Dispatcher.BeginInvoke(_box.SelectAll);
        SetColumn(_box, 1);

        Children.Add(StepButton("−", -Money.Step, 0));
        Children.Add(_box);
        Children.Add(StepButton("+", Money.Step, 2));
        ShowValue();
    }

    public decimal Value
    {
        get => (decimal)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public decimal Minimum
    {
        get => (decimal)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public decimal Maximum
    {
        get => (decimal)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>Puts the cursor in the box, its value selected.</summary>
    public void FocusText() => _box.Focus();

    /// <summary>Takes what was typed (also done when the box is left).</summary>
    public void Commit()
    {
        if (Money.TryParse(_box.Text, out var typed))
        {
            SetClamped(typed);
        }

        ShowValue();
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        if (_box.IsKeyboardFocusWithin)
        {
            Step(e.Delta > 0 ? Money.Step : -Money.Step);
            e.Handled = true;
        }
    }

    private RepeatButton StepButton(string text, decimal step, int column)
    {
        var button = new RepeatButton { Content = text, Width = 32, FontSize = 15, Focusable = false, VerticalAlignment = VerticalAlignment.Stretch };
        button.Click += (_, _) => Step(step);
        SetColumn(button, column);
        return button;
    }

    private void Step(decimal step)
    {
        Commit();
        SetClamped(Value + step);
        if (_box.IsKeyboardFocusWithin)
        {
            _box.SelectAll();
        }
    }

    private void SetClamped(decimal value) => Value = Math.Clamp(decimal.Round(value, 1, MidpointRounding.AwayFromZero), Minimum, Maximum);

    private void ShowValue() => _box.Text = Money.Format(Value);

    private void Box_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Up:
                Step(Money.Step);
                e.Handled = true;
                break;
            case Key.Down:
                Step(-Money.Step);
                e.Handled = true;
                break;
            case Key.Enter:
                // Takes the typed value; the key goes on (e.g. to the dialog's default button).
                Commit();
                break;
        }
    }
}
