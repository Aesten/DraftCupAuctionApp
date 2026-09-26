using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using AuctionApp.ViewModels;

namespace AuctionApp.Views;

public partial class AuctionView : UserControl
{
    public AuctionView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is AuctionViewModel oldModel)
        {
            oldModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        if (e.NewValue is AuctionViewModel newModel)
        {
            newModel.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private void SkippedButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is AuctionViewModel viewModel)
        {
            new SkippedDialog { DataContext = viewModel, Owner = Window.GetWindow(this) }.ShowDialog();
        }
    }

    // One pick board at a time, shared by every auction page (a page can be rebuilt while its board stays open).
    private static PickBoardWindow? _board;

    /// <summary>Captain Pick: opens the pick board in its own window (or brings it to the front if it's already open).</summary>
    private void BoardButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not AuctionViewModel viewModel)
        {
            return;
        }

        if (_board is { IsLoaded: true } && _board.DataContext == viewModel)
        {
            _board.WindowState = _board.WindowState == WindowState.Minimized ? WindowState.Normal : _board.WindowState;
            _board.Activate();
            return;
        }

        _board?.Close();
        _board = new PickBoardWindow(viewModel) { Owner = Window.GetWindow(this) };
        _board.Closed += (_, _) => _board = null;
        _board.Show();
    }

    // Wheel notches not turned into price steps yet (touchpads send small deltas).
    private int _wheelDelta;

    /// <summary>
    /// Scrolling over the price (the box or its −/+ buttons) changes it: 0.1 per notch, 1.0 with Ctrl held. Works on
    /// hover, without clicking the box first.
    /// </summary>
    private void PriceRow_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (DataContext is not AuctionViewModel { HasCurrentPlayer: true } viewModel)
        {
            return;
        }

        e.Handled = true;
        _wheelDelta += e.Delta;
        var notches = _wheelDelta / Mouse.MouseWheelDeltaForOneLine;
        if (notches == 0)
        {
            return;
        }

        _wheelDelta -= notches * Mouse.MouseWheelDeltaForOneLine;
        var step = (Keyboard.Modifiers & ModifierKeys.Control) != 0 ? 1.0m : 0.1m;
        viewModel.ChangePriceCommand.Execute((notches * step).ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    /// <summary>Leaving the price box (Enter, or a click elsewhere) shows the price as it will be used.</summary>
    private void PriceBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => (DataContext as AuctionViewModel)?.CommitPrice();

    private void RemainingButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is AuctionViewModel viewModel)
        {
            new PlayerListDialog(
                $"Remaining players ({viewModel.Remaining.Count})",
                "Everyone still in the queue, in alphabetical order so the auction order stays hidden. Skipped players aren't included. "
                + "Put one on the block to auction them now; the player on the block comes up right after.",
                viewModel.Remaining,
                viewModel.BringToBlock) { Owner = Window.GetWindow(this) }.ShowDialog();
        }
    }

    /// <summary>The More button opens its menu on a normal click.</summary>
    private void MoreButton_Click(object sender, RoutedEventArgs e) => OpenMenu(MoreButton);

    /// <summary>Clicking a bought player opens the menu to fix the sale (instead of selecting the team).</summary>
    private void PickRow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement row)
        {
            OpenMenu(row);
            e.Handled = true;
        }
    }

    private static void OpenMenu(FrameworkElement target)
    {
        if (target.ContextMenu is { } menu)
        {
            menu.PlacementTarget = target;
            menu.Placement = PlacementMode.Bottom;
            menu.IsOpen = true;
        }
    }

    /// <summary>
    /// After selecting the bidding team, the page takes the keyboard focus (unless the price is being typed), so
    /// Ctrl+Enter sells right away. The price box isn't selected: its highlighted text was distracting on stream.
    /// </summary>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AuctionViewModel.SelectedTeam) && sender is AuctionViewModel { SelectedTeam: not null })
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, () =>
            {
                if (!PriceBox.IsKeyboardFocusWithin && !IsKeyboardFocusWithin)
                {
                    Focus();
                }
            });
        }
    }
}
