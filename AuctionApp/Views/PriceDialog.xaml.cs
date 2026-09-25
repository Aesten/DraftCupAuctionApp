using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AuctionApp.Core.Engine;

namespace AuctionApp.Views;

/// <summary>Asks for a price; accepts "2.5" as well as "2,5".</summary>
public partial class PriceDialog : Window
{
    public PriceDialog(string title, string message, decimal initial)
    {
        InitializeComponent();
        Title = title;
        HeadingText.Text = title;
        MessageText.Text = message;
        PriceBox.Text = Money.Format(initial);
        Loaded += (_, _) =>
        {
            PriceBox.Focus();
            PriceBox.SelectAll();
        };
    }

    public decimal Price { get; private set; }

    private void PriceBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var valid = Money.TryParse(PriceBox.Text, out var price) && price >= 0 && Money.IsWholeStep(price);
        ProblemText.Text = valid ? string.Empty : $"Enter a price in steps of {Money.Format(Money.Step)}.";
        OkButton.IsEnabled = valid;
    }

    private void PriceBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Ok_Click(sender, e);
            e.Handled = true;
        }
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (OkButton.IsEnabled && Money.TryParse(PriceBox.Text, out var price))
        {
            Price = price;
            DialogResult = true;
        }
    }
}
