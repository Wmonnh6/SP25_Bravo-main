using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerSide.Models.Entities;

public class TimeEntries
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public required User User { get; set; }

    [MaxLength(512)]
    public string? Comment { get; set; }

    public int Hours { get; set; }
    public int MyTimeEntryTaskId { get; set; }
    
    public required MyTimeEntryTask MyTimeEntryTask { get; set; }

    public DateTime Date {get; set; }

    [ForeignKey("TimeOffRequest")]
    public int? TimeOffRequestId { get; set; }
    public TimeOffRequest? TimeOffRequest { get; set; }
}