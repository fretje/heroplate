namespace Heroplate.Api.Application.Common.Mailing;

public interface IMailService : ITransientService
{
    Task SendAsync(MailRequest req, CancellationToken ct);
}