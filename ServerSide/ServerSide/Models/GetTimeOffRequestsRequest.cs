namespace ServerSide.Models;

public class GetTimeOffRequestsRequest
{
    public int? UserId { get; set; }
    public string? RequestStatus { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
