using ServerSide.DTOs;
using ServerSide.Models;
using Microsoft.AspNetCore.Mvc;
using ServerSide.Managers.TimeOffManager;
using Microsoft.AspNetCore.Authorization;

namespace ServerSide.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = "AdminOnly")]
public class TimeOffManagementController : ControllerBase
{
    [HttpPost("get-time-off-requests")]
    public Task<ManagerResult<List<TimeEntryDTO>>> GetAllTimeOffRequests([FromServices] ITimeOffManager timeOffManager, GetTimeOffRequestsRequest request)
    {
        return timeOffManager.GetAllTimeOffRequests(request);
    }

    [HttpPost("approve-time-off-status")]
    public async Task<ActionResult<ManagerResult<string>>> ApproveTimeOffStatus([FromServices] ITimeOffManager timeOffManager,[FromBody] ApproveTimeOffStatusRequest request)
    {
        return await timeOffManager.ApproveTimeOffStatus(request);
    }

    [HttpPost("reject-time-off-request")]
    public async Task<ActionResult<ManagerResult<string>>> RejectTimeOffRequest([FromServices] ITimeOffManager timeOffManager, [FromBody] RejectTimeOffStatusRequest request)
    {
        return await timeOffManager.RejectTimeOffRequest(request);
    }
}
