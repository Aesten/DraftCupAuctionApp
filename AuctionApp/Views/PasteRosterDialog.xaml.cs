using System.Windows;
using System.Windows.Controls;
using AuctionApp.Core.Storage;

namespace AuctionApp.Views;

public sealed record StageChoice(Guid Id, string Name);

public partial class PasteRosterDialog : Window
{
    public PasteRosterDialog(IReadOnlyList<StageChoice> stages)
    {
        InitializeComponent();
        StagePicker.ItemsSource = stages;
        StagePicker.SelectedIndex = 0;
        var showStages = stages.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        StagePicker.Visibility = showStages;
        StageLabel.Visibility = showStages;
        Loaded += (_, _) => Input.Focus();
    }

    public List<ParsedPlayer> Players { get; private set; } = [];

    public Guid StageId => (StagePicker.SelectedItem as StageChoice)?.Id ?? Guid.Empty;

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
