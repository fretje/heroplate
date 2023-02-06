namespace Heroplate.Api.Application.Common.FileStorage;

public interface IFileStorageService : ITransientService
{
    public Task<string> UploadAsync<T>(FileUploadRequest? req, FileType supportedFileType, CancellationToken ct = default)
        where T : class;

    public void Remove(string? path);
}