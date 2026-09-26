using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AuctionApp.Controls;
using AuctionApp.Core.Model;

namespace AuctionApp.Views;

/// <summary>Captain Pick: edits the minimum bid of each tier (0.0 to 10.0, in steps of 0.1).</summary>
public partial class TierMinimumsDialog : Window
{
    private const decimal Highest = 10m;

    private readonly List<NumberBox> _boxes = [];

    public TierMinimumsDialog(IReadOnlyList<decimal> minimums)
    {
        InitializeComponent();
        foreach (var tier in Tiers.All)
        {
            Fields.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var label = new TextBlock { Text = Tiers.Name(tier), VerticalAlignment = VerticalAlignment.Center, Style = (Style)FindResource("Body") };
            var box = new NumberBox { Minimum = 0m, Maximum = Highest, Value = Math.Min(minimums[tier - 1], Highest), Margin = new Thickness(0, 0, 0, 6) };
            Grid.SetRow(label, tier - 1);
            Grid.SetRow(box, tier - 1);
            Grid.SetColumn(box, 1);
            Fields.Children.Add(label);
            Fields.Children.Add(box);
            _boxes.Add(box);
        }

        Loaded += (_, _) => _boxes[0].FocusText();
        PreviewKeyDown += (_, e) =>
        {
            // Enter saves (the default button isn't always triggered from a text box).
            if (e.Key == Key.Enter)
            {
                Ok_Click(this, e);
                e.Handled = true;
            }
        };
    }

    public IReadOnlyList<decimal> Minimums { get; private set; } = [];

    private void Defaults_Click(object sender, RoutedEventArgs e)
    {
        for (var i = 0; i < _boxes.Count; i++)
        {
            _boxes[i].Value = Tiers.DefaultMinimums[i];
        }
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        foreach (var box in _boxes)
        {
            box.Commit();
        }

        Minimums = _boxes.Select(box => box.Value).ToList();
        DialogResult = true;
    }
}
