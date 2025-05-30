namespace ServerSide.Requests;

public class LoginRequest
{
    public string Password { get; set; } = null!;
    public string Email { get; set; } = null!;
}
