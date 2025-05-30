namespace ServerSide.DTOs;

public class TimeEntryDTO
{
    public int Id { get; set; }
    public UserDTO? User { get; set; }
    public MyTimeEntryTaskDTO? Task { get; set; }
    public int Hours { get; set; }
    public DateTime Date { get; set; }
    public string? Comment { get; set; }
    public TimeOffRequestDTO? TimeOffRequest { get; set; }
}
