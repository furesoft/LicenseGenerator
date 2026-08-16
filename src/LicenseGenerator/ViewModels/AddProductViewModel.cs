using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LicenseGenerator.ViewModels;

public partial class AddProductViewModel : ObservableObject
{
    [ObservableProperty] private string _productName = string.Empty;
    [ObservableProperty] private string _passphrase = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public bool Validate()
    {
        StatusMessage = string.Empty;
        var name = ProductName.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            StatusMessage = "⚠ Bitte Produktname eingeben.";
            return false;
        }

        if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            StatusMessage = "⚠ Produktname enthält ungültige Zeichen.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Passphrase))
        {
            StatusMessage = "⚠ Bitte Passphrase für das neue Produkt eingeben.";
            return false;
        }

        return true;
    }
}
