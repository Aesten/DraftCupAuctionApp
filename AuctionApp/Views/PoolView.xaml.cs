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

                // Grid columns aren't in the visual tree, so the tier column is shown from here (Captain Pick only).
                TierColumn.Visibility = newModel.IsCaptainPick ? Visibility.Visible : Visibility.Collapsed;
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

    /// <summary>
    /// Delete asks before removing players who were already bought. In Captain Pick, 1 to 5 set the tier of the
    /// selected players, to go through a list quickly.
    /// </summary>
    private void PlayersGrid_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (TierKey(e.Key) is { } tier && !IsEditingCell() && ViewModel is { IsCaptainPick: true } pool)
        {
            pool.SetTier(PlayersGrid.SelectedItems.OfType<PoolPlayerRowViewModel>(), tier);
            e.Handled = true;
        }
        else if (e.Key == Key.Delete && !IsEditingCell() && ViewModel is { } viewModel)
        {
            var rows = PlayersGrid.SelectedItems.OfType<PoolPlayerRowViewModel>().ToList();
            if (!viewModel.ConfirmRemoval(rows))
            {
                e.Handled = true;
            }
        }
    }

    /// <summary>Keeps a tier key from also starting to edit the player's name (the grid edits a cell when you type).</summary>
    private void PlayersGrid_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (e.Text is ['1' or '2' or '3' or '4' or '5'] && !IsEditingCell() && ViewModel is { IsCaptainPick: true })
        {
            e.Handled = true;
        }
    }

    private static int? TierKey(Key key) => key switch
    {
        >= Key.D1 and <= Key.D5 => key - Key.D0,
        >= Key.NumPad1 and <= Key.NumPad5 => key - Key.NumPad0,
        _ => null,
    };

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
