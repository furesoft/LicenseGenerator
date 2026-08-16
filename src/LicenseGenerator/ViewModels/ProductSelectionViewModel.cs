using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using LicenseGenerator.Models;

namespace LicenseGenerator.ViewModels;

public partial class ProductSelectionViewModel : ObservableObject
{
    public ObservableCollection<string> Products { get; } = [];

    [ObservableProperty] private string _selectedProductName = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;
    private readonly Dictionary<string, string> _newProductPassphrases = new(StringComparer.OrdinalIgnoreCase);

    public ProductSelectionViewModel(IEnumerable<string> existingProducts)
    {
        foreach (var product in existingProducts.OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
            Products.Add(product);

        if (Products.Count > 0)
            SelectedProductName = Products[0];
    }

    public void AddProduct(string name, string passphrase)
    {
        var existing = Products.FirstOrDefault(p => string.Equals(p, name, StringComparison.OrdinalIgnoreCase));
        if (existing is null)
        {
            Products.Add(name);
            _newProductPassphrases[name] = passphrase;
        }
        SelectedProductName = existing ?? name;
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
