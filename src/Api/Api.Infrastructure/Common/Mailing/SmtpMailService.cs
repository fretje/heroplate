using Heroplate.Api.Application.Common.Mailing;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Heroplate.Api.Infrastructure.Common.Mailing;

public class SmtpMailService : IMailService
{
    private readonly MailSettings _settings;
    private readonly ILogger<SmtpMailService> _logger;

    public SmtpMailService(IOptions<MailSettings> settings, ILogger<SmtpMailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(MailRequest req, CancellationToken ct = default)
    {
        try
        {
            var email = new MimeMessage();

            // From
            email.From.Add(new MailboxAddress(_settings.DisplayName, req.From ?? _settings.From));

            // To
            foreach (string address in req.To)
            {
                email.To.Add(MailboxAddress.Parse(address));
            }

            // Reply To
            if (!string.IsNullOrEmpty(req.ReplyTo))
            {
                email.ReplyTo.Add(new MailboxAddress(req.ReplyToName, req.ReplyTo));
            }

            // Bcc
            if (req.Bcc != null)
            {
                foreach (string address in req.Bcc.Where(bccValue => !string.IsNullOrWhiteSpace(bccValue)))
                {
                    email.Bcc.Add(MailboxAddress.Parse(address.Trim()));
                }
            }

            // Cc
            if (req.Cc != null)
            {
                foreach (string? address in req.Cc.Where(ccValue => !string.IsNullOrWhiteSpace(ccValue)))
                {
                    email.Cc.Add(MailboxAddress.Parse(address.Trim()));
                }
            }

            // Headers
            if (req.Headers != null)
            {
                foreach (var header in req.Headers)
                {
                    email.Headers.Add(header.Key, header.Value);
                }
            }

            // Content
            var builder = new BodyBuilder();
            email.Sender = new MailboxAddress(req.DisplayName ?? _settings.DisplayName, req.From ?? _settings.From);
            email.Subject = req.Subject;
            builder.HtmlBody = req.Body;

            // Create the file attachments for this e-mail message
            if (req.AttachmentData != null)
            {
                foreach (var attachmentInfo in req.AttachmentData)
                {
                    builder.Attachments.Add(attachmentInfo.Key, attachmentInfo.Value);
                }
            }

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls, ct);
            await smtp.AuthenticateAsync(_settings.UserName, _settings.Password, ct);
            await smtp.SendAsync(email, ct);
            await smtp.DisconnectAsync(true, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while sending mail.");
        }
    }
}