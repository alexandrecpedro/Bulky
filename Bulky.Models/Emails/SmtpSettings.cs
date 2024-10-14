namespace Bulky.Models.Emails;

public record SmtpSettings(
    string SmtpServer,
    int SmtpPort,
    string SmtpUser,
    string SmtpPass,
    string EmailFrom
);
