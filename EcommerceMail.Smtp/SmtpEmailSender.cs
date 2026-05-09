using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace EcommerceMail.Smtp;

public class SmtpEmailSender : IEmailSender, IDisposable
{
    private readonly Func<SmtpClient> _clientFactory;
    private readonly SmtpOptions _options;
    private bool _disposed;

    public SmtpEmailSender(IOptions<SmtpOptions> options)
        : this(options, () => CreateDefaultClient(options.Value))
    {
    }

    internal SmtpEmailSender(IOptions<SmtpOptions> options, Func<SmtpClient> clientFactory)
    {
        _options = options.Value;
        _clientFactory = clientFactory;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message.To))
        {
            throw new ArgumentException("EmailMessage.To is required.", nameof(message));
        }

        using var mail = new MailMessage
        {
            From = string.IsNullOrWhiteSpace(_options.FromName)
                ? new MailAddress(_options.FromAddress)
                : new MailAddress(_options.FromAddress, _options.FromName),
            Subject = message.Subject ?? string.Empty,
            Body = message.BodyHtml ?? message.BodyText ?? string.Empty,
            IsBodyHtml = !string.IsNullOrEmpty(message.BodyHtml)
        };

        var to = string.IsNullOrWhiteSpace(message.ToName)
            ? new MailAddress(message.To)
            : new MailAddress(message.To, message.ToName);
        mail.To.Add(to);

        if (!string.IsNullOrEmpty(message.BodyHtml) && !string.IsNullOrEmpty(message.BodyText))
        {
            mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
                message.BodyText, null, "text/plain"));
        }

        using var client = _clientFactory();
        await client.SendMailAsync(mail, cancellationToken);
    }

    private static SmtpClient CreateDefaultClient(SmtpOptions options)
    {
        var client = new SmtpClient(options.Host, options.Port)
        {
            EnableSsl = options.EnableSsl
        };

        if (!string.IsNullOrEmpty(options.Username))
        {
            client.Credentials = new NetworkCredential(options.Username, options.Password);
        }

        return client;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
