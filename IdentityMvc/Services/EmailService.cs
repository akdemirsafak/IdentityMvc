using System.Net;
using System.Net.Mail;
using IdentityMvc.OptionModels;
using Microsoft.Extensions.Options;

namespace IdentityMvc.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;


    public EmailService(IOptions<EmailSettings> options)
    {
        _emailSettings = options.Value;
    }

    public async Task SendResetPasswordEmailAsync(string resetPasswordEmailLink, string toEmail)
    {
        var smtpClient = new SmtpClient();
        smtpClient.Host = _emailSettings.Host;
        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
        smtpClient.UseDefaultCredentials = false; //kendi credential'ımız olacak.
        smtpClient.Port = 587;
        smtpClient.Credentials = new NetworkCredential(_emailSettings.Email, _emailSettings.Password);
        smtpClient.EnableSsl = true;
        var mailMessage = new MailMessage();

        mailMessage.From = new MailAddress(_emailSettings.Email);
        mailMessage.To.Add(toEmail);
        mailMessage.Subject = "Localhost parola sıfırlama.";
        mailMessage.Body = @$"<h4>Şifrenizi sıfırlamak için aşağıdaki linke tıklayınız.</h4><br/>
            <p><a href={resetPasswordEmailLink}>Şifrenizi belirlemek için tıklayınız.</a></p>";
        mailMessage.IsBodyHtml = true;
        await smtpClient.SendMailAsync(mailMessage);
    }
}