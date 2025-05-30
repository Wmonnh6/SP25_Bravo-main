using System.ComponentModel.DataAnnotations;

namespace ServerSide.Requests;

public class InviteUserRequest
{
    /*
     * Backend validation for required and that it is a valid email address format
     * Was going to include a regular expression to prevent sql injection but a) this is bad
     * practice due to the quantity and complexity of sql commands and b) Entity Framework provides
     * quite a bit of built-in protection against SQL injection attacks mitigating the need to
     * handle it here
     */
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    public bool IsAdmin { get; set; }
}
