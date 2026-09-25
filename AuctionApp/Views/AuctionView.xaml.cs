using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
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
