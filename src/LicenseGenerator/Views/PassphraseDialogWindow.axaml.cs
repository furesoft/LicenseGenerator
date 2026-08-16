using Avalonia.Controls;
using Avalonia.Interactivity;
using LicenseGenerator.ViewModels;
using PleasantUI.Controls;

namespace LicenseGenerator.Views;

public partial class PassphraseDialogWindow : PleasantWindow
{
    private PassphraseDialogViewModel ViewModel => (PassphraseDialogViewModel)DataContext!;
    public string? EnteredPassphrase { get; private set; }

    public PassphraseDialogWindow() : this(string.Empty, string.Empty)
    {
    }

    public PassphraseDialogWindow(string productName, string initialPassphrase)
    {
        InitializeComponent();
        DataContext = new PassphraseDialogViewModel
        {
            ProductName = productName,
            Passphrase = initialPassphrase
        };
    }

    private void OnCancelClicked(object? sender, RoutedEventArgs e)
    {
        EnteredPassphrase = null;
        Close();
    }

    private void OnOkClicked(object? sender, RoutedEventArgs e)
    {
        var passphrase = ViewModel.Passphrase;
        if (string.IsNullOrWhiteSpace(passphrase))
        {
            ViewModel.StatusMessage = "⚠ Please enter a passphrase.";
            return;
        }

        EnteredPassphrase = passphrase;
        Close();
    }
}
