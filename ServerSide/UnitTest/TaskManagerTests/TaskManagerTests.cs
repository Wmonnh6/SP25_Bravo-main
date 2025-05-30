using Moq;
using ServerSide.Requests;
using ServerSide.DatabaseContext;
using ServerSide.Models.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ServerSide.Managers.TaskManager;

namespace UnitTest.TaskManagerTests;

public class TaskManagerTests
{
    private readonly Mock<ILoggerFactory> _mockLoggerFactory;
    private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

    public TaskManagerTests()
    {
        // Setup mock logger
        _mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger<TaskManager>>();
        _mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        // Set up in-memory database for testing
        _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    #region GetAllTasksAsync Tests

    [Fact]
    public async Task GetAllTasksAsync_WhenTasksExist_ReturnsSuccessWithTasks()
    {
        // Arrange
        using var context = new ApplicationDbContext(_dbContextOptions);
        var tasks = new List<MyTimeEntryTask>
        {
            new MyTimeEntryTask { Id = 1, Name = "Task 1", IsActive = true, IsTimeOff = false },
            new MyTimeEntryTask { Id = 2, Name = "Task 2", IsActive = true, IsTimeOff = true }
        };
        await context.MyTimeEntryTasks.AddRangeAsync(tasks);
        await context.SaveChangesAsync();

        var taskManager = new TaskManager(context, _mockLoggerFactory.Object);

        // Act
        var result = await taskManager.GetAllTasksAsync();

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Tasks retrieved successfully", result.Message);
        Assert.Equal(2, result.Data.Count);
        Assert.Contains(result.Data, task => task.Name == "Task 1");
        Assert.Contains(result.Data, task => task.Name == "Task 2");
    }

    [Fact]
    public async Task GetAllTasksAsync_WhenNoTasksExist_ReturnsUnsuccessful()
    {
        // Arrange
        using var context = new ApplicationDbContext(_dbContextOptions);
        var taskManager = new TaskManager(context, _mockLoggerFactory.Object);

        // Act
        var result = await taskManager.GetAllTasksAsync();

        // Assert
        Assert.False(result.Success);
        Assert.Equal("No tasks available.", result.Message);
        Assert.Null(result.Data);
    }

    #endregion

    #region AddTaskAsync Tests

    [Fact]
    public async Task AddTaskAsync_WithValidRequest_AddsTaskAndReturnsSuccess()
    {
        // Arrange
        using var context = new ApplicationDbContext(_dbContextOptions);
        var taskManager = new TaskManager(context, _mockLoggerFactory.Object);
        var request = new AddTaskRequest
        {
            Name = "New Task",
            IsActive = true,
            isTimeOff = false
        };

        // Act
        var result = await taskManager.AddTaskAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Task Created Successfully", result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal("New Task", result.Data.Name);
        Assert.True(result.Data.IsActive);
        Assert.False(result.Data.IsTimeOff);

        // Verify task was actually added to the database
        var taskInDb = await context.MyTimeEntryTasks.FirstOrDefaultAsync(t => t.Name == "New Task");
        Assert.NotNull(taskInDb);
    }

    [Fact]
    public async Task AddTaskAsync_WithNullRequest_ReturnsUnsuccessful()
    {
        // Arrange
        using var context = new ApplicationDbContext(_dbContextOptions);
        var taskManager = new TaskManager(context, _mockLoggerFactory.Object);

        // Act
        var result = await taskManager.AddTaskAsync(null);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Task data is required.", result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task AddTaskAsync_WithEmptyName_ReturnsUnsuccessful()
    {
        // Arrange
        using var context = new ApplicationDbContext(_dbContextOptions);
        var taskManager = new TaskManager(context, _mockLoggerFactory.Object);
        var request = new AddTaskRequest
        {
            Name = string.Empty,
            IsActive = true,
            isTimeOff = false
        };

        // Act
        var result = await taskManager.AddTaskAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Task data is required.", result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task AddTaskAsync_WithDuplicateName_ReturnsUnsuccessful()
    {
        // Arrange
        using var context = new ApplicationDbContext(_dbContextOptions);
        await context.MyTimeEntryTasks.AddAsync(new MyTimeEntryTask
        {
            Name = "Existing Task",
            IsActive = true,
            IsTimeOff = false
        });
        await context.SaveChangesAsync();

        var taskManager = new TaskManager(context, _mockLoggerFactory.Object);
        var request = new AddTaskRequest
        {
            Name = "Existing Task", // Duplicate name
            IsActive = true,
            isTimeOff = false
        };

        // Act
        var result = await taskManager.AddTaskAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("A task with this name already exists.", result.Message);
        Assert.Null(result.Data);
    }

    #endregion

    #region DeleteTaskAsync Tests

    [Fact]
    public async Task DeleteTaskAsync_WithExistingId_DeletesTaskAndReturnsSuccess()
    {
        // Arrange
        using var context = new ApplicationDbContext(_dbContextOptions);
        var task = new MyTimeEntryTask
        {
            Id = 1,
            Name = "Task to Delete",
            IsActive = true,
            IsTimeOff = false
        };
        await context.MyTimeEntryTasks.AddAsync(task);
        await context.SaveChangesAsync();

        var taskManager = new TaskManager(context, _mockLoggerFactory.Object);
        var request = new DeleteTaskRequest { TaskId = 1 };

        // Act
        var result = await taskManager.DeleteTaskAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Task deleted successfully.", result.Message);
        Assert.Equal(1, result.Data);

        // Verify task was actually deleted from the database
        var taskInDb = await context.MyTimeEntryTasks.FirstOrDefaultAsync(t => t.Id == 1);
        Assert.Null(taskInDb);
    }

    [Fact]
    public async Task DeleteTaskAsync_WithNonExistingId_ReturnsUnsuccessful()
    {
        // Arrange
        using var context = new ApplicationDbContext(_dbContextOptions);
        var taskManager = new TaskManager(context, _mockLoggerFactory.Object);
        var request = new DeleteTaskRequest { TaskId = 999 }; // Non-existing ID

        // Act
        var result = await taskManager.DeleteTaskAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Couldn't find the task.", result.Message);
        Assert.Equal(0, result.Data); // Default value for int
    }

    #endregion

    #region UpdateTaskAsync Tests

    [Fact]
    public async Task UpdateTaskAsync_WithValidRequest_UpdatesTaskAndReturnsSuccess()
    {
        // Arrange
        using var context = new ApplicationDbContext(_dbContextOptions);
        var task = new MyTimeEntryTask
        {
            Id = 1,
            Name = "Original Task Name",
            IsActive = true,
            IsTimeOff = false
        };
        await context.MyTimeEntryTasks.AddAsync(task);
        await context.SaveChangesAsync();

        var taskManager = new TaskManager(context, _mockLoggerFactory.Object);
        var request = new UpdateTaskRequest
        {
            Id = 1,
            Name = "Updated Task Name",
            IsActive = false,
            IsTimeOff = true
        };

        // Act
        var result = await taskManager.UpdateTaskAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Task updated successfully.", result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal("Updated Task Name", result.Data.Name);
        Assert.False(result.Data.IsActive);
        Assert.True(result.Data.IsTimeOff);

        // Verify task was actually updated in the database
        var taskInDb = await context.MyTimeEntryTasks.FirstOrDefaultAsync(t => t.Id == 1);
        Assert.NotNull(taskInDb);
        Assert.Equal("Updated Task Name", taskInDb.Name);
        Assert.False(taskInDb.IsActive);
        Assert.True(taskInDb.IsTimeOff);
    }

    [Fact]
    public async Task UpdateTaskAsync_WithNullRequest_ReturnsUnsuccessful()
    {
        // Arrange
        using var context = new ApplicationDbContext(_dbContextOptions);
        var taskManager = new TaskManager(context, _mockLoggerFactory.Object);

        // Act
        var result = await taskManager.UpdateTaskAsync(null);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Task name is required.", result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task UpdateTaskAsync_WithNonExistingId_ReturnsUnsuccessful()
    {
        // Arrange
        using var context = new ApplicationDbContext(_dbContextOptions);
        var taskManager = new TaskManager(context, _mockLoggerFactory.Object);
        var request = new UpdateTaskRequest
        {
            Id = 999, // Non-existing ID
            Name = "Updated Task Name",
            IsActive = true,
            IsTimeOff = false
        };

        // Act
        var result = await taskManager.UpdateTaskAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Couldn't find the task.", result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task UpdateTaskAsync_WithDuplicateName_ReturnsUnsuccessful()
    {
        // Arrange
        using var context = new ApplicationDbContext(_dbContextOptions);
        var tasks = new List<MyTimeEntryTask>
        {
            new MyTimeEntryTask
            {
                Id = 1,
                Name = "Task 1",
                IsActive = true,
                IsTimeOff = false
            },
            new MyTimeEntryTask
            {
                Id = 2,
                Name = "Task 2",
                IsActive = true,
                IsTimeOff = false
            }
        };
        await context.MyTimeEntryTasks.AddRangeAsync(tasks);
        await context.SaveChangesAsync();

        var taskManager = new TaskManager(context, _mockLoggerFactory.Object);
        var request = new UpdateTaskRequest
        {
            Id = 1,
            Name = "Task 2", // Name of another existing task
            IsActive = true,
            IsTimeOff = false
        };

        // Act
        var result = await taskManager.UpdateTaskAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("A task with this name already exists.", result.Message);
        Assert.Null(result.Data);
    }

    #endregion
}