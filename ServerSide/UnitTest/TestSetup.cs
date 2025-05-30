using ServerSide.Models;
using ServerSide.Core.Services;
using ServerSide.DatabaseContext;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ServerSide.Core.Authentication;
using ServerSide.Managers.UserManager;
using ServerSide.Managers.TimeOffManager;
using ServerSide.Core.Services.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServerSide.Core.Authentication.IAuthentication;
using ServerSide.Managers.TimeEntryManager;

namespace UnitTest;

public class TestSetup : IDisposable
{
    public ServiceProvider ServiceProvider { get; private set; }

    public TestSetup()
    {
        // Setup the service collection
        var services = new ServiceCollection();

        var config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.Test.json", optional: true)
        .Build();

        // Configure InMemory database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));

        services.AddLogging(configure => configure.AddConsole());

        // Register services
        services.AddScoped<IUserManager, UserManager>();
        services.AddScoped<ITimeOffManager, TimeOffManager>();
        services.AddScoped<IEmailService, MockEmailService>();
        services.AddScoped<IJWTTokenGenerator, JWTTokenGenerator>();
        services.AddScoped<IPasswordHashing, PasswordHashing>();
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<ITimeEntryManager, TimeEntryManager>();
        services.Configure<EmailSettingsModel>(config.GetSection(EmailSettingsModel.Position));
        services.Configure<JWTSettings>(config.GetSection(JWTSettings.SectionName));

        // Build the service provider
        ServiceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        // Dispose the service provider
        GC.SuppressFinalize(this);
    }
}
