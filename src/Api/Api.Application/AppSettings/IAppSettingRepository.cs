namespace Heroplate.Api.Application.AppSettings;

public interface IAppSettingRepository
{
    List<AppSettingDto> GetAll();
    string Get(string id);
    Task UpdateAsync(string id, string? value, CancellationToken ct);
}