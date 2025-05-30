using ServerSide.DTOs;
using ServerSide.Models;
using Microsoft.AspNetCore.Mvc;
using ServerSide.Managers.Calendar;
using Microsoft.AspNetCore.Authorization;

namespace ServerSide.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CalendarController : ControllerBase
{
    [HttpGet("get-all-time-off-requests")]
    public async Task<ManagerResult<List<EmployeeTimeOffRequestsDTO>>> GetAllTasks([FromServices] ICalendarManager calendarManager)
    {
        return await calendarManager.GetAllEmployeeTimeOffRequestsAsync();
    }
}
