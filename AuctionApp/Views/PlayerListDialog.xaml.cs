using System.Windows;
using AuctionApp.ViewModels;

namespace AuctionApp.Views;

public partial class PlayerListDialog : Window
{
    public PlayerListDialog(string title, string caption, IReadOnlyCollection<PlayerItemViewModel> players)
    {
        InitializeComponent();
        Title = title;
        HeadingText.Text = title;
        CaptionText.Text = caption;
        PlayersList.ItemsSource = players;
        EmptyText.Visibility = players.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }
}
