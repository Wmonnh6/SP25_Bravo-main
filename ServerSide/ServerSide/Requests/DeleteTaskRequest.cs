using System.ComponentModel.DataAnnotations;

namespace ServerSide.Requests;

public class DeleteTaskRequest
{
    [Required]
    public int TaskId { get; set; }
}