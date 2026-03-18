using System.Net;
using System.Net.Mail;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly global::Infrastructure.EmailSetting _settings;

    public EmailService(IOptions<global::Infrastructure.EmailSetting> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        var message = new MailMessage
        {
            Subject = subject,
            Body = body,
            From = new MailAddress(_settings.Email)
        };
        message.To.Add(to);

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            Credentials = new NetworkCredential(_settings.Email, _settings.Password),
            EnableSsl = true
        };

        await client.SendMailAsync(message);
    }
}

