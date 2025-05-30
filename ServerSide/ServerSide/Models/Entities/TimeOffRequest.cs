using System.ComponentModel.DataAnnotations;

namespace ServerSide.Models.Entities;

public class TimeOffRequest
{
    [Key]
    public int Id { get; set; }
    public TimeOffRequestStatusEnum Status { get; set; }
}
