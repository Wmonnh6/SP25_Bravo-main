namespace ServerSide.Requests;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class CreateNewAccountRequest
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;

    [Required]
    [StringLength(50, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long and 50 characters max long.")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[!@#$%*]).+$", ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
    public string Password { get; set; } = null!;

    [Required]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = null!;
    public string? InvitationToken { get; set; }
    [JsonIgnore]
    public bool IsAdmin { get; set; }
}
