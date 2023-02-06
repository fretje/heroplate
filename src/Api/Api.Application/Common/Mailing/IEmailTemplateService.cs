namespace Heroplate.Api.Application.Common.Mailing;

public interface IEmailTemplateService : ITransientService
{
    string GenerateEmailTemplate<T>(string templateName, T mailTemplateModel);
}