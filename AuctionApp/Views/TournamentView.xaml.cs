using System.Windows.Controls;
using System.Windows.Input;
using AuctionApp.ViewModels;

namespace AuctionApp.Views;

public partial class TournamentView : UserControl
{
    public TournamentView()
    {
        InitializeComponent();
    }

    private void Title_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => (DataContext as TournamentViewModel)?.CommitTitle();
}
