using System.Windows;
using AuctionApp.ViewModels;

namespace AuctionApp.Views;

/// <summary>The full skipped list. Like in the old app, it closes once a choice is made so the auction goes on.</summary>
public partial class SkippedDialog : Window
{
    public SkippedDialog()
    {
        InitializeComponent();
    }

    private AuctionViewModel? ViewModel => DataContext as AuctionViewModel;

    private void BringBack_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: PlayerItemViewModel player } && ViewModel?.BringBackCommand.CanExecute(player) == true)
        {
            ViewModel.BringBackCommand.Execute(player);
            Close();
        }
    }

    private void RequeueAll_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.RequeueSkippedCommand.Execute(null);
        Close();
    }
}
