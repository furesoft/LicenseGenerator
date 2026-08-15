using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using LicenseGenerator.Models;

namespace LicenseGenerator.Services;

public class SettingsService : ISettingsService
{
    private static readonly string SettingsDirectory =
        Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "LicenseGenerator");

    private static readonly string SettingsPath = Path.Combine(SettingsDirectory, "settings.json");

    public async Task<AppSettings> LoadAsync()
    {
        if (!File.Exists(SettingsPath))
            return new AppSettings();

        var json = await File.ReadAllTextAsync(SettingsPath);
        return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
    }

    public async Task SaveAsync(AppSettings settings)
    {
        Directory.CreateDirectory(SettingsDirectory);
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(SettingsPath, json);
    }
}
