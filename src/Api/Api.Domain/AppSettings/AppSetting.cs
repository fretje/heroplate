namespace Heroplate.Api.Domain.AppSettings;

public class AppSetting : BaseEntity<string>, IAggregateRoot
{
    public string TenantId { get; private set; } = default!; // for AppSettingsConfigurationProvider
    public string Value { get; private set; } = default!;

    public AppSetting(string id, string value) => (Id, Value) = (id, value);

    public void Update(string value) => Value = value;
}