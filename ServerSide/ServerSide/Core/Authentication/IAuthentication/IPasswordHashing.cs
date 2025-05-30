namespace ServerSide.Core.Authentication.IAuthentication;

public interface IPasswordHashing
{
    string HashPassword(string password);
}