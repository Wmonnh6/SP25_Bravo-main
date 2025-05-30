using System.Text;
using ServerSide.Models;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using ServerSide.Core.Authentication.IAuthentication;

namespace ServerSide.Core.Authentication;

public class PasswordHashing : IPasswordHashing
{
    private readonly JWTSettings _jwtOptions;

    public PasswordHashing(IOptions<JWTSettings> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public string HashPassword(string password)
    {
        // Combine the password and the salt
        string saltedPassword = password + _jwtOptions.PasswordSalt;

        // Convert the salted password to bytes
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(saltedPassword));

        // Convert byte array to a hexadecimal string
        StringBuilder builder = new();
        foreach (byte b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }

        return builder.ToString();
    }
}
