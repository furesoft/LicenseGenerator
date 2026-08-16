using Avalonia.Interactivity;
using LicenseGenerator.ViewModels;
using PleasantUI.Controls;

namespace LicenseGenerator.Views;

public partial class AddProductWindow : PleasantWindow
{
    private AddProductViewModel ViewModel => (AddProductViewModel)DataContext!;

    public string? NewProductName { get; private set; }
    public string? NewProductPassphrase { get; private set; }

    public AddProductWindow()
    {
        InitializeComponent();
        DataContext = new AddProductViewModel();
    }

    private void OnCancelClicked(object? sender, RoutedEventArgs e) => Close();

    private void OnCreateClicked(object? sender, RoutedEventArgs e)
    {
        if (!ViewModel.Validate())
            return;

        NewProductName = ViewModel.ProductName.Trim();
        NewProductPassphrase = ViewModel.Passphrase;
        Close();
    }
}
