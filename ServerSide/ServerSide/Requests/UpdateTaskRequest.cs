namespace ServerSide.Requests;

public class UpdateTaskRequest
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsTimeOff { get; set; }
    public bool IsActive { get; set; }
}
