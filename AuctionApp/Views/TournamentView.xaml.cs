using System.Windows.Controls;
using AuctionApp.ViewModels;

namespace AuctionApp.Views;

public partial class TournamentView : UserControl
{
    public TournamentView()
    {
        InitializeComponent();
    }

    /// <summary>The title box was left (the focus is outside it and its clear button): an emptied title comes back.</summary>
    private void Title_FocusWithinChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is false)
        {
            (DataContext as TournamentViewModel)?.CommitTitle();
        }
    }
}
