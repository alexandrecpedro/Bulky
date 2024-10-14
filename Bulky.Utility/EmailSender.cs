using Bulky.Models.Emails;
using Bulky.Utility.Enum;
using Bulky.Utility.Factories;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;

namespace Bulky.Utility;

public class EmailSender : IEmailSender
{
    private readonly EmailProviderFactory _emailProviderFactory;
    private readonly string _providerType;

    public EmailSender(EmailProviderFactory emailProviderFactory, IConfiguration configuration)
    {
        _emailProviderFactory = emailProviderFactory;
        _providerType = configuration.GetValue<string>("EmailSender:EmailProvider:Type") ?? nameof(EmailProviderEnum.MailKit);
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var emailProvider = _emailProviderFactory.CreateProvider(providerType: _providerType);
        var emailSettings = new EmailSettings
        {
            Email = email,
            Subject = subject,
            HtmlMessage = htmlMessage
        };
        await emailProvider.SendEmailAsync(emailSettings: emailSettings);
    }



}
