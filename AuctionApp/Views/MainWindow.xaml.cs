using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using AuctionApp.ViewModels;

namespace AuctionApp.Views;

public partial class MainWindow : Window
{
    private WindowState _stateBeforeStreamMode = WindowState.Normal;

    public MainWindow()
    {
        InitializeComponent();
    }

    private MainViewModel? ViewModel => DataContext as MainViewModel;

    protected override void OnClosing(CancelEventArgs e)
    {
        ViewModel?.Shutdown();
        base.OnClosing(e);
    }

    /// <summary>F11 toggles stream mode (full screen, page content only); Escape leaves it.</summary>
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        if (ViewModel is not { } viewModel)
        {
            return;
        }

        if (e.Key == Key.F11 || (e.Key == Key.Escape && viewModel.IsStreamMode))
        {
            SetStreamMode(!viewModel.IsStreamMode && e.Key == Key.F11);
            e.Handled = true;
        }
    }

    private void SetStreamMode(bool enabled)
    {
        if (ViewModel is not { } viewModel || viewModel.IsStreamMode == enabled)
        {
            return;
        }

        viewModel.IsStreamMode = enabled;
        if (enabled)
        {
            _stateBeforeStreamMode = WindowState;
            WindowStyle = WindowStyle.None;
            // Going through Normal makes Windows recompute the maximized bounds without the title bar.
            WindowState = WindowState.Normal;
            WindowState = WindowState.Maximized;
        }
        else
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowState = _stateBeforeStreamMode;
        }
    }

    // Tournament files can be dropped anywhere on the window to import them.

    private static string[] DroppedFiles(DragEventArgs e) =>
        e.Data.GetDataPresent(DataFormats.FileDrop) && e.Data.GetData(DataFormats.FileDrop) is string[] files
            ? files.Where(file => file.EndsWith(".json", StringComparison.OrdinalIgnoreCase) && File.Exists(file)).ToArray()
            : [];

    private void Window_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DroppedFiles(e).Length > 0 ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        var files = DroppedFiles(e);
        if (files.Length == 0 || ViewModel is not { } viewModel)
        {
            return;
        }

        e.Handled = true;
        // Let the drop finish before showing dialogs.
        Dispatcher.BeginInvoke(() =>
        {
            foreach (var file in files)
            {
                viewModel.ImportFile(file);
            }
        });
    }
}
