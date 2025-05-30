using ServerSide.DTOs;
using ServerSide.Models;
using ServerSide.Requests;
using Microsoft.AspNetCore.Mvc;
using ServerSide.Managers.TaskManager;
using Microsoft.AspNetCore.Authorization;

namespace ServerSide.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaskController : ControllerBase
{
    // API to Retrieve All Tasks
    [HttpGet("getAllTasks")]
    public async Task<ManagerResult<List<MyTimeEntryTaskDTO>>> GetAllTasks([FromServices] ITaskManager taskManager)
    {
        return await taskManager.GetAllTasksAsync();
    }

    // API to Add Tasks
    [HttpPost("addTask")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ManagerResult<MyTimeEntryTaskDTO>>> AddTask([FromServices] ITaskManager taskManager, [FromBody] AddTaskRequest request)
    {
        return await taskManager.AddTaskAsync(request);
    }

    //API to Delete Tasks
    [HttpDelete("deleteTask")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ManagerResult<int>>> DeleteTask([FromServices] ITaskManager taskManager, [FromBody] DeleteTaskRequest request)
    {
        return await taskManager.DeleteTaskAsync(request);
    }

    //API to Update Tasks
    [HttpPut("updateTask")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ManagerResult<MyTimeEntryTaskDTO>>> UpdateTask([FromServices] ITaskManager taskManager, [FromBody] UpdateTaskRequest request)
    {
        return await taskManager.UpdateTaskAsync(request);
    }
}
