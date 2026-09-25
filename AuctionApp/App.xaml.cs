using System.IO;
using System.Windows;
using System.Windows.Threading;
using AuctionApp.Core.Storage;
using AuctionApp.Services;
using AuctionApp.ViewModels;
using AuctionApp.Views;

namespace AuctionApp;

public partial class App : Application
{
    private Mutex? _singleInstance;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Two copies of the app editing the same tournament would overwrite each other's saves.
        _singleInstance = new Mutex(true, @"Local\DraftCupAuction.SingleInstance", out var isFirstInstance);
        if (!isFirstInstance)
        {
            MessageBox.Show(
                "Draft Cup Auction is already open. To import a file, drop it on its window.",
                "Draft Cup Auction",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            Shutdown();
            return;
        }

        TournamentStore store;
        try
        {
            store = new TournamentStore(TournamentStore.DefaultRootDirectory);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"The app can't create its data folder:\n{ex.Message}", "Draft Cup Auction", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
            return;
        }

        DispatcherUnhandledException += (_, args) =>
        {
            LogError(store, args.Exception);
            new DialogService().ShowError("Something went wrong", args.Exception.Message + "\n\nYour tournaments are saved after every change, so nothing should be lost.");
            args.Handled = true;
        };

        var settings = AppSettings.Load(store.RootDirectory);
        settings.ApplyTheme();

        var viewModel = new MainViewModel(store, new DialogService(), settings);
        var window = new MainWindow { DataContext = viewModel };
        MainWindow = window;
        window.Show();

        // A tournament file opened with the app ("Open with", or dropped on the exe).
        foreach (var file in e.Args.Where(File.Exists))
        {
            viewModel.ImportFile(file);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _singleInstance?.Dispose();
        base.OnExit(e);
    }

    private static void LogError(TournamentStore store, Exception exception)
    {
        try
        {
            File.AppendAllText(Path.Combine(store.RootDirectory, "errors.log"), $"[{DateTime.Now:u}] {exception}\n\n");
        }
        catch (IOException)
        {
            // Logging is best effort.
        }
    }
}
