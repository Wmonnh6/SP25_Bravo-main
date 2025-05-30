using System.Net.Mail;
using ServerSide.Core.Services.IServices;

namespace UnitTest;

public class FailingEmailService : IEmailService
{
    public Task<bool> SendEmailAsync(MailMessage message)
    {
        return Task.FromResult(false); // simulate failure
    }
}
