namespace ServerSide.DTOs;

public class EmployeeTimeOffRequestsDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public DateOnly Date { get; set; }
}
