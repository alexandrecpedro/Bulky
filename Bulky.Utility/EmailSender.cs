using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Bulky.Utility;

public class EmailSender : IEmailSender
{
    private string SendGridSecret { get; set; }
    private string EmailFrom { get; set; }

    public EmailSender(IConfiguration _config)
    {
        SendGridSecret = _config.GetValue<string>("SendGrid:SecretKey") ?? throw new InvalidOperationException("String 'SendGrid Secret' not found!");
        EmailFrom = _config.GetValue<string>("SendGrid:Origin") ?? throw new InvalidOperationException("String 'SendGrid EmailAddress Origin' not found.");
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var client = new SendGridClient(SendGridSecret);

        var from = new EmailAddress(email: EmailFrom, name: "Bulky");
        var to = new EmailAddress(email: email);
        var message = MailHelper.CreateSingleEmail(from: from, to: to, subject: subject, plainTextContent: "", htmlContent: htmlMessage);

        return client.SendEmailAsync(message);
    }
}
