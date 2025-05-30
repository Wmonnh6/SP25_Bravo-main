using ServerSide.Models;
using ServerSide.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ServerSide.Managers.ClosedWeekManager;

namespace ServerSide.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClosedWeekController : ControllerBase
{
    //API to Close Weeks for Edits
    [HttpPost("closeWeek")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ManagerResult<DateTime>>> CloseWeek(
        [FromServices] IClosedWeekManager closedWeekManager,
        [FromBody] ClosedWeekRequest request)
    {
        return await closedWeekManager.CloseWeek(request);
    }

    //API to Open Weeks for Edits
    [HttpDelete("openWeek")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ManagerResult<DateTime>>> OpenWeek(
        [FromServices] IClosedWeekManager closedWeekManager,
        [FromBody] ClosedWeekRequest request)
    {
        return await closedWeekManager.OpenWeek(request);
    }

    //API to Check a Weeks Open/Closed Status
    [HttpGet("checkWeekStatus")]
    public async Task<ActionResult<ManagerResult<bool>>> CheckWeekStatus(
        [FromServices] IClosedWeekManager closedWeekManager,
        [FromQuery] DateTime date)
    {
        return await closedWeekManager.CheckWeekStatus(date);
    }
}
