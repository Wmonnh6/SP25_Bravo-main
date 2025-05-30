using ServerSide.DTOs;
using ServerSide.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ServerSide.Managers.TimeOffSummaryManager;

namespace ServerSide.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = "AdminOnly")]
public class TimeOffSummaryController : ControllerBase
{
    // API to Retrieve Time Off Summary for All Employees
    [HttpGet("getTimeOffSummary")]
    public async Task<ManagerResult<List<TimeOffSummaryDTO>>> GetTimeOffSummary([FromQuery] DateTime month, [FromServices] ITimeOffSummaryManager timeOffSummaryManager)
    {
        return await timeOffSummaryManager.GetAllEmployeeTimeOffRequestsAsync(month);
    }
}
