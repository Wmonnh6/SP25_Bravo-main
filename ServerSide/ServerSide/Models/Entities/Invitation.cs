using System.ComponentModel.DataAnnotations;

namespace ServerSide.Models.Entities;

public class Invitation
{
    public int Id { get; set; }
    public string Token { get; set; } = null!;
    public DateTime DateCreated { get; set; }
    [MaxLength(512)]
    public string Email { get; set; } = null!;
    public bool IsAdmin { get; set; }
}
