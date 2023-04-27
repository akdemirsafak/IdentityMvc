namespace Identity.Service.Services;

public interface IEmailService
{
    Task SendResetPasswordEmailAsync(string resetPasswordEmailLink, string toEmail);
    Task ConfirmEmailAsync(string link, string email); //Verify
}