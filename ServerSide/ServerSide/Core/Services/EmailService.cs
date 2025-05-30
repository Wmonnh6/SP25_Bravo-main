using System.Net;
using System.Net.Mail;
using ServerSide.Models;
using Microsoft.Extensions.Options;
using ServerSide.Core.Services.IServices;

namespace ServerSide.Core.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettingsModel _emailSettings;

    public EmailService(IOptions<EmailSettingsModel> emailSettings,
        ILogger<EmailService> logger)
    {
        _logger = logger;
        _emailSettings = emailSettings.Value;
    }


    public async Task<bool> SendEmailAsync(MailMessage mailMessage)
    {
        try
        {
            mailMessage.From = new MailAddress(_emailSettings.Username, _emailSettings.SenderName);

            using var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port);
            smtpClient.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
            smtpClient.EnableSsl = _emailSettings.EnableSsl;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

            await smtpClient.SendMailAsync(mailMessage);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Got an exception while sending an email.");

            return false;
        }
    }
}
