using System.Text;
using ServerSide.Models;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using ServerSide.Core.Services.IServices;
using ServerSide.Core.Authentication.IAuthentication;

namespace ServerSide.Core.Authentication;

public class JWTTokenGenerator : IJWTTokenGenerator
{
    private readonly IDateTimeProvider DateTimeProvider;
    private readonly JWTSettings JwtOptions;

    public JWTTokenGenerator(IDateTimeProvider dateTimeProvider, IOptions<JWTSettings> jwtOptions)
    {
        DateTimeProvider = dateTimeProvider;
        JwtOptions = jwtOptions.Value;
    }

    public string GenerateJwtToken(int userId, string firstName, string lastName, string email, bool isAdmin)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtOptions.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Name, firstName),
            new(JwtRegisteredClaimNames.FamilyName, lastName),
            new("isAdmin", isAdmin.ToString().ToLower()) // Attach isAdmin claim
        };

        var token = new JwtSecurityToken(
            JwtOptions.Issuer,
            JwtOptions.Audience,
            claims,
            expires: DateTimeProvider.UtcNow.AddHours(24), // Token expiration time
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
