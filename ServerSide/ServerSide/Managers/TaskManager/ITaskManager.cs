using ServerSide.DTOs;
using ServerSide.Models;
using ServerSide.Requests;

namespace ServerSide.Managers.TaskManager;

public interface ITaskManager
{
    Task<ManagerResult<List<MyTimeEntryTaskDTO>>> GetAllTasksAsync();
    Task<ManagerResult<MyTimeEntryTaskDTO>> AddTaskAsync(AddTaskRequest request);
    Task<ManagerResult<int>> DeleteTaskAsync(DeleteTaskRequest request);
    Task<ManagerResult<MyTimeEntryTaskDTO>> UpdateTaskAsync(UpdateTaskRequest request);
}
