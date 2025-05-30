using ServerSide.DatabaseContext;
using ServerSide.Models.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ServerSide.Managers.TimeOffSummaryManager;

namespace UnitTest
{
    public class TimeOffSummaryManagerTests : IClassFixture<TestSetup>, IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeOffSummaryManager _summaryManager;

        public TimeOffSummaryManagerTests(TestSetup setup)
        {
            _context = new(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TimeOffSummaryManagerTests")
                .Options
            );
            var loggerFactory = setup.GetService<ILoggerFactory>();
            _summaryManager = new TimeOffSummaryManager(_context, loggerFactory);


            // Ensure clean database state before each test
            _context.Database.EnsureDeleted(); // Ensures the database is deleted before the test runs
            _context.Database.EnsureCreated();

            Dispose();
        }

        // This method will be called after each test runs to clear the data.
        public void Dispose()
        {
        // Clear data after each test to avoid side effects between tests.
        _context.TimeEntries.RemoveRange(_context.TimeEntries);
        _context.MyTimeEntryTasks.RemoveRange(_context.MyTimeEntryTasks);
        _context.Users.RemoveRange(_context.Users);
        _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllEmployeeTimeOffRequestsAsync_ReturnsSummaries_WhenEntriesExist()
        {
            // Arrange
            var user = new User { FirstName = "Alice", LastName = "Smith", Email = "alice@example.com", Password = "pass" };
            _context.Users.Add(user);

            var task = new MyTimeEntryTask { Name = "Vacation", IsActive = true, IsTimeOff = true };
            _context.MyTimeEntryTasks.Add(task);

            var entry = new TimeEntries
            {
                User = user,
                MyTimeEntryTask = task,
                Hours = 8,
                Date = new DateTime(2025, 4, 1),
                Comment = "Time off"
            };
            _context.TimeEntries.Add(entry);

            await _context.SaveChangesAsync();

            // Act
            var result = await _summaryManager.GetAllEmployeeTimeOffRequestsAsync(new DateTime(2025, 4, 1));

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data);
            Assert.Equal("Alice Smith", result.Data.First().UserName);
            Assert.Equal(8, result.Data.First().TotalHours);
        }

        [Fact]
        public async Task GetAllEmployeeTimeOffRequestsAsync_ReturnsUnsuccessful_WhenNoEntriesExist()
        {
            // Act
            var result = await _summaryManager.GetAllEmployeeTimeOffRequestsAsync(new DateTime(2025, 3, 1));

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal("No time-off entries found.", result.Message);
        }

        [Fact]
        public async Task GetAllEmployeeTimeOffRequestsAsync_ExcludesNonTimeOffTasks()
        {
            // Arrange
            var user = new User { FirstName = "Bob", LastName = "Brown", Email = "bob@example.com", Password = "pass" };
            _context.Users.Add(user);

            var regularTask = new MyTimeEntryTask { Name = "Work", IsActive = true, IsTimeOff = false };
            _context.MyTimeEntryTasks.Add(regularTask);

            var entry = new TimeEntries
            {
                User = user,
                MyTimeEntryTask = regularTask,
                Hours = 6,
                Date = new DateTime(2025, 4, 15),
                Comment = "Working"
            };
            _context.TimeEntries.Add(entry);

            await _context.SaveChangesAsync();

            // Act
            var result = await _summaryManager.GetAllEmployeeTimeOffRequestsAsync(new DateTime(2025, 4, 1));

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetAllEmployeeTimeOffRequestsAsync_HandlesMultipleUsers()
        {
            // Arrange
            var user1 = new User { FirstName = "User", LastName = "One", Email = "user1@example.com", Password = "pass" };
            var user2 = new User { FirstName = "User", LastName = "Two", Email = "user2@example.com", Password = "pass" };
            _context.Users.AddRange(user1, user2);

            var task = new MyTimeEntryTask { Name = "Holiday", IsActive = true, IsTimeOff = true };
            _context.MyTimeEntryTasks.Add(task);

            _context.TimeEntries.AddRange(
                new TimeEntries { User = user1, MyTimeEntryTask = task, Hours = 5, Date = new DateTime(2025, 4, 5) },
                new TimeEntries { User = user2, MyTimeEntryTask = task, Hours = 3, Date = new DateTime(2025, 4, 6) }
            );

            await _context.SaveChangesAsync();

            // Act
            var result = await _summaryManager.GetAllEmployeeTimeOffRequestsAsync(new DateTime(2025, 4, 1));

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetAllEmployeeTimeOffRequestsAsync_IncludesBoundaryDates()
        {
            // Arrange
            var user = new User { FirstName = "Edge", LastName = "Case", Email = "edge@example.com", Password = "pass" };
            _context.Users.Add(user);

            var task = new MyTimeEntryTask { Name = "Leave", IsActive = true, IsTimeOff = true };
            _context.MyTimeEntryTasks.Add(task);

            _context.TimeEntries.AddRange(
                new TimeEntries { User = user, MyTimeEntryTask = task, Hours = 4, Date = new DateTime(2025, 4, 1) },
                new TimeEntries { User = user, MyTimeEntryTask = task, Hours = 4, Date = new DateTime(2025, 5, 30) }
            );

            await _context.SaveChangesAsync();

            // Act
            var result = await _summaryManager.GetAllEmployeeTimeOffRequestsAsync(new DateTime(2025, 4, 1)); // Gets only time off requests for April (1)

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data);
            Assert.Equal(4, result.Data.First().TotalHours);
        }
    }
}