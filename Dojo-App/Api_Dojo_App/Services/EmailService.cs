namespace Api_Dojo_App.Services;

using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

public interface IEmailService
{
    Task<bool> SendConfirmationEmailAsync(string email, string confirmationLink);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
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

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(senderEmail));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();

            // MailKit (recomendación oficial de Microsoft; System.Net.Mail.SmtpClient
            // está obsoleto). Puerto 587 = STARTTLS: la conexión empieza en claro y se
            // eleva a TLS con el comando STARTTLS (465 sería TLS directo/SslOnConnect).
            using var smtpClient = new SmtpClient();
            var socketOptions = smtpPort == 465
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;

            await smtpClient.ConnectAsync(smtpServer, smtpPort, socketOptions);
            await smtpClient.AuthenticateAsync(senderEmail, senderPassword);
            await smtpClient.SendAsync(message);
            await smtpClient.DisconnectAsync(true);

            _logger.LogInformation("Email de confirmación enviado");
            return true;
        }
        catch (Exception ex)
        {
            // ILogger y no Debug.WriteLine: en Release, Debug no escribe nada y los
            // fallos de email quedarían invisibles en producción.
            _logger.LogError(ex, "Error enviando email de confirmación");
            return false;
        }
    }
}
