using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Common.Options;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace HEVEQ.Infrastructure.Services.Email;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    public EmailService(IOptions<EmailSettings> emailOptions)
    {
        _settings = emailOptions.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        ValidateSettings();
        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        message.To.Add(to);

        using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = new NetworkCredential(_settings.UserName, _settings.Password)
        };

        await client.SendMailAsync(message, cancellationToken);
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_settings.SmtpHost))
            throw new InvalidOperationException("EmailSettings:SmtpHost is missing.");

        if (string.IsNullOrWhiteSpace(_settings.FromEmail))
            throw new InvalidOperationException("EmailSettings:FromEmail is missing.");

        if (string.IsNullOrWhiteSpace(_settings.UserName))
            throw new InvalidOperationException("EmailSettings:UserName is missing.");

        if (string.IsNullOrWhiteSpace(_settings.Password))
            throw new InvalidOperationException("EmailSettings:Password is missing.");
    }
}