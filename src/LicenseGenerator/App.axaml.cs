using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;
using LicenseGenerator.Models;
using LicenseGenerator.Services;
using LicenseGenerator.Views;
using Velopack;
using Velopack.Sources;

namespace LicenseGenerator;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _ = RunStartupFlowAsync(desktop);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static async Task RunStartupFlowAsync(IClassicDesktopStyleApplicationLifetime desktop)
    {
        desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var productService = new ProductService(new LicensingService());
        var existingProducts = await productService.GetProductsAsync();

        var selectionWindow = new ProductSelectionWindow(existingProducts);
        desktop.MainWindow = selectionWindow;
        selectionWindow.Show();
        await WaitUntilClosedAsync(selectionWindow);

        ProductSelectionResult? selection = selectionWindow.SelectionResult;
        if (selection is null)
        {
            desktop.Shutdown();
            return;
        }

        var passphraseWindow = new PassphraseDialogWindow(selection.ProductName, selection.NewProductPassphrase);
        desktop.MainWindow = passphraseWindow;
        passphraseWindow.Show();
        await WaitUntilClosedAsync(passphraseWindow);

        var passphrase = passphraseWindow.EnteredPassphrase;
        if (string.IsNullOrWhiteSpace(passphrase))
        {
            desktop.Shutdown();
            return;
        }

        if (selection.IsNewProduct)
            await productService.CreateProductAsync(selection.ProductName, passphrase);

        var mainWindow = new MainWindow(productService);
        await mainWindow.InitializeProductAsync(selection.ProductName, passphrase);
        mainWindow.Opened += async (_, _) => await CheckForUpdatesAsync();

        desktop.MainWindow = mainWindow;
        desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
        mainWindow.Show();
    }

    private static Task WaitUntilClosedAsync(Window window)
    {
        var tcs = new TaskCompletionSource();
        window.Closed += OnClosed;
        return tcs.Task;

        void OnClosed(object? sender, System.EventArgs e)
        {
            window.Closed -= OnClosed;
            tcs.TrySetResult();
        }
    }

    private static async Task CheckForUpdatesAsync()
    {
        var source = new GithubSource("https://github.com/furesoft/LicenseGenerator", null, false);
        var mgr = new UpdateManager(source);

        if (!mgr.IsInstalled)
            return;

        var update = await mgr.CheckForUpdatesAsync();
        if (update is null)
            return;

        await mgr.DownloadUpdatesAsync(update);
        mgr.ApplyUpdatesAndRestart(update);
    }
}