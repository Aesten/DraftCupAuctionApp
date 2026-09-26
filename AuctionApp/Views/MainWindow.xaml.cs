using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using AuctionApp.Services;
using AuctionApp.ViewModels;

namespace AuctionApp.Views;

public partial class MainWindow : Window
{
    private WindowState _stateBeforeFullScreen = WindowState.Normal;

    public MainWindow()
    {
        InitializeComponent();

        // Opens where it was last time (maximized again if it was; full screen stays an F11 away).
        if (AppSettings.Current?.MainWindow is { } placement)
        {
            placement.ApplyTo(this);
            if (placement.Maximized || placement.FullScreen)
            {
                SourceInitialized += (_, _) => WindowState = WindowState.Maximized;
            }
        }

        DataContextChanged += (_, e) =>
        {
            if (e.OldValue is MainViewModel oldModel)
            {
                oldModel.PropertyChanged -= OnViewModelPropertyChanged;
            }

            if (e.NewValue is MainViewModel newModel)
            {
                newModel.PropertyChanged += OnViewModelPropertyChanged;
            }
        };
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.IsMenuOpen) && ViewModel is { IsMenuOpen: true })
        {
            Dispatcher.BeginInvoke(PlaceMenuButton, System.Windows.Threading.DispatcherPriority.Loaded);
        }
    }

    /// <summary>
    /// Puts the menu's ☰ button exactly over the top bar's one (same place, same size), so the menu closes by clicking
    /// the same spot that opened it. The menu's title lines up beside it.
    /// </summary>
    private void PlaceMenuButton()
    {
        if (FindByAutomationId(this, "MenuToggle") is not FrameworkElement toggle || MenuCloseButton.Parent is not UIElement panel)
        {
            return;
        }

        var position = toggle.TranslatePoint(new Point(0, 0), panel);
        MenuCloseButton.Margin = new Thickness(position.X, position.Y, 0, 0);
        MenuCloseButton.Width = toggle.ActualWidth;
        MenuCloseButton.Height = toggle.ActualHeight;
        MenuHeader.Margin = new Thickness(position.X + toggle.ActualWidth - 4, position.Y, 0, 8);
        MenuHeader.Height = toggle.ActualHeight;
    }

    private static DependencyObject? FindByAutomationId(DependencyObject element, string id)
    {
        if (System.Windows.Automation.AutomationProperties.GetAutomationId(element) == id)
        {
            return element;
        }

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
        {
            if (FindByAutomationId(VisualTreeHelper.GetChild(element, i), id) is { } found)
            {
                return found;
            }
        }

        return null;
    }

    private MainViewModel? ViewModel => DataContext as MainViewModel;

    protected override void OnClosing(CancelEventArgs e)
    {
        ViewModel?.Shutdown();
        if (AppSettings.Current is { } settings)
        {
            var placement = WindowPlacement.Capture(this);
            if (placement.FullScreen)
            {
                // Full screen goes back to how it was before (maximized or not).
                placement.Maximized = _stateBeforeFullScreen == WindowState.Maximized;
                placement.FullScreen = false;
            }

            settings.MainWindow = placement;
            settings.Save();
        }

        base.OnClosing(e);
    }

    /// <summary>F11 toggles full screen (for screen sharing); Escape closes the menu, then leaves full screen.</summary>
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        if (e.Key == Key.F11)
        {
            SetFullScreen(WindowStyle != WindowStyle.None);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && ViewModel is { IsMenuOpen: true } viewModel)
        {
            viewModel.IsMenuOpen = false;
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && WindowStyle == WindowStyle.None)
        {
            SetFullScreen(false);
            e.Handled = true;
        }
    }

    private void SetFullScreen(bool enabled)
    {
        if (enabled)
        {
            _stateBeforeFullScreen = WindowState;
            WindowStyle = WindowStyle.None;
            // Going through Normal makes Windows recompute the maximized bounds without the title bar.
            WindowState = WindowState.Normal;
            WindowState = WindowState.Maximized;
        }
        else
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowState = _stateBeforeFullScreen;
        }
    }

    // Text boxes: Enter confirms and leaves the box, and clicking the background leaves it too, as people expect.

    /// <summary>Enter in a one-line text box that doesn't use Enter itself (add a player, grid cells...) leaves the box.</summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!e.Handled && e.Key == Key.Enter && Keyboard.FocusedElement is TextBox { AcceptsReturn: false } box)
        {
            LeaveTextBox(box);
            e.Handled = true;
        }
    }

    /// <summary>
    /// A click on something that doesn't take the focus itself (the page background, a team card...) moves the focus
    /// out of the text box being edited, so the edit is committed and the caret goes away.
    /// </summary>
    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseDown(e);
        if (Keyboard.FocusedElement is not TextBox box || e.OriginalSource is not DependencyObject source || IsWithin(source, box))
        {
            return;
        }

        // Controls that take the focus when clicked (buttons, lists, other boxes...) are left to do so.
        var target = FocusableAncestor(source);
        if (target is not (ButtonBase or TextBoxBase or Selector or ListBoxItem or ComboBoxItem or DataGridCell or ScrollViewer or MenuItem or Thumb))
        {
            LeaveTextBox(box);
        }
    }

    /// <summary>
    /// Gives the focus to the page around the text box: pages are focusable without a focus rectangle, and their
    /// shortcuts (Ctrl+Z in the auction) keep working.
    /// </summary>
    private static void LeaveTextBox(TextBox box)
    {
        for (DependencyObject? element = VisualTreeHelper.GetParent(box); element != null; element = VisualTreeHelper.GetParent(element))
        {
            if (element is UserControl { Focusable: true } page)
            {
                page.Focus();
                return;
            }
        }

        Keyboard.ClearFocus();
    }

    private static UIElement? FocusableAncestor(DependencyObject? element)
    {
        for (; element != null; element = element is Visual or System.Windows.Media.Media3D.Visual3D ? VisualTreeHelper.GetParent(element) : LogicalTreeHelper.GetParent(element))
        {
            if (element is UIElement { Focusable: true, IsEnabled: true, IsVisible: true } focusable)
            {
                return focusable;
            }
        }

        return null;
    }

    private static bool IsWithin(DependencyObject element, DependencyObject ancestor)
    {
        for (var current = element; current != null; current = current is Visual or System.Windows.Media.Media3D.Visual3D ? VisualTreeHelper.GetParent(current) : LogicalTreeHelper.GetParent(current))
        {
            if (current == ancestor)
            {
                return true;
            }
        }

        return false;
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
