using System.ComponentModel.DataAnnotations;

namespace ServerSide.Models.Entities;

public class User
{
    public int Id { get; set; }
    [MaxLength(512)]
    public string FirstName { get; set; } = null!;
    [MaxLength(512)]
    public string LastName { get; set; } = null!;
    [MaxLength(512)]
    public string Password { get; set; } = null!;
    [MaxLength(512)]
    public string Email { get; set; } = null!;
    public bool IsAdmin { get; set; }
}
