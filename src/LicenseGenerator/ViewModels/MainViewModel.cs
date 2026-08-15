using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
    private readonly ISettingsService _settings;

    public MainViewModel(IStorageService storage, ISettingsService settings)
    {
        _storage = storage;
        _settings = settings;
    }

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

    public ObservableCollection<string> Features { get; } = new();

    // ── Validate Tab ─────────────────────────────────────────
    [ObservableProperty] private string _licenseToValidate = string.Empty;
    [ObservableProperty] private string _validationPublicKey = string.Empty;
    [ObservableProperty] private string _validationResult = string.Empty;
    [ObservableProperty] private string _licenseDetails = string.Empty;

    // ── Settings Tab ──────────────────────────────────────────
    [ObservableProperty] private string _settingsPublicKeyPath = string.Empty;
    [ObservableProperty] private string _settingsPrivateKeyPath = string.Empty;
    [ObservableProperty] private string _settingsStatus = string.Empty;

    // ── Commands ─────────────────────────────────────────────

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
    private async Task BrowseSettingsPublicKeyPathAsync()
    {
        var path = await _storage.PickOpenFilePathAsync();
        if (!string.IsNullOrWhiteSpace(path))
            SettingsPublicKeyPath = path;
    }

    [RelayCommand]
    private async Task BrowseSettingsPrivateKeyPathAsync()
    {
        var path = await _storage.PickOpenFilePathAsync();
        if (!string.IsNullOrWhiteSpace(path))
            SettingsPrivateKeyPath = path;
    }

    [RelayCommand]
    private async Task SaveKeyPathSettingsAsync()
    {
        await _settings.SaveAsync(new AppSettings
        {
            PublicKeyPath = SettingsPublicKeyPath,
            PrivateKeyPath = SettingsPrivateKeyPath
        });
        SettingsStatus = "✔ Einstellungen gespeichert.";
    }

    [RelayCommand]
    private Task LoadKeysFromSettingsAsync() => LoadConfiguredKeysAsync(isStartup: false);

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

    // ── Helpers ──────────────────────────────────────────────

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

    public async Task InitializeAsync()
    {
        var appSettings = await _settings.LoadAsync();
        SettingsPublicKeyPath = appSettings.PublicKeyPath;
        SettingsPrivateKeyPath = appSettings.PrivateKeyPath;
        await LoadConfiguredKeysAsync(isStartup: true);
    }

    private async Task LoadConfiguredKeysAsync(bool isStartup)
    {
        SettingsStatus = string.Empty;
        var messages = new List<string>();

        if (!string.IsNullOrWhiteSpace(SettingsPublicKeyPath))
        {
            var publicKey = await TryLoadKeyAsync(SettingsPublicKeyPath, "Public Key");
            if (publicKey is not null)
            {
                PublicKey = publicKey;
                messages.Add("Public Key geladen");
            }
        }

        if (!string.IsNullOrWhiteSpace(SettingsPrivateKeyPath))
        {
            var privateKey = await TryLoadKeyAsync(SettingsPrivateKeyPath, "Private Key");
            if (privateKey is not null)
            {
                PrivateKey = privateKey;
                messages.Add("Private Key geladen");
            }
        }

        if (messages.Count > 0)
            SettingsStatus = $"✔ {string.Join(", ", messages)}.";
        else if (!isStartup)
            SettingsStatus = "⚠ Keine Keys geladen. Bitte Pfade prüfen.";
    }

    private async Task<string?> TryLoadKeyAsync(string path, string keyName)
    {
        try
        {
            var content = await _storage.ReadTextFileAsync(path);
            if (content is null)
            {
                SettingsStatus = $"⚠ {keyName}-Datei nicht gefunden: {path}";
                return null;
            }
            return content;
        }
        catch (IOException ex)
        {
            SettingsStatus = $"⚠ Fehler beim Lesen von {keyName}: {ex.Message}";
            return null;
        }
        catch (UnauthorizedAccessException ex)
        {
            SettingsStatus = $"⚠ Zugriff verweigert für {keyName}: {ex.Message}";
            return null;
        }
    }
}
