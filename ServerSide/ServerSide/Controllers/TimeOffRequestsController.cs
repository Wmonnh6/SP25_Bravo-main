using ServerSide.DTOs;
using ServerSide.Models;
using ServerSide.Requests;
using Microsoft.AspNetCore.Mvc;
using ServerSide.Managers.TimeOffManager;
using Microsoft.AspNetCore.Authorization;
using ServerSide.Core.Authentication;

namespace ServerSide.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TimeOffRequestsController : ControllerBase
{
    // API to Retrieve All Time Off Requests for the Logged-In User
    [HttpGet("getUserTimeOffRequests")]
    public async Task<ManagerResult<List<TimeEntryDTO>>> GetUserTimeOffRequests(
        [FromServices] ITimeOffManager timeOffManager)
    {
        // Extract the userId from the JWT token 
        var userId = User.GetUserId();
        if (userId == null)
        {
            return ManagerResult<List<TimeEntryDTO>>.Unsuccessful("User ID is missing from the token.");
        }

        return await timeOffManager.GetUserTimeOffRequestsAsync(userId.Value);
    }

    //API to Delete a Time Off Request for the Logged-In User
    [HttpDelete("deleteUserTimeOffRequest")]
    public async Task<ManagerResult<int>> DeleteUserTimeOffRequestAsync(
        [FromServices] ITimeOffManager timeOffManager,
        [FromBody] DeleteTimeOffRequest request)
    {
        // Extract the userId from the JWT token 
        var userId = User.GetUserId();
        if (userId == null)
        {
            return ManagerResult<int>.Unsuccessful("User ID is missing from the token.");
        }

        return await timeOffManager.DeleteUserTimeOffRequestAsync(request, userId.Value);
    }
}
