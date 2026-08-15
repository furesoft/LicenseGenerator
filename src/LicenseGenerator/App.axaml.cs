using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;
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
            desktop.MainWindow = new MainWindow();
            desktop.MainWindow.Opened += async (_, _) => await CheckForUpdatesAsync();
        }

        base.OnFrameworkInitializationCompleted();
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