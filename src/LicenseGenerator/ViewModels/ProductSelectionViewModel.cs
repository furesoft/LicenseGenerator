using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LicenseGenerator.Models;

namespace LicenseGenerator.ViewModels;

public partial class ProductSelectionViewModel : ObservableObject
{
    public ObservableCollection<string> Products { get; } = new();

    [ObservableProperty] private string _selectedProductName = string.Empty;
    [ObservableProperty] private string _newProductName = string.Empty;
    [ObservableProperty] private string _newProductPassphrase = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;
    private readonly Dictionary<string, string> _newProductPassphrases = new(StringComparer.OrdinalIgnoreCase);

    public ProductSelectionViewModel(System.Collections.Generic.IEnumerable<string> existingProducts)
    {
        foreach (var product in existingProducts.OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
            Products.Add(product);

        if (Products.Count > 0)
            SelectedProductName = Products[0];
    }

    [RelayCommand]
    private void AddProduct()
    {
        StatusMessage = string.Empty;
        var name = NewProductName.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            StatusMessage = "⚠ Bitte Produktname eingeben.";
            return;
        }

        if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            StatusMessage = "⚠ Produktname enthält ungültige Zeichen.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewProductPassphrase))
        {
            StatusMessage = "⚠ Bitte Passphrase für neues Produkt eingeben.";
            return;
        }

        var existing = Products.FirstOrDefault(p => string.Equals(p, name, StringComparison.OrdinalIgnoreCase));
        if (existing is null)
        {
            Products.Add(name);
            SelectedProductName = name;
            _newProductPassphrases[name] = NewProductPassphrase;
            NewProductName = string.Empty;
            NewProductPassphrase = string.Empty;
            return;
        }

        SelectedProductName = existing;
        NewProductName = string.Empty;
        NewProductPassphrase = string.Empty;
        StatusMessage = "ℹ Produkt existiert bereits und wurde ausgewählt.";
    }

    public bool TryBuildResult(out ProductSelectionResult? result)
    {
        var selected = SelectedProductName.Trim();
        if (string.IsNullOrWhiteSpace(selected))
        {
            StatusMessage = "⚠ Bitte ein Produkt auswählen oder mit + anlegen.";
            result = null;
            return false;
        }

        if (_newProductPassphrases.TryGetValue(selected, out var passphrase))
        {
            result = new ProductSelectionResult
            {
                ProductName = selected,
                IsNewProduct = true,
                NewProductPassphrase = passphrase
            };
            return true;
        }

        result = new ProductSelectionResult
        {
            ProductName = selected,
            IsNewProduct = false,
            NewProductPassphrase = string.Empty
        };
        return true;
    }
}
