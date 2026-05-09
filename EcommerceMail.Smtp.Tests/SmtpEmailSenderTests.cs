using System.Net.Mail;
using EcommerceMail;
using EcommerceMail.Smtp;
using Microsoft.Extensions.Options;
using Xunit;

namespace EcommerceMail.Smtp.Tests;

public class SmtpEmailSenderTests
{
    [Fact]
    public async Task SendAsync_throws_when_To_is_blank()
    {
        var sender = BuildSender();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            sender.SendAsync(new EmailMessage { To = "", Subject = "hi", BodyText = "hello" }));
    }

    [Fact]
    public async Task SendAsync_throws_when_To_is_whitespace()
    {
        var sender = BuildSender();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            sender.SendAsync(new EmailMessage { To = "   ", Subject = "hi" }));
    }

    [Fact]
    public void Default_options_have_localhost_host_and_port_25()
    {
        var options = new SmtpOptions();
        Assert.Equal("localhost", options.Host);
        Assert.Equal(25, options.Port);
        Assert.False(options.EnableSsl);
    }

    // SmtpClient.SendMailAsync is not virtual, so we cannot reliably capture the MailMessage
    // for assertions in a unit test without manufacturing a test-only seam. The end-to-end
    // SMTP transport path needs an integration test (real or in-process SMTP server).
    private static SmtpEmailSender BuildSender(string fromAddress = "no-reply@demo.com")
    {
        var options = Options.Create(new SmtpOptions { FromAddress = fromAddress });
        return new SmtpEmailSender(options, () => new SmtpClient("localhost", 25));
    }
}
