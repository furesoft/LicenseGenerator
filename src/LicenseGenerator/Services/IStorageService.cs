using System.Threading.Tasks;

namespace LicenseGenerator.Services;

public interface IStorageService
{
    Task<string?> OpenTextFileAsync();
    Task SaveTextFileAsync(string suggestedName, string content);
    Task<string?> PickOpenFilePathAsync();
    Task<string?> ReadTextFileAsync(string path);
}
