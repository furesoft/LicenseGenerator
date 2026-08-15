using System.Collections.Generic;
using System.Threading.Tasks;

namespace LicenseGenerator.Services;

public interface IProductService
{
    Task<IReadOnlyList<string>> GetProductsAsync();
    Task<bool> ProductExistsAsync(string productName);
    Task CreateProductAsync(string productName, string passphrase);
    Task<(string PublicKey, string PrivateKey)> LoadProductKeysAsync(string productName);
}
