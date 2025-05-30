using System.ComponentModel.DataAnnotations;

namespace ServerSide.Requests;

public class AddTaskRequest
{
    [Required]
    public string Name { get; set; } = null!;
    [Required]
    public bool isTimeOff { get; set; }
    [Required]
    public bool IsActive { get; set; }
}
