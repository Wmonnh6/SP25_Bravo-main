namespace ServerSide.DTOs;

public class MyTimeEntryTaskDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; }
    public bool IsTimeOff { get; set; }
}
