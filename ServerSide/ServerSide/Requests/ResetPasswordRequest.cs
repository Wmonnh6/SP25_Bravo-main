namespace ServerSide.Requests;

public class ResetPasswordRequest
{
    public string ResetToken { get; set; } = null!;
    public string newPassword { get; set; } = null!;
}
