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
            StatusMessage = "⚠ Please enter a product name.";
            return false;
        }

        if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            StatusMessage = "⚠ Product name contains invalid characters.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Passphrase))
        {
            StatusMessage = "⚠ Please enter a passphrase for the new product.";
            return false;
        }

        return true;
    }
}
