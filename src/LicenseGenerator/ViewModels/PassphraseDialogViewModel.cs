using CommunityToolkit.Mvvm.ComponentModel;

namespace LicenseGenerator.ViewModels;

public partial class PassphraseDialogViewModel : ObservableObject
{
    [ObservableProperty] private string _productName = string.Empty;
    [ObservableProperty] private string _passphrase = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;
}
