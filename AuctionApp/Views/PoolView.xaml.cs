using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using AuctionApp.Controls;
using AuctionApp.ViewModels;

namespace AuctionApp.Views;

/// <summary>The player pool grid. Code-behind handles what XAML can't: drag and drop reordering and edit commits.</summary>
public partial class PoolView : UserControl
{
    private const double AutoScrollMargin = 32;

    private PoolPlayerRowViewModel? _dragCandidate;
    private Point _dragStart;
    private InsertionAdorner? _insertion;

    public PoolView()
    {
        InitializeComponent();
    }

    private PoolViewModel? ViewModel => DataContext as PoolViewModel;

    // Drag and drop reordering, started from the handle column.

    private void Handle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: PoolPlayerRowViewModel row })
        {
            _dragCandidate = row;
            _dragStart = e.GetPosition(PlayersGrid);
        }
    }

    private void PlayersGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => _dragCandidate = null;

    private void PlayersGrid_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (_dragCandidate is not { } row || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var offset = e.GetPosition(PlayersGrid) - _dragStart;
        if (Math.Abs(offset.Y) < SystemParameters.MinimumVerticalDragDistance && Math.Abs(offset.X) < SystemParameters.MinimumHorizontalDragDistance)
        {
            return;
        }

        _dragCandidate = null;
        PlayersGrid.CommitEdit(DataGridEditingUnit.Row, exitEditingMode: true);
        PlayersGrid.SelectedItem = row;
        try
        {
            DragDrop.DoDragDrop(PlayersGrid, new DataObject(typeof(PoolPlayerRowViewModel), row), DragDropEffects.Move);
        }
        finally
        {
            RemoveInsertionLine();
        }
    }

    private void PlayersGrid_DragOver(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(typeof(PoolPlayerRowViewModel)))
        {
            return;
        }

        e.Effects = DragDropEffects.Move;
        e.Handled = true;
        AutoScroll(e.GetPosition(PlayersGrid).Y);

        var (row, after) = DropTarget(e);
        if (row == null)
        {
            RemoveInsertionLine();
            return;
        }

        var y = row.TranslatePoint(new Point(0, after ? row.ActualHeight : 0), PlayersGrid).Y;
        if (_insertion == null)
        {
            _insertion = new InsertionAdorner(PlayersGrid, (Brush)FindResource("AccentFillColorDefaultBrush"));
            AdornerLayer.GetAdornerLayer(PlayersGrid)?.Add(_insertion);
        }

        _insertion.MoveTo(y);
    }

    private void PlayersGrid_DragLeave(object sender, DragEventArgs e)
    {
        // DragLeave also fires when moving between the grid's own children; only react when really leaving it.
        var position = e.GetPosition(PlayersGrid);
        if (position.X < 0 || position.Y < 0 || position.X > PlayersGrid.ActualWidth || position.Y > PlayersGrid.ActualHeight)
        {
            RemoveInsertionLine();
        }
    }

    private void PlayersGrid_Drop(object sender, DragEventArgs e)
    {
        RemoveInsertionLine();
        if (e.Data.GetData(typeof(PoolPlayerRowViewModel)) is not PoolPlayerRowViewModel dragged || ViewModel is not { } viewModel)
        {
            return;
        }

        e.Handled = true;
        var (row, after) = DropTarget(e);
        // Dropping on the empty "new player" row, or below the rows, moves the player to the end.
        viewModel.Move(dragged, row?.Item as PoolPlayerRowViewModel, after);
        PlayersGrid.SelectedItem = dragged;
        PlayersGrid.ScrollIntoView(dragged);
    }

    /// <summary>The row under the mouse, and whether the drop goes after it (lower half) or before it.</summary>
    private (DataGridRow? Row, bool After) DropTarget(DragEventArgs e)
    {
        var row = FindAncestor<DataGridRow>(e.OriginalSource as DependencyObject);
        if (row == null)
        {
            return (null, true);
        }

        return (row, e.GetPosition(row).Y > row.ActualHeight / 2);
    }

    private void AutoScroll(double y)
    {
        if (FindDescendant<ScrollViewer>(PlayersGrid) is not { } scroll)
        {
            return;
        }

        if (y < AutoScrollMargin + 32)
        {
            scroll.LineUp();
        }
        else if (y > PlayersGrid.ActualHeight - AutoScrollMargin)
        {
            scroll.LineDown();
        }
    }

    private void RemoveInsertionLine()
    {
        if (_insertion != null)
        {
            AdornerLayer.GetAdornerLayer(PlayersGrid)?.Remove(_insertion);
            _insertion = null;
        }
    }

    // Keyboard: Alt+Up/Down moves the selected player, Delete asks before removing bought players.

    private void PlayersGrid_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (ViewModel is not { } viewModel)
        {
            return;
        }

        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (Keyboard.Modifiers == ModifierKeys.Alt && key is Key.Up or Key.Down && PlayersGrid.SelectedItem is PoolPlayerRowViewModel row)
        {
            PlayersGrid.CommitEdit(DataGridEditingUnit.Row, exitEditingMode: true);
            viewModel.MoveBy(row, key == Key.Up ? -1 : 1);
            PlayersGrid.SelectedItem = row;
            PlayersGrid.ScrollIntoView(row);
            e.Handled = true;
        }
        else if (e.Key == Key.Delete && !IsEditingCell())
        {
            var rows = PlayersGrid.SelectedItems.OfType<PoolPlayerRowViewModel>().ToList();
            if (!viewModel.ConfirmRemoval(rows))
            {
                e.Handled = true;
            }
        }
    }

    private bool IsEditingCell() => Keyboard.FocusedElement is TextBox box && FindAncestor<DataGridCell>(box) != null;

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

    private static T? FindDescendant<T>(DependencyObject element)
        where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
        {
            var child = VisualTreeHelper.GetChild(element, i);
            if (child is T match)
            {
                return match;
            }

            if (FindDescendant<T>(child) is { } found)
            {
                return found;
            }
        }

        return null;
    }
}
