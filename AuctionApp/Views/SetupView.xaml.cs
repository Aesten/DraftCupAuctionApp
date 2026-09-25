using System.Windows;
using System.Windows.Controls;
using AuctionApp.ViewModels;

namespace AuctionApp.Views;

public partial class SetupView : UserControl
{
    public SetupView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Commits the cell being edited as soon as the focus leaves the grid (search box, buttons, other pages),
    /// so a name that was just typed is never lost.
    /// </summary>
    private void PlayersGrid_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is false)
        {
            PlayersGrid.CommitEdit(DataGridEditingUnit.Row, exitEditingMode: true);
        }
    }

    private void PlayersGrid_Unloaded(object sender, RoutedEventArgs e) =>
        PlayersGrid.CommitEdit(DataGridEditingUnit.Row, exitEditingMode: true);

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
