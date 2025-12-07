using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace AlarmManagement.Services;

public class EmailNotificationService
{
    private readonly ILogger<EmailNotificationService> _logger;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPass;
    private readonly string _fromEmail;

    public EmailNotificationService(ILogger<EmailNotificationService> logger)
    {
        _logger = logger;
        _smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "smtp.gmail.com";
        _smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
        _smtpUser = Environment.GetEnvironmentVariable("SMTP_USER") ?? "";
        _smtpPass = Environment.GetEnvironmentVariable("SMTP_PASS") ?? "";
        _fromEmail = Environment.GetEnvironmentVariable("SMTP_FROM") ?? "scada-alerts@yourdomain.com";
    }

    public async Task SendAlarmNotificationAsync(string toEmail, string subject, string body)
    {
        if (string.IsNullOrEmpty(_smtpUser) || string.IsNullOrEmpty(_smtpPass))
        {
            _logger.LogWarning("SMTP credentials not configured, skipping email");
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("SCADA Alarm System", _fromEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2 style='color: #dc2626;'>⚠️ SCADA Alarm Alert</h2>
                        <div style='background: #fee2e2; padding: 15px; border-left: 4px solid #dc2626;'>
                            {body}
                        </div>
                        <p style='color: #666; font-size: 12px; margin-top: 20px;'>
                            This is an automated alert from your SCADA system.
                        </p>
                    </body>
                    </html>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_smtpUser, _smtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Sent email notification to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email notification");
        }
    }
}
