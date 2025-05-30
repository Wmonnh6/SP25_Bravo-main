using System.Text;
using ServerSide.Models;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ServerSide.Core.Authentication.IAuthentication;

namespace ServerSide.Core.Authentication;

public static class AuthExtension
{
    public static WebApplicationBuilder AddAuth(this WebApplicationBuilder self)
    {
        var jwtSettings = new JWTSettings();
        self.Configuration.Bind(JWTSettings.SectionName, jwtSettings);

        self.Services.AddSingleton(Options.Create(jwtSettings));
        self.Services.AddTransient<IJWTTokenGenerator, JWTTokenGenerator>();

        self.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
                    };
                });

        self.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "isAdmin" && c.Value == "true")));
        });


        return self;
    }

    public static int? GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return null;
        }

        return int.Parse(userIdClaim.Value);
    }

    public static bool GetIsAdmin(this ClaimsPrincipal user)
    {
    var isAdminClaim = user.FindFirst("isAdmin")?.Value;
    return isAdminClaim != null && bool.Parse(isAdminClaim);  // Returns true or false based on the claim
    }
}
