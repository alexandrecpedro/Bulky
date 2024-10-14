using Bulky.Models.Emails;
using Bulky.Utility.Providers.IProvider;
using MailKit.Net.Smtp;
using MimeKit;

namespace Bulky.Utility.Providers;

public class MailKitEmailProvider : IEmailProvider
{
    private readonly SmtpSettings _smtpSettings;

    public MailKitEmailProvider(SmtpSettings smtpSettings)
    {
        _smtpSettings = smtpSettings; 
            //?? throw new ArgumentNullException(paramName: nameof(smtpSettings));
    }

    public async Task SendEmailAsync(EmailSettings emailSettings)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(name: "Bulky", address: _smtpSettings.EmailFrom));
        message.To.Add(new MailboxAddress(name: emailSettings.Email, address: emailSettings.Email));
        message.Subject = emailSettings.Subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = emailSettings.HtmlMessage
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            // connect to SMTP server
            await client.ConnectAsync(
                host: _smtpSettings.SmtpServer,
                port: _smtpSettings.SmtpPort,
                options: MailKit.Security.SecureSocketOptions.StartTls
            );

            // Authentication with SMTP
            await client.AuthenticateAsync(userName: _smtpSettings.SmtpUser, password: _smtpSettings.SmtpPass);

            // Send email
            await client.SendAsync(message: message);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(message: $"Error when sending email: {ex.Message}");
        }
        finally
        {
            await client.DisconnectAsync(quit: true);
            client.Dispose();
        }
    }
}
