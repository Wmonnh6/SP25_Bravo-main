using System.Net.Mail;
using ServerSide.Core.Services.IServices;

namespace UnitTest;

public class MockEmailService : IEmailService
{
    public Task<bool> SendEmailAsync(MailMessage mailMessage)
    {
        return Task.FromResult(true);
    }
}
