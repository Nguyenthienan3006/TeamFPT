using MailKit.Net.Smtp;
using MimeKit;

public class EmailService
{
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPassword;

    public EmailService(string smtpServer, int smtpPort, string smtpUser, string smtpPassword)
    {
        _smtpServer = smtpServer;
        _smtpPort = smtpPort;
        _smtpUser = smtpUser;
        _smtpPassword = smtpPassword;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("Admin", _smtpUser));
        email.To.Add(new MailboxAddress("", toEmail));
        email.Subject = subject;
        email.Body = new TextPart("plain")
        {
            Text = message
        };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_smtpServer, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_smtpUser, _smtpPassword);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}
