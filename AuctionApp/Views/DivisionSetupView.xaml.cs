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

    private void DivisionName_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e) =>
        (DataContext as DivisionSetupViewModel)?.CommitName();

    private void CaptainBox_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e) =>
        ((sender as FrameworkElement)?.DataContext as CaptainRowViewModel)?.CommitEdits();

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
