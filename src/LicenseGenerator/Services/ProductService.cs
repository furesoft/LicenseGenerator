using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LicenseGenerator.Services;

public class ProductService : IProductService
{
    private readonly LicensingService _licensingService;
    private readonly string _productsRootPath;

    public ProductService(LicensingService licensingService)
    {
        _licensingService = licensingService;
        _productsRootPath = Path.Combine(AppContext.BaseDirectory, "products");
    }

    public Task<IReadOnlyList<string>> GetProductsAsync()
    {
        if (!Directory.Exists(_productsRootPath))
            return Task.FromResult<IReadOnlyList<string>>([]);

        var products = Directory.GetDirectories(_productsRootPath)
            .Select(Path.GetFileName)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .Cast<string>()
            .ToArray();

        return Task.FromResult<IReadOnlyList<string>>(products);
    }

    public Task<bool> ProductExistsAsync(string productName)
    {
        var normalized = NormalizeAndValidateProductName(productName);
        var productPath = GetProductPath(normalized);
        return Task.FromResult(Directory.Exists(productPath));
    }

    public async Task CreateProductAsync(string productName, string passphrase)
    {
        var normalized = NormalizeAndValidateProductName(productName);
        if (string.IsNullOrWhiteSpace(passphrase))
            throw new ArgumentException("Passphrase darf nicht leer sein.", nameof(passphrase));

        var productPath = GetProductPath(normalized);
        var publicKeyPath = Path.Combine(productPath, "key.pub");
        var privateKeyPath = Path.Combine(productPath, "key.priv");

        if (File.Exists(publicKeyPath) && File.Exists(privateKeyPath))
            return;

        Directory.CreateDirectory(productPath);

        var keyPair = _licensingService.GenerateKeyPair(passphrase);

        await File.WriteAllTextAsync(publicKeyPath, keyPair.PublicKey, Encoding.UTF8);
        await File.WriteAllTextAsync(privateKeyPath, keyPair.PrivateKey, Encoding.UTF8);
    }

    public async Task<(string PublicKey, string PrivateKey)> LoadProductKeysAsync(string productName)
    {
        var normalized = NormalizeAndValidateProductName(productName);
        var productPath = GetProductPath(normalized);
        var publicKeyPath = Path.Combine(productPath, "key.pub");
        var privateKeyPath = Path.Combine(productPath, "key.priv");

        if (!File.Exists(publicKeyPath))
            throw new FileNotFoundException("Public Key Datei nicht gefunden.", publicKeyPath);

        if (!File.Exists(privateKeyPath))
            throw new FileNotFoundException("Private Key Datei nicht gefunden.", privateKeyPath);

        var publicKey = await File.ReadAllTextAsync(publicKeyPath, Encoding.UTF8);
        var privateKey = await File.ReadAllTextAsync(privateKeyPath, Encoding.UTF8);
        return (publicKey, privateKey);
    }

    public async Task ExportProductKeysAsync(string productName, string destinationDirectory)
    {
        if (string.IsNullOrWhiteSpace(destinationDirectory))
            throw new ArgumentException("Zielordner darf nicht leer sein.", nameof(destinationDirectory));

        var (publicKey, privateKey) = await LoadProductKeysAsync(productName);

        if (!Directory.Exists(destinationDirectory))
            Directory.CreateDirectory(destinationDirectory);

        var targetPublicKeyPath = Path.Combine(destinationDirectory, "key.pub");
        var targetPrivateKeyPath = Path.Combine(destinationDirectory, "key.priv");

        await File.WriteAllTextAsync(targetPublicKeyPath, publicKey, Encoding.UTF8);
        await File.WriteAllTextAsync(targetPrivateKeyPath, privateKey, Encoding.UTF8);
    }

    private string GetProductPath(string productName) => Path.Combine(_productsRootPath, productName);

    private static string NormalizeAndValidateProductName(string productName)
    {
        var normalized = productName.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException("Produktname darf nicht leer sein.", nameof(productName));

        if (normalized.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new ArgumentException("Produktname enthält ungültige Zeichen.", nameof(productName));

        return normalized;
    }
}
