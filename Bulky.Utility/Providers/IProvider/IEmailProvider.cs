using Bulky.Models.Emails;

namespace Bulky.Utility.Providers.IProvider;

public interface IEmailProvider
{
    Task SendEmailAsync(EmailSettings emailSettings);
}
