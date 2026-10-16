namespace SmartCartPro.Services.Interfaces;

public interface IEmailService
{
    Task SendAsync(string toEmail, string subject, string plainBody, CancellationToken cancellationToken = default);
}
