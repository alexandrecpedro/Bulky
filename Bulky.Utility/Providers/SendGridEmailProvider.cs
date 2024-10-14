using Bulky.Models.Emails;
using Bulky.Utility.Providers.IProvider;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Bulky.Utility.Providers;

public class SendGridEmailProvider : IEmailProvider
{
    private readonly SendGridSettings _sendGridSettings;

    public SendGridEmailProvider(SendGridSettings sendGridSettings)
    {
        _sendGridSettings = sendGridSettings;
    }

    public async Task SendEmailAsync(EmailSettings emailSettings)
    {
        var client = new SendGridClient(_sendGridSettings.ApiKey);

        var from = new EmailAddress(email: _sendGridSettings.EmailFrom, name: "Bulky");
        var to = new EmailAddress(email: emailSettings.Email);
        var message = MailHelper.CreateSingleEmail(from: from, to: to, subject: emailSettings.Subject, plainTextContent: "", htmlContent: emailSettings.HtmlMessage);

        await client.SendEmailAsync(msg: message);
    }
}
