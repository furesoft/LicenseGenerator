using Avalonia.Controls;
using Avalonia.Interactivity;
using LicenseGenerator.ViewModels;

namespace LicenseGenerator.Views;

public partial class ValidateView : UserControl
{
    private MainViewModel Vm => (MainViewModel)DataContext!;

    public ValidateView() => InitializeComponent();

    private async void OnLoadLicenseForValidation(object? sender, RoutedEventArgs e)
        => await Vm.LoadLicenseForValidationAsync(TopLevel.GetTopLevel(this)!);
    private void OnUseGeneratedPublicKey(object? sender, RoutedEventArgs e)
        => Vm.UseGeneratedPublicKeyForValidation();
    private void OnValidateLicense(object? sender, RoutedEventArgs e)
        => Vm.ValidateLicense();
    private async void OnLoadAndDisplayLicense(object? sender, RoutedEventArgs e)
        => await Vm.LoadLicenseAndDisplay(TopLevel.GetTopLevel(this)!);
}
