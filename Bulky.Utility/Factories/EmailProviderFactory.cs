using Bulky.Models.Emails;
using Bulky.Utility.Enum;
using Bulky.Utility.Providers;
using Bulky.Utility.Providers.IProvider;

namespace Bulky.Utility.Factories;

public class EmailProviderFactory
{
    private readonly SmtpSettings _smtpSettings;
    //private readonly SendGridSettings _sendGridSettings;

    public EmailProviderFactory(SmtpSettings smtpSettings)
    {
        _smtpSettings = smtpSettings;
        //_sendGridSettings = sendGridSettings;
    }

    public IEmailProvider CreateProvider(string providerType)
    {
        return providerType switch
        {
            nameof(EmailProviderEnum.MailKit) => new MailKitEmailProvider(smtpSettings: _smtpSettings),
            //nameof(EmailProviderEnum.SendGrid) => new SendGridEmailProvider(sendGridSettings: _sendGridSettings),
            _ => throw new InvalidOperationException(message: "Invalid email provider type!")
        };
    }
}
