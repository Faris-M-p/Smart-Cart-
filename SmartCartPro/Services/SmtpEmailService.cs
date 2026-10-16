using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using SmartCartPro.Configuration;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public sealed class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<SmtpSettings> settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(
        string toEmail,
        string subject,
        string plainBody,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.IsConfigured)
        {
            throw new InvalidOperationException(
                "SMTP is not configured. Set SmtpSettings (Host, Port, FromEmail, UserName, Password) in appsettings.");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = subject,
            Body = plainBody,
            IsBodyHtml = false
        };
        message.To.Add(toEmail);

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

        if (!string.IsNullOrWhiteSpace(_settings.UserName))
        {
            client.Credentials = new NetworkCredential(_settings.UserName, _settings.Password);
        }

        _logger.LogInformation("Sending email to {ToEmail} via {Host}:{Port}", toEmail, _settings.Host, _settings.Port);
        await client.SendMailAsync(message, cancellationToken);
    }
}
