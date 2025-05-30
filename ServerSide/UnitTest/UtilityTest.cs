using System.Net.Mail;
using ServerSide.Core.Services.IServices;

namespace UnitTest;

public class UtilityTest : IClassFixture<TestSetup>
{
    private readonly IEmailService EmailService;

    public UtilityTest(TestSetup setup)
    {
        EmailService = setup.GetService<IEmailService>();
    }


    [Fact]
    public async Task SendEmailTest()
    {
        //MailMessage mailAddress = new()
        //{
        //    Subject = "Test",
        //    Body = "Body Test"
        //};
        //mailAddress.To.Add("team.bravo.2025@gmail.com");
        //var result = await EmailService.SendEmailAsync(mailAddress);
        //Assert.True(result);
    }
}
