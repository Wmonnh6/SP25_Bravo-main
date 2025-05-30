using System.ComponentModel.DataAnnotations;

namespace ServerSide.Requests;

public class UpdateUserRequest
{
    [MaxLength(512)]
    [RegularExpression("^[\\w]+")]
    public string FirstName { get; set; } = null!;
    [MaxLength(512)]
    [RegularExpression("^[\\w]+")]
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    [StringLength(512, MinimumLength = 8)]
    [RegularExpression("^(?=.*[A-Z])(?=.*[a-z])(?=.*\\d)(?=.*[!@#$%*]).+$")]
    public string? NewPassword { get; set; }
}
