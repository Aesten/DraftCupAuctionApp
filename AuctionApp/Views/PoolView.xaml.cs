using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using AuctionApp.ViewModels;

namespace AuctionApp.Views;

/// <summary>The player pool grid. Code-behind handles what XAML can't: showing added players and committing edits.</summary>
public partial class PoolView : UserControl
{
    public PoolView()
    {
        InitializeComponent();
        DataContextChanged += (_, e) =>
        {
            if (e.OldValue is PoolViewModel oldModel)
            {
                oldModel.PlayerAdded -= ShowPlayer;
            }

            if (e.NewValue is PoolViewModel newModel)
            {
                newModel.PlayerAdded += ShowPlayer;
            }
        };
    }

    private PoolViewModel? ViewModel => DataContext as PoolViewModel;

    /// <summary>Selects a player that was just added (or already listed) so the auctioneer sees it, then back to the add box.</summary>
    private void ShowPlayer(PoolPlayerRowViewModel row)
    {
        PlayersGrid.SelectedItem = row;
        PlayersGrid.ScrollIntoView(row);
        NewPlayerBox.Focus();
    }

    /// <summary>Export… asks for the format (CSV or JSON) with a small menu.</summary>
    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (ExportButton.ContextMenu is { } menu)
        {
            menu.PlacementTarget = ExportButton;
            menu.Placement = PlacementMode.Bottom;
            menu.IsOpen = true;
        }
    }

    /// <summary>Delete asks before removing players who were already bought.</summary>
    private void PlayersGrid_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete && !IsEditingCell() && ViewModel is { } viewModel)
        {
            var rows = PlayersGrid.SelectedItems.OfType<PoolPlayerRowViewModel>().ToList();
            if (!viewModel.ConfirmRemoval(rows))
            {
                e.Handled = true;
            }
        }
    }

    private static bool IsEditingCell() => Keyboard.FocusedElement is TextBox box && FindAncestor<DataGridCell>(box) != null;

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

    private static T? FindAncestor<T>(DependencyObject? element)
        where T : DependencyObject
    {
        while (element != null && element is not T)
        {
            element = element is Visual or System.Windows.Media.Media3D.Visual3D ? VisualTreeHelper.GetParent(element) : LogicalTreeHelper.GetParent(element);
        }

        return element as T;
    }
}
