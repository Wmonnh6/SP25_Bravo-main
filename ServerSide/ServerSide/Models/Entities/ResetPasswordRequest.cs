namespace ServerSide.Models.Entities;

public class ResetPasswordRequest
{
    public int Id { get; set; }
    public string Token { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime DateCreated { get; set; }
}
