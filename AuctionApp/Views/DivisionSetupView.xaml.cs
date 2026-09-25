using System.Windows;
using System.Windows.Controls;
using AuctionApp.ViewModels;

namespace AuctionApp.Views;

public partial class DivisionSetupView : UserControl
{
    public DivisionSetupView()
    {
        InitializeComponent();
    }

    /// <summary>Puts the cursor in the name box of a captain that was just added.</summary>
    private void CaptainName_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox { DataContext: CaptainRowViewModel { FocusRequested: true } row } box)
        {
            row.FocusRequested = false;
            box.Focus();
        }
    }
}
