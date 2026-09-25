using System.Windows;
using System.Windows.Controls;
using AuctionApp.Core.Storage;

namespace AuctionApp.Views;

public partial class PasteRosterDialog : Window
{
    public PasteRosterDialog()
    {
        InitializeComponent();
        Loaded += (_, _) => Input.Focus();
    }

    public List<ParsedPlayer> Players { get; private set; } = [];

    private void Input_TextChanged(object sender, TextChangedEventArgs e)
    {
        Players = RosterParser.Parse(Input.Text);
        var withClasses = Players.Count(player => player.Classes.Count > 0);
        Summary.Text = Players.Count switch
        {
            0 => "No players yet",
            1 => $"1 player found ({withClasses} with classes)",
            _ => $"{Players.Count} players found ({withClasses} with classes)",
        };
        AddButton.IsEnabled = Players.Count > 0;
    }

    private void Add_Click(object sender, RoutedEventArgs e) => DialogResult = true;
}
