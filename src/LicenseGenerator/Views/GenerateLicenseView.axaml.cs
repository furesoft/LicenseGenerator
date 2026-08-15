using Avalonia.Controls;
using Avalonia.Interactivity;
using LicenseGenerator.ViewModels;

namespace LicenseGenerator.Views;

public partial class GenerateLicenseView : UserControl
{
    private MainViewModel Vm => (MainViewModel)DataContext!;

    public GenerateLicenseView() => InitializeComponent();

    private void OnAddFeature(object? sender, RoutedEventArgs e) => Vm.AddFeature();
    private void OnRemoveFeature(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string feature })
            Vm.RemoveFeature(feature);
    }
    private void OnGenerateLicense(object? sender, RoutedEventArgs e) => Vm.GenerateLicense();
    private async void OnSaveLicense(object? sender, RoutedEventArgs e)
        => await Vm.SaveLicenseAsync(TopLevel.GetTopLevel(this)!);
}
