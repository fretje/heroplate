namespace Heroplate.Api.Infrastructure.Common.Mailing;

public class MailSettings
{
    public string? DisplayName { get; set; }
    public string? From { get; set; }
    public string? Host { get; set; }
    public int Port { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
}