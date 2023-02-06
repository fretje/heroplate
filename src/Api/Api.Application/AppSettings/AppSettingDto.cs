namespace Heroplate.Api.Application.AppSettings;

public record AppSettingDto
{
    public string Id { get; }
    public string Category { get; }
    public string Name { get; }
    public string Value { get; init; }

    public AppSettingDto(string id, string value)
    {
        // id is in the form `{category}:{name}`
        string[] idParts = id.Split(':');
        if (idParts.Length != 2)
        {
            throw new ArgumentException($"Invalid AppSetting Id '{id}'.", nameof(id));
        }

        Id = id;
        Category = idParts[0];
        Name = idParts[1];
        Value = value;
    }
}