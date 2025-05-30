using System.ComponentModel.DataAnnotations;

namespace ServerSide.Requests;

public class AddTimeEntryRequest
{
    public int UserId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Hours must be greater than 0.")]
    public int Hours { get; set; }

    [Required]
    public int MyTimeEntryTaskId { get; set; }

    [MaxLength(512)]
    public string? Comment { get; set; }

    [Required]
    public DateTime Date { get; set; }
}
