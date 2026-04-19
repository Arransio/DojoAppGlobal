namespace Api_Dojo_App.Services;

using System.Net;
using System.Net.Mail;
using System.Diagnostics;

public interface IEmailService
{
    Task<bool> SendConfirmationEmailAsync(string email, string confirmationLink);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<bool> SendConfirmationEmailAsync(string email, string confirmationLink)
    {
        try
        {
            var emailSettings = _config.GetSection("EmailSettings");
            var smtpServer = emailSettings["SmtpServer"];
            var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
            var senderEmail = emailSettings["SenderEmail"];
            var senderPassword = emailSettings["SenderPassword"];

            using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
            {
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
                smtpClient.Timeout = 10000;

                var subject = "Confirma tu email - Dojo App";
                var body = $@"
                    <html>
                        <body style='font-family: Arial, sans-serif;'>
                            <h2>¡Bienvenido a Dojo App!</h2>
                            <p>Por favor, confirma tu email haciendo clic en el siguiente link:</p>
                            <p><a href='{confirmationLink}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px; display: inline-block;'>Confirmar Email</a></p>
                            <p>O copia y pega este link en tu navegador:</p>
                            <p>{confirmationLink}</p>
                            <p>Este link expirará en 24 horas.</p>
                            <p>Si no creaste esta cuenta, ignora este email.</p>
                        </body>
                    </html>";

                var mailMessage = new MailMessage(senderEmail, email, subject, body)
                {
                    IsBodyHtml = true
                };

                await smtpClient.SendMailAsync(mailMessage);
                Debug.WriteLine($"[EmailService] Email de confirmación enviado a: {email}");
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EmailService] Error enviando email a {email}: {ex.Message}");
            return false;
        }
    }
}
