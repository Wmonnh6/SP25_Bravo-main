using ServerSide.DTOs;
using ServerSide.Models;
using ServerSide.Requests;
using Microsoft.AspNetCore.Mvc;
using ServerSide.Core.Authentication;
using Microsoft.AspNetCore.Authorization;
using ServerSide.Managers.TimeEntryManager;

namespace ServerSide.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TimeEntryController : ControllerBase
{

    // API to Retrieve All Time Entries for the Logged-In User
    [HttpGet("getUserTimeEntries")]
    public async Task<ManagerResult<List<TimeEntryDTO>>> GetUserTimeEntries(
        [FromServices] ITimeEntryManager timeEntryManager,
        DateTime date)
    {
        // Extract the userId from the JWT token 
        var userId = User.GetUserId();
        if (userId == null)
        {
            return ManagerResult<List<TimeEntryDTO>>.Unsuccessful("User ID is missing from the token.");
        }

        return await timeEntryManager.GetUserTimeEntriesAsync(userId.Value, date);
    }

    // API for Admins to Retrieve Time Entries of a Specific Employee
    [HttpGet("getEmployeeTimeEntries")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ManagerResult<List<TimeEntryDTO>>> GetEmployeeTimeEntries(
        [FromServices] ITimeEntryManager timeEntryManager,
        [FromQuery] int userId,  // Employee ID
        [FromQuery] DateTime date)
    {
        return await timeEntryManager.GetUserTimeEntriesAsync(userId, date);
    }

    // API to Add a Time Entry
    [HttpPost("addTimeEntry")]
    public async Task<ActionResult<ManagerResult<TimeEntryDTO>>> AddTimeEntry(
        [FromServices] ITimeEntryManager timeEntryManager,
        [FromBody] TimeEntryRequest request)
    {
        // Extract the userId from the JWT token 
        var userId = User.GetUserId();
        if (userId == null)
        {
            return ManagerResult<TimeEntryDTO>.Unsuccessful("User ID is missing from the token.");
        }

        request.UserId = userId.Value;
        return await timeEntryManager.AddTimeEntryAsync(request);
    }

    // API for Admins to Add a Time Entry for Another Employee
    [HttpPost("addOthersTimeEntry")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ManagerResult<TimeEntryDTO>>> AddOthersTimeEntry(
        [FromServices] ITimeEntryManager timeEntryManager,
        [FromBody] TimeEntryRequest request)
    {
        return await timeEntryManager.AddTimeEntryAsync(request);
    }

    // API to Delete a Time Entry
    [HttpDelete("deleteTimeEntry")]
    public async Task<ActionResult<ManagerResult<int>>> DeleteTimeEntry(
    [FromServices] ITimeEntryManager timeEntryManager,
    [FromBody] DeleteTimeEntryRequest request)
    {
        // Extract the userId from the JWT token
        var userId = User.GetUserId();
        if (userId == null)
        {
            return ManagerResult<int>.Unsuccessful("User ID is missing from the token.");
        }

        // Check if user is admin
        bool isAdmin = User.GetIsAdmin();



        return await timeEntryManager.DeleteTimeEntryAsync(request, userId.Value, isAdmin);
    }

    // API for Admins to Delete Any Time Entry
    [HttpDelete("deleteAnyTimeEntry")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ManagerResult<int>>> DeleteAnyTimeEntry(
        [FromServices] ITimeEntryManager timeEntryManager,
        [FromBody] DeleteTimeEntryRequest request)
    {
        // No need to check admin status since the endpoint is admin-only
        var userId = User.GetUserId();
        if (userId == null)
        {
            return ManagerResult<int>.Unsuccessful("User ID is missing from the token.");
        }

        return await timeEntryManager.DeleteAnyTimeEntryAsync(request);
    }

    [HttpPut("updateTimeEntry")]
    public async Task<ActionResult<ManagerResult<TimeEntryDTO>>> UpdateTimeEntry(
        [FromServices] ITimeEntryManager manager,
        [FromBody] TimeEntryRequest request)
    {
        // Get the user id from the JWT to ensure
        // that a user isn't changing someone else's time entry
        // as a non-admin
        var userId = User.GetUserId();
        if (userId == null)
        {
            return ManagerResult<TimeEntryDTO>.Successful("User ID is missing from the token.");
        }

        request.UserId = userId.Value;
        return await manager.UpdateTimeEntryAsync(request);
    }

    // TODO: this section will be for the #21 ticket
    [HttpPut("updateTimeEntry/admin")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ManagerResult<TimeEntryDTO>>> UpdateTimeEntryAsAdmin(
        [FromServices] ITimeEntryManager manager,
        [FromBody] TimeEntryRequest request)
    {
        return await manager.UpdateTimeEntryAsync(request);
    }
}
