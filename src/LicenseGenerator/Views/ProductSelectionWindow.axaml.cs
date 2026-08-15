using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using LicenseGenerator.Models;
using LicenseGenerator.ViewModels;
using PleasantUI.Controls;

namespace LicenseGenerator.Views;

public partial class ProductSelectionWindow : PleasantWindow
{
    private ProductSelectionViewModel ViewModel => (ProductSelectionViewModel)DataContext!;
    public ProductSelectionResult? SelectionResult { get; private set; }

    public ProductSelectionWindow() : this([])
    {
    }

    public ProductSelectionWindow(IEnumerable<string> existingProducts)
    {
        InitializeComponent();
        DataContext = new ProductSelectionViewModel(existingProducts);
    }

    private void OnCancelClicked(object? sender, RoutedEventArgs e)
    {
        SelectionResult = null;
        Close();
    }

    private void OnOkClicked(object? sender, RoutedEventArgs e)
    {
        if (!ViewModel.TryBuildResult(out ProductSelectionResult? result))
            return;

        SelectionResult = result;
        Close();
    }
}
