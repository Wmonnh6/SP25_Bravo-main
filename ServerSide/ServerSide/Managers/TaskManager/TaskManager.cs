using ServerSide.DTOs;
using ServerSide.Mapper;
using ServerSide.Models;
using ServerSide.Requests;
using ServerSide.DatabaseContext;
using ServerSide.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ServerSide.Managers.TaskManager;

public class TaskManager : BaseManager, ITaskManager
{
    // Constructor to inject ApplicationDbContext and logger
    public TaskManager(
        ApplicationDbContext dbContext,
        ILoggerFactory logger) : base(logger, dbContext)
    {
    }

    // Method to retrieve all tasks
    public async Task<ManagerResult<List<MyTimeEntryTaskDTO>>> GetAllTasksAsync()
    {
        var tasks = await DbContext.MyTimeEntryTasks.ToListAsync();
        if (tasks == null || !tasks.Any())
        {
            return ManagerResult<List<MyTimeEntryTaskDTO>>.Unsuccessful("No tasks available.");
        }

        return ManagerResult<List<MyTimeEntryTaskDTO>>.Successful("Tasks retrieved successfully", tasks.Select(x => x.ToDTO()).ToList());
    }

    // Method to add a new task
    public async Task<ManagerResult<MyTimeEntryTaskDTO>> AddTaskAsync(AddTaskRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Name))
        {
            return ManagerResult<MyTimeEntryTaskDTO>.Unsuccessful("Task data is required.");
        }

        // Check if a task with the same name already exists (assuming 'Name' should be unique)
        var existingTask = await DbContext.MyTimeEntryTasks.FirstOrDefaultAsync(x => x.Name.ToLower() == request.Name.ToLower());
        if (existingTask != null)
        {
            return ManagerResult<MyTimeEntryTaskDTO>.Unsuccessful("A task with this name already exists.");
        }

        // Create the new task
        var newTask = new MyTimeEntryTask
        {
            Name = request.Name,
            IsTimeOff = request.isTimeOff,
            IsActive = request.IsActive
        };

        DbContext.MyTimeEntryTasks.Add(newTask);
        await DbContext.SaveChangesAsync();

        return ManagerResult<MyTimeEntryTaskDTO>.Successful("Task Created Successfully", newTask.ToDTO());
    }

    // Method to delete a task
    public async Task<ManagerResult<int>> DeleteTaskAsync(DeleteTaskRequest request)
    {
        var task = await DbContext.MyTimeEntryTasks.FirstOrDefaultAsync(x => x.Id == request.TaskId);
        if (task == null)
        {
            _logger.LogError("Couldn't find the task with ID: {TaskId}", request.TaskId);
            return ManagerResult<int>.Unsuccessful("Couldn't find the task.");
        }

        DbContext.MyTimeEntryTasks.Remove(task);
        await DbContext.SaveChangesAsync();

        return ManagerResult<int>.Successful("Task deleted successfully.", task.Id);
    }

    // Method to update an existing task
    public async Task<ManagerResult<MyTimeEntryTaskDTO>> UpdateTaskAsync(UpdateTaskRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Name))
        {
            return ManagerResult<MyTimeEntryTaskDTO>.Unsuccessful("Task name is required.");
        }

        // Find the task to update
        var task = await DbContext.MyTimeEntryTasks.FirstOrDefaultAsync(x => x.Id == request.Id);
        if (task == null)
        {
            _logger.LogError("Couldn't find the task with ID: {TaskId}", request.Id);
            return ManagerResult<MyTimeEntryTaskDTO>.Unsuccessful("Couldn't find the task.");
        }

        // Check if a task with the same name already exists (excluding the current task)
        var existingTask = await DbContext.MyTimeEntryTasks
            .FirstOrDefaultAsync(x => x.Name.ToLower() == request.Name.ToLower() && x.Id != request.Id);
        if (existingTask != null)
        {
            return ManagerResult<MyTimeEntryTaskDTO>.Unsuccessful("A task with this name already exists.");
        }

        // Update the task properties
        task.Name = request.Name;
        task.IsTimeOff = request.IsTimeOff;
        task.IsActive = request.IsActive;

        await DbContext.SaveChangesAsync();
        return ManagerResult<MyTimeEntryTaskDTO>.Successful("Task updated successfully.", task.ToDTO());
    }
}
