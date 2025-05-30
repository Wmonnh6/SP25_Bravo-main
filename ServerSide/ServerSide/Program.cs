using Serilog;
using ServerSide.Core;
using ServerSide.Models;
using Microsoft.OpenApi.Models;
using ServerSide.Core.Services;
using ServerSide.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using ServerSide.Core.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using ServerSide.Core.Services.IServices;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    // Configure Serilog
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console() // Log to console
        .WriteTo.File("Logs/app.log", rollingInterval: RollingInterval.Day) // Log to a file, create a new file daily
        .CreateLogger();
    // Add Serilog as the logging provider
    builder.Host.UseSerilog();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(o =>
    {
        o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
        {
            Name = "Authorization",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer"
        });

        o.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
            },
            Array.Empty<string>()
        }});
    });

    builder.Services.Register();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    );

    builder.AddAuth();
    builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

    builder.Services.Configure<EmailSettingsModel>(builder.Configuration.GetSection(EmailSettingsModel.Position));

    var app = builder.Build();

    // temp
    using var scope = app.Services.CreateScope();
    ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors(options => options
        .WithOrigins(builder.Configuration["HostUrl"]!)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.UseExceptionHandler(appBuilder =>
    {
        appBuilder.Run(async context =>
        {
            Log.Logger.Error(context.Features.Get<IExceptionHandlerFeature>()?.Error, "An unexpected error occurred.");
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";

            var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
            var exception = exceptionFeature?.Error;

            var errorResponse = ManagerResult<bool>.Unsuccessful($"An unexpected error occurred. Exception: {exception?.Message}");

            await context.Response.WriteAsJsonAsync(errorResponse);
        });
    });

    app.Run();

    // Ensure logs are flushed on application exit
    Log.CloseAndFlush();
}
catch (Exception ex)
{
    Log.Logger.Error(ex, "An unexpected error occurred while running the.net application.");
}
