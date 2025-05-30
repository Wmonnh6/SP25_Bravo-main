namespace ServerSide.Core.Authentication.IAuthentication;

public interface IJWTTokenGenerator
{
    string GenerateJwtToken(int userId, string firstName, string lastName, string email, bool isAdmin);
}
