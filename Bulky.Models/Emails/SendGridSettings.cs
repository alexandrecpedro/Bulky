namespace Bulky.Models.Emails;

public record SendGridSettings(
    string ApiKey,
    string EmailFrom
);
