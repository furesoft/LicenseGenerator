using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace LicenseGenerator.Services;

public class StorageService : IStorageService
{
    private readonly TopLevel _topLevel;

    public StorageService(TopLevel topLevel) => _topLevel = topLevel;

    public async Task<string?> OpenTextFileAsync()
    {
        var files = await _topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false
        });

        if (files.Count == 0) return null;
        await using var stream = await files[0].OpenReadAsync();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    public async Task SaveTextFileAsync(string suggestedName, string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return;

        var file = await _topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            SuggestedFileName = suggestedName
        });

        if (file is null) return;
        await using var stream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(stream, Encoding.UTF8);
        await writer.WriteAsync(content);
    }

    public async Task<string?> PickOpenFilePathAsync()
    {
        var files = await _topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false
        });

        if (files.Count == 0)
            return null;

        return files[0].Path.LocalPath;
    }

    public async Task<string?> PickFolderPathAsync()
    {
        var folders = await _topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false
        });

        if (folders.Count == 0)
            return null;

        return folders[0].Path.LocalPath;
    }

    public async Task<string?> ReadTextFileAsync(string path)
    {
        if (!File.Exists(path))
            return null;

        return await File.ReadAllTextAsync(path, Encoding.UTF8);
    }
}
