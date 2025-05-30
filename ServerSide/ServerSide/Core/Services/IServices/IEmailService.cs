using System.Net.Mail;

namespace ServerSide.Core.Services.IServices;

public interface IEmailService
{
    Task<bool> SendEmailAsync(MailMessage mailMessage);
}
