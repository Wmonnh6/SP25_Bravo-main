namespace ServerSide.Requests;

public class ChangePasswordRequest
{
    public string Email { get; set; } = null!;
    public string OldPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}
