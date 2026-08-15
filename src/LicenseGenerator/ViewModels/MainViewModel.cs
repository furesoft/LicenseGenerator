using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using LicenseGenerator.Models;
using LicenseGenerator.Services;
using Standard.Licensing;

namespace LicenseGenerator.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly LicensingService _service = new();

    // ── Key Tab ──────────────────────────────────────────────
    private string _passphrase = string.Empty;
    public string Passphrase { get => _passphrase; set => Set(ref _passphrase, value); }

    private string _publicKey = string.Empty;
    public string PublicKey { get => _publicKey; set => Set(ref _publicKey, value); }

    private string _privateKey = string.Empty;
    public string PrivateKey { get => _privateKey; set => Set(ref _privateKey, value); }

    // ── Generate Tab ─────────────────────────────────────────
    private string _customerName = string.Empty;
    public string CustomerName { get => _customerName; set => Set(ref _customerName, value); }

    private string _customerEmail = string.Empty;
    public string CustomerEmail { get => _customerEmail; set => Set(ref _customerEmail, value); }

    private string _customerCompany = string.Empty;
    public string CustomerCompany { get => _customerCompany; set => Set(ref _customerCompany, value); }

    private DateTimeOffset? _expirationDate = DateTimeOffset.Now.AddYears(1);
    public DateTimeOffset? ExpirationDate { get => _expirationDate; set => Set(ref _expirationDate, value); }

    private bool _neverExpires;
    public bool NeverExpires
    {
        get => _neverExpires;
        set { Set(ref _neverExpires, value); OnPropertyChanged(nameof(ExpirationEnabled)); }
    }
    public bool ExpirationEnabled => !_neverExpires;

    private int _maxUsages = 1;
    public int MaxUsages { get => _maxUsages; set => Set(ref _maxUsages, value); }

    private string _newFeature = string.Empty;
    public string NewFeature { get => _newFeature; set => Set(ref _newFeature, value); }

    public ObservableCollection<string> Features { get; } = new();

    private string _generatedLicenseXml = string.Empty;
    public string GeneratedLicenseXml { get => _generatedLicenseXml; set => Set(ref _generatedLicenseXml, value); }

    // ── Validate Tab ─────────────────────────────────────────
    private string _licenseToValidate = string.Empty;
    public string LicenseToValidate { get => _licenseToValidate; set => Set(ref _licenseToValidate, value); }

    private string _validationPublicKey = string.Empty;
    public string ValidationPublicKey { get => _validationPublicKey; set => Set(ref _validationPublicKey, value); }

    private string _validationResult = string.Empty;
    public string ValidationResult { get => _validationResult; set => Set(ref _validationResult, value); }

    private string _licenseDetails = string.Empty;
    public string LicenseDetails { get => _licenseDetails; set => Set(ref _licenseDetails, value); }

    // ── Commands ─────────────────────────────────────────────

    public void GenerateKeyPair()
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

    public async Task SavePublicKeyAsync(TopLevel owner)
    {
        await SaveTextFileAsync(owner, "public.key", PublicKey);
    }

    public async Task SavePrivateKeyAsync(TopLevel owner)
    {
        await SaveTextFileAsync(owner, "private.key", PrivateKey);
    }

    public async Task LoadPublicKeyAsync(TopLevel owner)
    {
        var text = await LoadTextFileAsync(owner);
        if (text is not null) PublicKey = text;
    }

    public async Task LoadPrivateKeyAsync(TopLevel owner)
    {
        var text = await LoadTextFileAsync(owner);
        if (text is not null) PrivateKey = text;
    }

    public void AddFeature()
    {
        var f = NewFeature.Trim();
        if (!string.IsNullOrEmpty(f) && !Features.Contains(f))
            Features.Add(f);
        NewFeature = string.Empty;
    }

    public void RemoveFeature(string feature) => Features.Remove(feature);

    public void GenerateLicense()
    {
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
                ProductFeatures = Features.ToList()
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

    public async Task SaveLicenseAsync(TopLevel owner)
    {
        await SaveTextFileAsync(owner, "license.lic", GeneratedLicenseXml);
    }

    public async Task LoadLicenseForValidationAsync(TopLevel owner)
    {
        var text = await LoadTextFileAsync(owner);
        if (text is not null) LicenseToValidate = text;
    }

    public void UseGeneratedPublicKeyForValidation()
    {
        ValidationPublicKey = PublicKey;
    }

    public void ValidateLicense()
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

    public async Task LoadLicenseAndDisplay(TopLevel owner)
    {
        var text = await LoadTextFileAsync(owner);
        if (text is null) return;

        LicenseToValidate = text;
        var license = _service.LoadLicense(text);
        LicenseDetails = license is not null
            ? FormatLicenseDetails(license)
            : "⚠ Lizenz konnte nicht geladen werden.";
    }

    // ── Helpers ──────────────────────────────────────────────

    private static string FormatLicenseDetails(Standard.Licensing.License license)
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

    private static async Task SaveTextFileAsync(TopLevel owner, string suggestedName, string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return;

        var file = await owner.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            SuggestedFileName = suggestedName
        });

        if (file is null) return;
        await using var stream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(stream, Encoding.UTF8);
        await writer.WriteAsync(content);
    }

    private static async Task<string?> LoadTextFileAsync(TopLevel owner)
    {
        var files = await owner.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false
        });

        if (files.Count == 0) return null;
        await using var stream = await files[0].OpenReadAsync();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    // ── INotifyPropertyChanged ────────────────────────────────
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }
}
