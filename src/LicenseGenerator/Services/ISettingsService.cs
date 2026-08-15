using System.Threading.Tasks;
using LicenseGenerator.Models;

namespace LicenseGenerator.Services;

public interface ISettingsService
{
    Task<AppSettings> LoadAsync();
    Task SaveAsync(AppSettings settings);
}
