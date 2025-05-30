using ServerSide.Core.Services;
using ServerSide.Managers.Calendar;
using ServerSide.Core.Authentication;
using ServerSide.Managers.UserManager;
using ServerSide.Managers.TaskManager;
using ServerSide.Core.Services.IServices;
using ServerSide.Managers.TimeOffManager;
using ServerSide.Managers.TimeEntryManager;
using ServerSide.Managers.ClosedWeekManager;
using ServerSide.Managers.TimeOffSummaryManager;
using ServerSide.Core.Authentication.IAuthentication;

namespace ServerSide.Core;

public static class RegisterDI
{
    public static void Register(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IUserManager, UserManager>();
        serviceCollection.AddScoped<IJWTTokenGenerator, JWTTokenGenerator>();
        serviceCollection.AddScoped<IPasswordHashing, PasswordHashing>();
        serviceCollection.AddScoped<IEmailService, EmailService>();
        serviceCollection.AddScoped<ITaskManager, TaskManager>();
        serviceCollection.AddScoped<ITimeEntryManager, TimeEntryManager>();
        serviceCollection.AddScoped<ICalendarManager, CalendarManager>();
        serviceCollection.AddScoped<ITimeOffSummaryManager, TimeOffSummaryManager>();
        serviceCollection.AddScoped<IClosedWeekManager, ClosedWeekManager>();
        serviceCollection.AddScoped<ITimeOffManager, TimeOffManager>();
    }
}
