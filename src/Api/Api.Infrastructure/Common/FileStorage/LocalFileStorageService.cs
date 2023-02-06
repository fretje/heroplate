using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Heroplate.Api.Application.Common.FileStorage;
using Heroplate.Api.Infrastructure.Common.Extensions;

namespace Heroplate.Api.Infrastructure.Common.FileStorage;

public partial class LocalFileStorageService : IFileStorageService
{
    public async Task<string> UploadAsync<T>(FileUploadRequest? req, FileType supportedFileType, CancellationToken ct = default)
        where T : class
    {
        if (req == null || req.Data == null)
        {
            return "";
        }

        if (req.Extension is null || !supportedFileType.GetDescriptionList().Contains(req.Extension.ToLowerInvariant()))
        {
            throw new InvalidOperationException("File Format Not Supported.");
        }

        if (req.Name is null)
        {
            throw new InvalidOperationException("Name is required.");
        }

        string base64Data = DataImageRegex().Match(req.Data).Groups["data"].Value;

        var streamData = new MemoryStream(Convert.FromBase64String(base64Data));
        if (streamData.Length > 0)
        {
            string folder = typeof(T).Name;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                folder = folder.Replace(@"\", "/");
            }

            string folderName = supportedFileType switch
            {
                FileType.Image => Path.Combine("Files", "Images", folder),
                _ => Path.Combine("Files", "Others", folder),
            };
            string pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            Directory.CreateDirectory(pathToSave);

            string fileName = req.Name.Trim('"');
            fileName = RemoveSpecialCharacters(fileName);
            fileName = fileName.ReplaceWhitespace("-");
            fileName += req.Extension.Trim();
            string fullPath = Path.Combine(pathToSave, fileName);
            string dbPath = Path.Combine(folderName, fileName);
            if (File.Exists(dbPath))
            {
                dbPath = NextAvailableFileName(dbPath);
                fullPath = NextAvailableFileName(fullPath);
            }

            using var stream = new FileStream(fullPath, FileMode.Create);
            await streamData.CopyToAsync(stream, ct);
            return dbPath.Replace("\\", "/");
        }
        else
        {
            return "";
        }
    }

    public static string RemoveSpecialCharacters(string str) =>
        SpecialCharsRegex().Replace(str, "");

    public void Remove(string? path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private const string NumberPattern = "-{0}";

    private static string NextAvailableFileName(string path) =>
        File.Exists(path)
            ? Path.HasExtension(path)
                ? GetNextFileName(path.Insert(path.LastIndexOf(Path.GetExtension(path), StringComparison.Ordinal), NumberPattern))
                : GetNextFileName(path + NumberPattern)
            : path;

    private static string GetNextFileName(string pattern)
    {
        string tmp = string.Format(CultureInfo.InvariantCulture, pattern, 1);

        if (!File.Exists(tmp))
        {
            return tmp;
        }

        int min = 1, max = 2;

        while (File.Exists(string.Format(CultureInfo.InvariantCulture, pattern, max)))
        {
            min = max;
            max *= 2;
        }

        while (max != min + 1)
        {
            int pivot = (max + min) / 2;
            if (File.Exists(string.Format(CultureInfo.InvariantCulture, pattern, pivot)))
            {
                min = pivot;
            }
            else
            {
                max = pivot;
            }
        }

        return string.Format(CultureInfo.InvariantCulture, pattern, max);
    }

    [GeneratedRegex("data:image/(?<type>.+?),(?<data>.+)")]
    private static partial Regex DataImageRegex();

    [GeneratedRegex("[^a-zA-Z0-9_.]+", RegexOptions.Compiled)]
    private static partial Regex SpecialCharsRegex();
}