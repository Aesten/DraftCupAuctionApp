using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AuctionApp.Services;

namespace AuctionApp.Views;

/// <summary>Picks one player from a list sorted by name (so it doesn't reveal the auction order), with instant search.</summary>
public partial class PlayerPickerDialog : Window
{
    private readonly IReadOnlyList<PlayerChoice> _players;

    public PlayerPickerDialog(string title, string message, IReadOnlyList<PlayerChoice> players)
    {
        InitializeComponent();
        Title = title;
        HeadingText.Text = title;
        MessageText.Text = message;
        _players = players.OrderBy(player => player.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
        PlayersList.ItemsSource = _players;
        Loaded += (_, _) => SearchBox.Focus();
    }

    public PlayerChoice? Picked { get; private set; }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var search = SearchBox.Text.Trim();
        SearchHint.Visibility = search.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
        PlayersList.ItemsSource = search.Length == 0
            ? _players
            : _players.Where(player => player.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase)).ToList();
        if (PlayersList.Items.Count == 1)
        {
            PlayersList.SelectedIndex = 0;
        }
    }

    /// <summary>Enter picks the selected player; Down arrow moves from the search box into the list.</summary>
    private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Ok_Click(sender, e);
            e.Handled = true;
        }
        else if (e.Key == Key.Down && PlayersList.Items.Count > 0)
        {
            PlayersList.SelectedIndex = Math.Max(0, PlayersList.SelectedIndex);
            (PlayersList.ItemContainerGenerator.ContainerFromIndex(PlayersList.SelectedIndex) as ListBoxItem)?.Focus();
            e.Handled = true;
        }
    }

    private void PlayersList_SelectionChanged(object sender, SelectionChangedEventArgs e) => OkButton.IsEnabled = PlayersList.SelectedItem != null;

    private void PlayersList_MouseDoubleClick(object sender, MouseButtonEventArgs e) => Ok_Click(sender, e);

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (PlayersList.SelectedItem is PlayerChoice player)
        {
            Picked = player;
            DialogResult = true;
        }
    }
}
