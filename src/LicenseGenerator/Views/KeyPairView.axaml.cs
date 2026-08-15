using Avalonia.Controls;
using Avalonia.Interactivity;
using LicenseGenerator.ViewModels;

namespace LicenseGenerator.Views;

public partial class KeyPairView : UserControl
{
    private MainViewModel Vm => (MainViewModel)DataContext!;

    public KeyPairView() => InitializeComponent();

    private void OnGenerateKeyPair(object? sender, RoutedEventArgs e) => Vm.GenerateKeyPair();
    private async void OnSavePublicKey(object? sender, RoutedEventArgs e)  => await Vm.SavePublicKeyAsync(TopLevel.GetTopLevel(this)!);
    private async void OnSavePrivateKey(object? sender, RoutedEventArgs e) => await Vm.SavePrivateKeyAsync(TopLevel.GetTopLevel(this)!);
    private async void OnLoadPublicKey(object? sender, RoutedEventArgs e)  => await Vm.LoadPublicKeyAsync(TopLevel.GetTopLevel(this)!);
    private async void OnLoadPrivateKey(object? sender, RoutedEventArgs e) => await Vm.LoadPrivateKeyAsync(TopLevel.GetTopLevel(this)!);
}
