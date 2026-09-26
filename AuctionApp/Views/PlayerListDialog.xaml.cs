using System.Windows;
using AuctionApp.ViewModels;

namespace AuctionApp.Views;

public partial class PlayerListDialog : Window
{
    private readonly Action<PlayerItemViewModel>? _bringToBlock;

    /// <param name="bringToBlock">If set, each player gets an "On the block" button, which closes the list.</param>
    public PlayerListDialog(string title, string caption, IReadOnlyCollection<PlayerItemViewModel> players, Action<PlayerItemViewModel>? bringToBlock = null)
    {
        _bringToBlock = bringToBlock;
        InitializeComponent();
        Title = title;
        HeadingText.Text = title;
        CaptionText.Text = caption;
        PlayersList.ItemsSource = players;
        EmptyText.Visibility = players.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    public bool CanBringToBlock => _bringToBlock != null;

    private void BringToBlock_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: PlayerItemViewModel player })
        {
            _bringToBlock?.Invoke(player);
            Close();
        }
    }
}
