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

    // A box counts as left once the focus is outside it and its clear (×) button, which takes the focus when clicked.

    private void DivisionName_FocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is false)
        {
            (DataContext as DivisionSetupViewModel)?.CommitName();
        }
    }

    private void CaptainBox_FocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is false)
        {
            ((sender as FrameworkElement)?.DataContext as CaptainRowViewModel)?.CommitEdits();
        }
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
