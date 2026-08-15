using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LicenseGenerator.Models;
using LicenseGenerator.Services;
using Standard.Licensing;

namespace LicenseGenerator.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly LicensingService _service = new();
    private readonly IStorageService _storage;
    private readonly IProductService _productService;

    public MainViewModel(IStorageService storage, IProductService productService)
    {
        _storage = storage;
        _productService = productService;
    }

    // ── Product Context ───────────────────────────────────────
    [ObservableProperty] private string _selectedProductName = string.Empty;
    [ObservableProperty] private string _productStatus = string.Empty;

    // ── Key Tab ──────────────────────────────────────────────
    [ObservableProperty] private string _passphrase = string.Empty;
    [ObservableProperty] private string _publicKey = string.Empty;
    [ObservableProperty] private string _privateKey = string.Empty;

    // ── Generate Tab ─────────────────────────────────────────
    [ObservableProperty] private string _customerName = string.Empty;
    [ObservableProperty] private string _customerEmail = string.Empty;
    [ObservableProperty] private string _customerCompany = string.Empty;
    [ObservableProperty] private DateTimeOffset? _expirationDate = DateTimeOffset.Now.AddYears(1);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ExpirationEnabled))]
    private bool _neverExpires;

    public bool ExpirationEnabled => !NeverExpires;

    [ObservableProperty] private int _maxUsages = 1;
    [ObservableProperty] private string _newFeature = string.Empty;
    [ObservableProperty] private string _generatedLicenseXml = string.Empty;

    public ObservableCollection<string> Features { get; } = [];

    // ── Validate Tab ─────────────────────────────────────────
    [ObservableProperty] private string _licenseToValidate = string.Empty;
    [ObservableProperty] private string _validationPublicKey = string.Empty;
    [ObservableProperty] private string _validationResult = string.Empty;
    [ObservableProperty] private string _licenseDetails = string.Empty;

    // ── Commands ─────────────────────────────────────────────

    public async Task SelectProductAsync(string productName, string passphrase)
    {
        ProductStatus = string.Empty;
        var (publicKey, privateKey) = await _productService.LoadProductKeysAsync(productName);

        SelectedProductName = productName.Trim();
        Passphrase = passphrase;
        PublicKey = publicKey;
        PrivateKey = privateKey;
        ValidationPublicKey = publicKey;
        ProductStatus = $"✔ Produkt geladen: {SelectedProductName}";
    }

    [RelayCommand]
    private async Task ExportKeysAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedProductName))
        {
            ProductStatus = "⚠ Bitte zuerst ein Produkt auswählen.";
            return;
        }

        var folder = await _storage.PickFolderPathAsync();
        if (string.IsNullOrWhiteSpace(folder))
            return;

        try
        {
            await _productService.ExportProductKeysAsync(SelectedProductName, folder);
            ProductStatus = $"✔ Schlüssel erfolgreich exportiert nach: {folder}";
        }
        catch (Exception ex)
        {
            ProductStatus = $"⚠ Fehler beim Exportieren der Schlüssel: {ex.Message}";
        }
    }

    [RelayCommand]
    private void GenerateKeyPair()
    {
        if (string.IsNullOrWhiteSpace(Passphrase))
        {
            PublicKey = "⚠ Bitte zuerst eine Passphrase eingeben.";
            return;
        }
        var keys = _service.GenerateKeyPair(Passphrase);
        PublicKey = keys.PublicKey;
        PrivateKey = keys.PrivateKey;
    }

    [RelayCommand]
    private Task SavePublicKeyAsync() => _storage.SaveTextFileAsync("public.key", PublicKey);

    [RelayCommand]
    private Task SavePrivateKeyAsync() => _storage.SaveTextFileAsync("private.key", PrivateKey);

    [RelayCommand]
    private async Task LoadPublicKeyAsync()
    {
        var text = await _storage.OpenTextFileAsync();
        if (text is not null) PublicKey = text;
    }

    [RelayCommand]
    private async Task LoadPrivateKeyAsync()
    {
        var text = await _storage.OpenTextFileAsync();
        if (text is not null) PrivateKey = text;
    }

    [RelayCommand]
    private void AddFeature()
    {
        var f = NewFeature.Trim();
        if (!string.IsNullOrEmpty(f) && !Features.Contains(f))
            Features.Add(f);
        NewFeature = string.Empty;
    }

    [RelayCommand]
    private void RemoveFeature(string feature) => Features.Remove(feature);

    [RelayCommand]
    private void GenerateLicense()
    {
        if (string.IsNullOrWhiteSpace(SelectedProductName))
        {
            GeneratedLicenseXml = "⚠ Bitte zuerst ein Produkt auswählen.";
            return;
        }

        if (string.IsNullOrWhiteSpace(PrivateKey) || string.IsNullOrWhiteSpace(Passphrase))
        {
            GeneratedLicenseXml = "⚠ Private Key und Passphrase werden benötigt.";
            return;
        }

        try
        {
            var model = new LicenseModel
            {
                CustomerName = CustomerName,
                CustomerEmail = CustomerEmail,
                CustomerCompany = CustomerCompany,
                ExpirationDate = NeverExpires ? null : ExpirationDate?.DateTime,
                MaxUsages = MaxUsages,
                ProductFeatures = [.. Features],
                AdditionalAttributes = new Dictionary<string, string>
                {
                    ["ProductName"] = SelectedProductName
                }
            };

            var keys = new KeyPairModel
            {
                PublicKey = PublicKey,
                PrivateKey = PrivateKey,
                Passphrase = Passphrase
            };

            GeneratedLicenseXml = _service.CreateLicense(model, keys);
        }
        catch (Exception ex)
        {
            GeneratedLicenseXml = $"⚠ Fehler: {ex.Message}";
        }
    }

    [RelayCommand]
    private Task SaveLicenseAsync() => _storage.SaveTextFileAsync("license.lic", GeneratedLicenseXml);

    [RelayCommand]
    private async Task LoadLicenseForValidationAsync()
    {
        var text = await _storage.OpenTextFileAsync();
        if (text is not null) LicenseToValidate = text;
    }

    [RelayCommand]
    private void UseGeneratedPublicKeyForValidation() => ValidationPublicKey = PublicKey;

    [RelayCommand]
    private void ValidateLicense()
    {
        if (string.IsNullOrWhiteSpace(LicenseToValidate) || string.IsNullOrWhiteSpace(ValidationPublicKey))
        {
            ValidationResult = "⚠ Lizenz-XML und Public Key werden benötigt.";
            LicenseDetails = string.Empty;
            return;
        }

        var (isValid, failures) = _service.ValidateLicense(LicenseToValidate, ValidationPublicKey);

        if (isValid)
        {
            ValidationResult = "✔ Lizenz ist gültig.";
            var license = _service.LoadLicense(LicenseToValidate);
            LicenseDetails = license is not null ? FormatLicenseDetails(license) : string.Empty;
        }
        else
        {
            ValidationResult = "✘ Lizenz ist ungültig:\n" +
                               string.Join("\n", failures.Select(f => $"  • {f.Message}: {f.HowToResolve}"));
            LicenseDetails = string.Empty;
        }
    }

    [RelayCommand]
    private async Task LoadLicenseAndDisplayAsync()
    {
        var text = await _storage.OpenTextFileAsync();
        if (text is null) return;

        LicenseToValidate = text;
        var license = _service.LoadLicense(text);
        LicenseDetails = license is not null
            ? FormatLicenseDetails(license)
            : "⚠ Lizenz konnte nicht geladen werden.";
    }

    private static string FormatLicenseDetails(License license)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"ID:           {license.Id}");
        sb.AppendLine($"Typ:          {license.Type}");
        sb.AppendLine($"Max. Nutzung: {license.Quantity}");
        sb.AppendLine($"Ablauf:       {(license.Expiration == DateTime.MaxValue ? "Nie" : license.Expiration.ToString("d"))}");
        sb.AppendLine();
        sb.AppendLine("── Kunde ──");
        sb.AppendLine($"Name:         {license.Customer?.Name}");
        sb.AppendLine($"E-Mail:       {license.Customer?.Email}");
        sb.AppendLine($"Firma:        {license.Customer?.Company}");

        var features = license.ProductFeatures?.GetAll();
        if (features?.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("── Features ──");
            foreach (var kv in features)
                sb.AppendLine($"  {kv.Key}: {kv.Value}");
        }

        var attrs = license.AdditionalAttributes?.GetAll();
        if (attrs?.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("── Zusätzliche Attribute ──");
            foreach (var kv in attrs)
                sb.AppendLine($"  {kv.Key}: {kv.Value}");
        }

        return sb.ToString();
    }
}
