using ServerSide.DTOs;
using ServerSide.Models;
using ServerSide.Requests;
using ServerSide.Models.Entities;
using ServerSide.DatabaseContext;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ServerSide.Managers.TimeOffManager;

namespace UnitTest;

public class TimeOffTests : IClassFixture<TestSetup>, IDisposable
{
    private readonly ITimeOffManager TimeOffManager;
    private readonly ApplicationDbContext Context;

    /// <summary>
    /// Use the constructor to inject services and setup the test
    /// </summary>
    public TimeOffTests(TestSetup setup)
    {
        TimeOffManager = setup.GetService<ITimeOffManager>();
        Context = new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TimeOffTests")
            .Options
        );

        Dispose();
        Context.Database.EnsureDeleted();
        Context.Database.EnsureCreated();


        TimeOffManager = new TimeOffManager(
            new LoggerFactory(),
            Context,
            new MockEmailService());

        PopulateTestData();
    }

    /**
     * Tests for method: GetAllTimeOffRequests
     */

    [Fact]
    public async void GetAllTimeOffRequests_NullParams_MultipleUsers_ReturnsAllRequestsSuccessfully()
    {
        //Arrange
        //Add another user who has a time off request
        User newUser = new()
        {
            Email = "email@email.com",
            Password = "PWtest3#",
            FirstName = "Test",
            LastName = "Test",
            IsAdmin = false
        };
        Context.Users.Add(newUser);

        TimeOffRequest timeOff = new()
        {
            Status = ServerSide.Models.TimeOffRequestStatusEnum.Pending
        };
        Context.TimeOffRequests.Add(timeOff);

        TimeEntries newEntry = new()
        {
            User = newUser,
            UserId = newUser.Id,
            MyTimeEntryTask = new() { Name = "TimeOff", IsTimeOff = true },
            MyTimeEntryTaskId = 2,
            Hours = 1,
            TimeOffRequest = timeOff,
            TimeOffRequestId = timeOff.Id
        };
        Context.TimeEntries.Add(newEntry);
        Context.SaveChanges();

        //Make a new request to pass to the method
        GetTimeOffRequestsRequest req = new();

        var result = await TimeOffManager.GetAllTimeOffRequests(req);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count); 
    }

    [Fact]
    public async void GetAllTimeOffRequests_UserFilter_ReturnsUsersRequestsSuccessfully()
    {
        //Arrange
        //Add another user who has a time off request to ensure the method filter works
        User newUser = new()
        {
            Email = "email@email.com",
            Password = "PWtest3#",
            FirstName = "Test",
            LastName = "Test",
            IsAdmin = false
        };
        Context.Users.Add(newUser);

        TimeOffRequest timeOff = new()
        {
            Status = ServerSide.Models.TimeOffRequestStatusEnum.Pending
        };
        Context.TimeOffRequests.Add(timeOff);

        TimeEntries newEntry = new()
        {
            User = newUser,
            UserId = newUser.Id,
            MyTimeEntryTask = new() { Name = "TimeOff", IsTimeOff = true },
            MyTimeEntryTaskId = 2,
            Hours = 1,
            TimeOffRequest = timeOff,
            TimeOffRequestId = timeOff.Id
        };
        Context.TimeEntries.Add(newEntry);
        Context.SaveChanges();

        //Make a new request to pass to the method
        GetTimeOffRequestsRequest req = new()
        {
            UserId = 1 //known user
        };

        var result = await TimeOffManager.GetAllTimeOffRequests(req);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.Count);
        result.Data.ForEach(te => Assert.Equal(1, te.User.Id)); //Contains only the specified user's request(s), not others'
    }

    [Fact]
    public async void GetAllTimeOffRequests_StatusFilter_ReturnsRequestsSuccessfully()
    {
        //Arrange
        var user = Context.Users.Find(1); //get existing user
        var task = Context.MyTimeEntryTasks.Find(2); //get existing time off task
        //Add time off requests of different statuses
        TimeOffRequest timeOff = new()
        {
            Status = ServerSide.Models.TimeOffRequestStatusEnum.Approved
        };
        Context.TimeOffRequests.Add(timeOff);

        TimeEntries newEntry = new()
        {
            User = user,
            UserId = user.Id,
            MyTimeEntryTask = task,
            MyTimeEntryTaskId = 2,
            Hours = 1,
            TimeOffRequest = timeOff,
            TimeOffRequestId = timeOff.Id
        };
        Context.TimeEntries.Add(newEntry);

        TimeOffRequest timeOff2 = new()
        {
            Status = ServerSide.Models.TimeOffRequestStatusEnum.Rejected
        };
        Context.TimeOffRequests.Add(timeOff2);

        TimeEntries newEntry2 = new()
        {
            User = user,
            UserId = user.Id,
            MyTimeEntryTask = task,
            MyTimeEntryTaskId = 2,
            Hours = 1,
            TimeOffRequest = timeOff2,
            TimeOffRequestId = timeOff2.Id
        };
        Context.TimeEntries.Add(newEntry2);
        Context.SaveChanges();

        //Make a new request to pass to the method
        GetTimeOffRequestsRequest req = new()
        {
            RequestStatus = "Approved"
        };

        var result = await TimeOffManager.GetAllTimeOffRequests(req);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.Count);
        result.Data.ForEach(te => Assert.Equal("Approved", te.TimeOffRequest.Status)); //Contains only requests of the "Approved" status
    }

    [Fact]
    public async void GetAllTimeOffRequests_StartAndEndDateFilter_ReturnsRequestsBetweenDatesSuccessfully()
    {
        //Arrange
        var user = Context.Users.Find(1); //get existing user
        var task = Context.MyTimeEntryTasks.Find(2); //get existing time off task

        //Add time off requests for different weeks
        TimeOffRequest timeOff = new()
        {
            Status = ServerSide.Models.TimeOffRequestStatusEnum.Approved
        };
        Context.TimeOffRequests.Add(timeOff);

        TimeEntries newEntry = new()
        {
            User = user,
            UserId = user.Id,
            MyTimeEntryTask = task,
            MyTimeEntryTaskId = 2,
            Hours = 1,
            TimeOffRequest = timeOff,
            TimeOffRequestId = timeOff.Id,
            Date = DateTime.Parse("03-03-03") //date in the past
        }; 
        Context.TimeEntries.Add(newEntry);

        TimeOffRequest timeOff2 = new()
        {
            Status = ServerSide.Models.TimeOffRequestStatusEnum.Rejected
        };
        Context.TimeOffRequests.Add(timeOff2);

        TimeEntries newEntry2 = new()
        {
            User = user,
            UserId = user.Id,
            MyTimeEntryTask = task,
            MyTimeEntryTaskId = 2,
            Hours = 1,
            TimeOffRequest = timeOff2,
            TimeOffRequestId = timeOff2.Id,
            Date = DateTime.Parse("03-5-25") 
        };
        Context.TimeEntries.Add(newEntry2);

        TimeOffRequest timeOff3 = new()
        {
            Status = ServerSide.Models.TimeOffRequestStatusEnum.Pending
        };
        Context.TimeOffRequests.Add(timeOff3);

        TimeEntries newEntry3 = new()
        {
            User = user,
            UserId = user.Id,
            MyTimeEntryTask = task,
            MyTimeEntryTaskId = 2,
            Hours = 1,
            TimeOffRequest = timeOff3,
            TimeOffRequestId = timeOff3.Id,
            Date = DateTime.Parse("03-11-25")
        };
        Context.TimeEntries.Add(newEntry3);

        Context.SaveChanges();

        DateTime start = DateTime.Parse("03-5-25");
        DateTime end = DateTime.Parse("03-12-25");

        //Make a new request to pass to the method
        GetTimeOffRequestsRequest req = new()
        {
            StartDate = start,
            EndDate = end,
        };

        var result = await TimeOffManager.GetAllTimeOffRequests(req);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        result.Data.ForEach(te => Assert.True(te.Date >= start && te.Date <= end)); //Contains only requests within the date range
    }

    /**
     * Tests for method: GetUserTimeOffRequests
     */

    [Fact]
    public async void GetUserTimeOffRequests_ValidUserWithTimeOffRequests_ReturnsSuccessfulResultAndList()
    {
        //get time off requests for a known user
        var result = await TimeOffManager.GetUserTimeOffRequestsAsync(1);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Count == 1);

    }

    [Fact]
    public async void GetUserTimeOffRequests_ValidUserWithTimeOffRequests_ReturnsUsersRequestsOnly()
    {
        //Arrange
        var task = Context.MyTimeEntryTasks.Find(2); //get existing time off task
        //Add another user who has a time off request
        User newUser = new()
        {
            Email = "email@email.com",
            Password = "PWtest3#",
            FirstName = "Test",
            LastName = "Test",
            IsAdmin = false
        };
        Context.Users.Add(newUser);

        TimeOffRequest timeOff = new()
        {
            Status = ServerSide.Models.TimeOffRequestStatusEnum.Pending
        };
        Context.TimeOffRequests.Add(timeOff);

        TimeEntries newEntry = new()
        {
            User = newUser,
            UserId = newUser.Id,
            MyTimeEntryTask = task,
            MyTimeEntryTaskId = 2,
            Hours = 1,
            TimeOffRequest = timeOff,
            TimeOffRequestId = timeOff.Id
        };
        Context.TimeEntries.Add(newEntry);
        Context.SaveChanges();

        //Get time off requests for a known user
        var result = await TimeOffManager.GetUserTimeOffRequestsAsync(1);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Count == 1);
        result.Data.ForEach(te => Assert.Equal(1, te.User.Id)); //Contains only the specified user's request(s), not others'

    }

    [Fact]
    public async void GetUserTimeOffRequests_ValidUser_RequestsFromDifWeeks_ReturnsSuccessful()
    {
        //Arrange 
        //Add new time off requests from more than a week away in the past and future
        //In order to check that the method returns all requests regardless of date

        var user = Context.Users.Find(1); //get existing user
        var task = Context.MyTimeEntryTasks.Find(2); //get existing time off task

        TimeOffRequest timeOff = new()
        {
            Status = ServerSide.Models.TimeOffRequestStatusEnum.Pending
        };
        Context.TimeOffRequests.Add(timeOff);

        TimeEntries newEntry = new()
        {
            User = user,
            UserId = 1,
            MyTimeEntryTask = task,
            MyTimeEntryTaskId = 2,
            Hours = 1,
            TimeOffRequest = timeOff,
            TimeOffRequestId = timeOff.Id,
            Date = DateTime.Now.AddDays(8) //date in the future
        };
        Context.TimeEntries.Add(newEntry);

        TimeOffRequest timeOff2 = new()
        {
            Status = ServerSide.Models.TimeOffRequestStatusEnum.Pending
        };
        Context.TimeOffRequests.Add(timeOff2);

        TimeEntries newEntry2 = new()
        {
            User = user,
            UserId = 1,
            MyTimeEntryTask = task,
            MyTimeEntryTaskId = 2,
            Hours = 1,
            TimeOffRequest = timeOff2,
            TimeOffRequestId = timeOff2.Id,
            Date = DateTime.Parse("03-03-03") //date in the past
        };
        Context.TimeEntries.Add(newEntry2);
        Context.SaveChanges();

        //Get time off requests for a known user
        var result = await TimeOffManager.GetUserTimeOffRequestsAsync(1);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.Count); //Verify that all 3 time off requests were returned
    }

    [Fact]
    public async void GetUserTimeOffRequests_ValidUser_NoRequests_UnsuccessfulResult()
    {
        //Arrange
        //Add a user with no time off requests
        User newUser = new()
        {
            Email = "email@email.com",
            Password = "PWtest3#",
            FirstName = "Test",
            LastName = "Test",
            IsAdmin = false
        };
        Context.Users.Add(newUser);
        Context.SaveChanges();

        //Get time off requests for the new user
        var result = await TimeOffManager.GetUserTimeOffRequestsAsync(newUser.Id);

        Assert.False(result.Success);
        Assert.Null(result.Data);
    }

    [Fact]
    public async void GetUserTimeOffRequests_InvalidUser_UnsuccessfulResult()
    {
        //Get time off requests using an invalid user id
        var result = await TimeOffManager.GetUserTimeOffRequestsAsync(0);

        Assert.False(result.Success);
        Assert.Null(result.Data);
    }

    /**
     * Tests for method: DeleteUserTimeOffRequest
     */

    [Fact]
    public async void DeleteUserTimeOffRequest_AllValidParams_SuccessfulResult()
    {
        //Arrange
        var user = Context.Users.Find(1); //get existing user
        var task = Context.MyTimeEntryTasks.Find(2); //get existing time off task

        //Add new time off request and entry
        TimeOffRequest timeOff = new()
        {
            Status = ServerSide.Models.TimeOffRequestStatusEnum.Pending
        };
        Context.TimeOffRequests.Add(timeOff);

        TimeEntries newEntry = new()
        {
            User = user,
            UserId = 1,
            MyTimeEntryTask = task,
            MyTimeEntryTaskId = 2,
            Hours = 1,
            TimeOffRequest = timeOff,
            TimeOffRequestId = timeOff.Id,
            Date = DateTime.Now.AddDays(8) //date in the future
        };
        Context.TimeEntries.Add(newEntry);
        Context.SaveChanges();

        //Make a new DeleteTimeOffRequest to pass to the method
        DeleteTimeOffRequest timeOffRequest = new()
        {
            TimeOffRequestId = 2,
            RequestedDate = DateTime.Now.AddDays(8)
        };

        //Delete the time off request
        var result = await TimeOffManager.DeleteUserTimeOffRequestAsync(timeOffRequest, 1);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(2, result.Data);
        Assert.Equal(1, Context.TimeOffRequests.Count()); //There should be only one time off request in the table
    }

    [Fact]
    public async void DeleteUserTimeOffRequest_InvalidRequestId_UnsuccessfulResult()
    {
        //Make a new DeleteTimeOffRequest to pass to the method
        DeleteTimeOffRequest timeOffRequest = new()
        {
            TimeOffRequestId = 2, //invalid, doesn't exist
            RequestedDate = DateTime.Now.AddDays(8)
        };

        //Attempt to delete the time off request for a known user
        var result = await TimeOffManager.DeleteUserTimeOffRequestAsync(timeOffRequest, 1);

        Assert.NotNull(result);
        Assert.False(result.Success);
    }

    [Fact]
    public async void DeleteUserTimeOffRequest_ValidUserAndRequestId_DateInPast_UnsuccessfulResult()
    {
        //Arrange
        var user = Context.Users.Find(1); //get existing user
        var task = Context.MyTimeEntryTasks.Find(2); //get existing time off task

        //Add new time off request and entry
        TimeOffRequest timeOff = new()
        {
            Status = ServerSide.Models.TimeOffRequestStatusEnum.Pending
        };
        Context.TimeOffRequests.Add(timeOff);

        TimeEntries newEntry = new()
        {
            User = user,
            UserId = 1,
            MyTimeEntryTask = task,
            MyTimeEntryTaskId = 2,
            Hours = 1,
            TimeOffRequest = timeOff,
            TimeOffRequestId = timeOff.Id,
            Date = DateTime.Parse("03-03-03")
        };
        Context.TimeEntries.Add(newEntry);
        Context.SaveChanges();

        //Make a new DeleteTimeOffRequest to pass to the method
        DeleteTimeOffRequest timeOffRequest = new()
        {
            TimeOffRequestId = 2,
            RequestedDate = DateTime.Parse("03-03-03") //date in the past
        };

        //Attempt to delete the time off request for a known user
        var result = await TimeOffManager.DeleteUserTimeOffRequestAsync(timeOffRequest, 1);

        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(2, Context.TimeOffRequests.Count()); //There should be two requests in the table
    }

    [Fact]
    public async void DeleteUserTimeOffRequest_ValidUserAndRequest_DateIsNow_UnsuccessfulResult()
    {
        //Arrange
        var user = Context.Users.Find(1); //get existing user
        var task = Context.MyTimeEntryTasks.Find(2); //get existing time off task

        //Add new time off request and entry
        TimeOffRequest timeOff = new()
        {
            Status = ServerSide.Models.TimeOffRequestStatusEnum.Pending
        };
        Context.TimeOffRequests.Add(timeOff);

        TimeEntries newEntry = new()
        {
            User = user,
            UserId = 1,
            MyTimeEntryTask = task,
            MyTimeEntryTaskId = 2,
            Hours = 1,
            TimeOffRequest = timeOff,
            TimeOffRequestId = timeOff.Id,
            Date = DateTime.Now
        };
        Context.TimeEntries.Add(newEntry);
        Context.SaveChanges();

        //Make a new DeleteTimeOffRequest to pass to the method
        DeleteTimeOffRequest timeOffRequest = new()
        {
            TimeOffRequestId = 2,
            RequestedDate = DateTime.Now
        };

        //Attempt to delete the time off request for a known user
        var result = await TimeOffManager.DeleteUserTimeOffRequestAsync(timeOffRequest, 1);

        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(2, Context.TimeOffRequests.Count()); //There should be two requests in the table
    }

    [Fact]
    public async void DeleteUserTimeOffRequest_ValidParams_UserNotAssociatedWithRequest_UnsuccessfulResult()
    {
        //Arrange
        var task = Context.MyTimeEntryTasks.Find(2); //get existing time off task

        //Add a new user
        User newUser = new()
        {
            Email = "email@email.com",
            Password = "PWtest3#",
            FirstName = "Test",
            LastName = "Test",
            IsAdmin = false
        };
        Context.Users.Add(newUser);

        //Add new time off request and entry
        TimeOffRequest timeOff = new()
        {
            Status = ServerSide.Models.TimeOffRequestStatusEnum.Pending
        };
        Context.TimeOffRequests.Add(timeOff);

        TimeEntries newEntry = new()
        {
            User = newUser,
            UserId = 2,
            MyTimeEntryTask = task,
            MyTimeEntryTaskId = 2,
            Hours = 1,
            TimeOffRequest = timeOff,
            TimeOffRequestId = timeOff.Id,
            Date = DateTime.Now
        };
        Context.TimeEntries.Add(newEntry);
        Context.SaveChanges();

        //Make a new DeleteTimeOffRequest to pass to the method
        DeleteTimeOffRequest timeOffRequest = new()
        {
            TimeOffRequestId = 2,
            RequestedDate = DateTime.Now
        };

        //Attempt to delete the new time off request with a user id that doesn't match
        var result = await TimeOffManager.DeleteUserTimeOffRequestAsync(timeOffRequest, 1);

        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(2, Context.TimeOffRequests.Count()); //There should be two requests in the table
    }

    [Fact]
    public async void DeleteUserTimeOffRequest_ValidParams_NoTimeEntryAssociatedWithRequest_UnsuccessfulResult()
    {
        //Arrange
        //Add new time off request
        TimeOffRequest timeOff = new()
        {
            Status = ServerSide.Models.TimeOffRequestStatusEnum.Pending
        };
        Context.TimeOffRequests.Add(timeOff);
        Context.SaveChanges();

        //Make a new DeleteTimeOffRequest to pass to the method
        DeleteTimeOffRequest timeOffRequest = new()
        {
            TimeOffRequestId = 2,
            RequestedDate = DateTime.Now
        };

        //Attempt to delete the new time off request 
        var result = await TimeOffManager.DeleteUserTimeOffRequestAsync(timeOffRequest, 1);

        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(2, Context.TimeOffRequests.Count()); //There should be two requests in the table
    }

    /**
     * Tests for method: ApproveTimeOffStatus
     */

    [Fact]
    public async void ApproveTimeOffStatus_AllValidParams_SuccessfulResultAndStatusChanged()
    {
        //Arrange
        var user = Context.Users.Find(1); //get existing user
        var task = Context.MyTimeEntryTasks.Find(2); //get existing time off task

        //Add new time off request and entry
        TimeOffRequest timeOff = new()
        {
            Status = ServerSide.Models.TimeOffRequestStatusEnum.Pending
        };
        Context.TimeOffRequests.Add(timeOff);

        TimeEntries newEntry = new()
        {
            User = user,
            UserId = 1,
            MyTimeEntryTask = task,
            MyTimeEntryTaskId = 2,
            Hours = 1,
            TimeOffRequest = timeOff,
            TimeOffRequestId = timeOff.Id,
            Date = DateTime.Now.AddDays(8)
        };
        Context.TimeEntries.Add(newEntry);
        Context.SaveChanges();

        //Make a new request to pass to the method
        ApproveTimeOffStatusRequest req = new()
        {
            RequestId = 2,
            Status = ServerSide.Models.TimeOffRequestStatusEnum.Approved
        };

        var result = await TimeOffManager.ApproveTimeOffStatus(req);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(ServerSide.Models.TimeOffRequestStatusEnum.Approved, timeOff.Status);
    }

    [Fact]
    public async void ApproveTimeOffStatus_NonexistentRequest_UnsuccessfulResult()
    {
        //Make a new request to pass to the method
        ApproveTimeOffStatusRequest req = new()
        {
            RequestId = 2,
            Status = ServerSide.Models.TimeOffRequestStatusEnum.Approved
        };

        var result = await TimeOffManager.ApproveTimeOffStatus(req);

        Assert.NotNull(result);
        Assert.False(result.Success);
    }

    [Fact]
    public async void ApproveTimeOffStatus_ValidParams_NoTimeEntryAssociatedWithRequest_UnsuccessfulResult()
    {
        //Arrange
        //Add new time off request
        TimeOffRequest timeOff = new()
        {
            Status = ServerSide.Models.TimeOffRequestStatusEnum.Pending
        };
        Context.TimeOffRequests.Add(timeOff);
        Context.SaveChanges();

        //Make a new request to pass to the method
        ApproveTimeOffStatusRequest req = new()
        {
            RequestId = 2,
            Status = ServerSide.Models.TimeOffRequestStatusEnum.Approved
        };

        var result = await TimeOffManager.ApproveTimeOffStatus(req);

        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(ServerSide.Models.TimeOffRequestStatusEnum.Pending, timeOff.Status); //The status should not be changed
    }

    /**
     * Tests for method: RejectTimeOffRequest
     */

    [Fact]
    public async void RejectTimeOffRequest_AllValidParams_NoComment_SuccessfulResultAndStatusChanged()
    {
        //Arrange
        var user = Context.Users.Find(1); //get existing user
        var task = Context.MyTimeEntryTasks.Find(2); //get existing time off task

        //Add new time off request and entry
        TimeOffRequest timeOff = new()
        {
            Status = ServerSide.Models.TimeOffRequestStatusEnum.Pending
        };
        Context.TimeOffRequests.Add(timeOff);

        TimeEntries newEntry = new()
        {
            User = user,
            UserId = 1,
            MyTimeEntryTask = task,
            MyTimeEntryTaskId = 2,
            Hours = 1,
            TimeOffRequest = timeOff,
            TimeOffRequestId = timeOff.Id,
            Date = DateTime.Now.AddDays(8)
        };
        Context.TimeEntries.Add(newEntry);
        Context.SaveChanges();

        //Make a new request to pass to the method
        RejectTimeOffStatusRequest req = new()
        {
            RequestId = 2,
            Status = ServerSide.Models.TimeOffRequestStatusEnum.Rejected
        };

        var result = await TimeOffManager.RejectTimeOffRequest(req);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(ServerSide.Models.TimeOffRequestStatusEnum.Rejected, timeOff.Status);
    }

    [Fact]
    public async void RejectTimeOffRequest_AllValidParams_WithComment_SuccessfulResult()
    {
        //Arrange
        var user = Context.Users.Find(1); //get existing user
        var task = Context.MyTimeEntryTasks.Find(2); //get existing time off task

        //Add new time off request and entry
        TimeOffRequest timeOff = new()
        {
            Status = ServerSide.Models.TimeOffRequestStatusEnum.Pending
        };
        Context.TimeOffRequests.Add(timeOff);

        TimeEntries newEntry = new()
        {
            User = user,
            UserId = 1,
            MyTimeEntryTask = task,
            MyTimeEntryTaskId = 2,
            Hours = 1,
            TimeOffRequest = timeOff,
            TimeOffRequestId = timeOff.Id,
            Date = DateTime.Now.AddDays(8)
        };
        Context.TimeEntries.Add(newEntry);
        Context.SaveChanges();

        String comment = "rejected";

        //Make a new request to pass to the method
        RejectTimeOffStatusRequest req = new()
        {
            RequestId = 2,
            Status = ServerSide.Models.TimeOffRequestStatusEnum.Rejected,
            Comment = comment
        };

        var result = await TimeOffManager.RejectTimeOffRequest(req);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(ServerSide.Models.TimeOffRequestStatusEnum.Rejected, timeOff.Status);
        Assert.Equal(comment, newEntry.Comment);
    }

    [Fact]
    public async void RejectTimeOffRequest_NonexistentRequest_UnsuccessfulResult()
    {
        //Make a new request to pass to the method
        RejectTimeOffStatusRequest req = new()
        {
            RequestId = 2,
            Status = ServerSide.Models.TimeOffRequestStatusEnum.Rejected
        };

        var result = await TimeOffManager.RejectTimeOffRequest(req);

        Assert.NotNull(result);
        Assert.False(result.Success);
    }

    [Fact]
    public async void RejectTimeOffRequest_ValidParams_NoTimeEntryAssociatedWithRequest_UnsuccessfulResult()
    {
        //Arrange
        //Add new time off request
        TimeOffRequest timeOff = new()
        {
            Status = ServerSide.Models.TimeOffRequestStatusEnum.Pending
        };
        Context.TimeOffRequests.Add(timeOff);
        Context.SaveChanges();

        //Make a new request to pass to the method
        RejectTimeOffStatusRequest req = new()
        {
            RequestId = 2,
            Status = ServerSide.Models.TimeOffRequestStatusEnum.Rejected
        };

        var result = await TimeOffManager.RejectTimeOffRequest(req);

        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(ServerSide.Models.TimeOffRequestStatusEnum.Pending, timeOff.Status); //The status should not be changed
    }
    private void PopulateTestData()
    {
        //every test needs a user, tasks, time entrie(s), time off request(s)
        User user = new User
        {
            Email = "test@email.com",
            Password = "PWtest0)",
            FirstName = "User",
            LastName = "One",
            IsAdmin = false

        };

        Context.Users.Add(user);

        var task = new MyTimeEntryTask
        {
            Name = "Work",
            IsActive = true,
            IsTimeOff = false
        };

        var task2 = new MyTimeEntryTask
        {
            Name = "TimeOff",
            IsActive = true,
            IsTimeOff = true
        };

        Context.MyTimeEntryTasks.AddRange([task, task2]);

        var timeOff = new TimeOffRequest
        {
            Status = ServerSide.Models.TimeOffRequestStatusEnum.Pending
        };

        Context.TimeOffRequests.Add(timeOff);

        var entry = new TimeEntries
        {
            User = user,
            UserId = user.Id,
            Date = DateTime.Now,
            Hours = 1,
            MyTimeEntryTask = task,
            MyTimeEntryTaskId = task.Id
        };

        var entry2 = new TimeEntries
        {
            User = user,
            UserId = user.Id,
            Date = DateTime.Now,
            Hours = 1,
            MyTimeEntryTask = task2,
            MyTimeEntryTaskId = task2.Id,
            TimeOffRequest = timeOff,
            TimeOffRequestId = timeOff.Id
        };

        Context.TimeEntries.AddRange([entry, entry2]);

        Context.SaveChanges();
    }

    /// <summary>
    /// Runs after each test
    /// </summary>
    public void Dispose()
    {
        Context.RemoveRange(Context.TimeEntries);
        Context.RemoveRange(Context.Users);
        Context.RemoveRange(Context.TimeOffRequests);
        Context.RemoveRange(Context.MyTimeEntryTasks);
        Context.SaveChanges();
    }
}

