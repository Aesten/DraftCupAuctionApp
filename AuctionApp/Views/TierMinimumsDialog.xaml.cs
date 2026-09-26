using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AuctionApp.Core.Engine;
using AuctionApp.Core.Model;

namespace AuctionApp.Views;

/// <summary>Captain Pick: edits the minimum bid of each tier. Accepts "1.5" as well as "1,5".</summary>
public partial class TierMinimumsDialog : Window
{
    private readonly List<TextBox> _boxes = [];

    public TierMinimumsDialog(IReadOnlyList<decimal> minimums)
    {
        InitializeComponent();
        foreach (var tier in Tiers.All)
        {
            Fields.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var label = new TextBlock { Text = Tiers.Name(tier), VerticalAlignment = VerticalAlignment.Center, Style = (Style)FindResource("Body") };
            var box = new TextBox { Text = Money.Format(minimums[tier - 1]), HorizontalContentAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, 0, 6) };
            box.TextChanged += (_, _) => Validate();
            box.PreviewKeyDown += Box_PreviewKeyDown;
            Grid.SetRow(label, tier - 1);
            Grid.SetRow(box, tier - 1);
            Grid.SetColumn(box, 1);
            Fields.Children.Add(label);
            Fields.Children.Add(box);
            _boxes.Add(box);
        }

        Loaded += (_, _) =>
        {
            _boxes[0].Focus();
            _boxes[0].SelectAll();
        };
    }

    public IReadOnlyList<decimal> Minimums { get; private set; } = [];

    private List<decimal>? Parse()
    {
        var values = new List<decimal>();
        foreach (var box in _boxes)
        {
            if (!Money.TryParse(box.Text, out var value) || value < 0 || !Money.IsWholeStep(value))
            {
                return null;
            }

            values.Add(value);
        }

        return values;
    }

    private void Validate()
    {
        var valid = Parse() != null;
        ProblemText.Text = valid ? string.Empty : $"Enter amounts in steps of {Money.Format(Money.Step)}.";
        OkButton.IsEnabled = valid;
    }

    private void Defaults_Click(object sender, RoutedEventArgs e)
    {
        for (var i = 0; i < _boxes.Count; i++)
        {
            _boxes[i].Text = Money.Format(Tiers.DefaultMinimums[i]);
        }
    }

    /// <summary>Enter saves (the default button isn't always triggered from a text box).</summary>
    private void Box_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Ok_Click(sender, e);
            e.Handled = true;
        }
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (OkButton.IsEnabled && Parse() is { } values)
        {
            Minimums = values;
            DialogResult = true;
        }
    }
}
