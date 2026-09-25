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

    /// <summary>Leaving the price box (Enter, or a click elsewhere) shows the price as it will be used.</summary>
    private void PriceBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => (DataContext as AuctionViewModel)?.CommitPrice();

    private void RemainingButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is AuctionViewModel viewModel)
        {
            new PlayerListDialog(
                $"Remaining players ({viewModel.Remaining.Count})",
                "Everyone still in the queue, in alphabetical order so the auction order stays hidden. Skipped players aren't included.",
                viewModel.Remaining) { Owner = Window.GetWindow(this) }.ShowDialog();
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

    /// <summary>After picking the winning team, the price box gets the focus so the price can be typed straight away.</summary>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AuctionViewModel.SelectedTeam) && sender is AuctionViewModel { SelectedTeam: not null })
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, () =>
            {
                PriceBox.Focus();
                PriceBox.SelectAll();
            });
        }
    }
}
