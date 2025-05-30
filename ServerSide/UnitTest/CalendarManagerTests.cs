using ServerSide.Models;
using ServerSide.DatabaseContext;
using ServerSide.Models.Entities;
using ServerSide.Managers.Calendar;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace UnitTest
{
    public class CalendarManagerTests : IClassFixture<TestSetup>, IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly CalendarManager _calendarManager;

        public CalendarManagerTests(TestSetup setup)
        {
            _context = new(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "CalendarManagerTests")
                .Options
            );
            var loggerFactory = setup.GetService<ILoggerFactory>();
            _calendarManager = new CalendarManager(loggerFactory, _context);

            // Ensure clean database state before each test
            _context.Database.EnsureDeleted(); // Ensures the database is deleted before the test runs
            _context.Database.EnsureCreated();

            // Ensure clean database state before each test
            Dispose();
        }

        public void Dispose()
        {
            // Clear data after each test to avoid side effects between tests.
            _context.TimeEntries.RemoveRange(_context.TimeEntries);
            _context.MyTimeEntryTasks.RemoveRange(_context.MyTimeEntryTasks);
            _context.Users.RemoveRange(_context.Users);
            _context.TimeOffRequests.RemoveRange(_context.TimeOffRequests);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllEmployeeTimeOffRequestsAsync_ReturnsTimeOffRequests_WhenTimeOffEntriesExist()
        {
            // Arrange
            var user = new User
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane.doe@example.com",
                Password = "securepass",
                IsAdmin = false
            };
            _context.Users.Add(user);

            var task = new MyTimeEntryTask
            {
                Name = "Vacation",
                IsActive = true,
                IsTimeOff = true
            };
            _context.MyTimeEntryTasks.Add(task);

            var request = new TimeOffRequest
            {
                Status = TimeOffRequestStatusEnum.Pending
            };
            _context.TimeOffRequests.Add(request);

            var timeEntry = new TimeEntries
            {
                User = user,
                MyTimeEntryTask = task,
                Hours = 8,
                Date = DateTime.Today,
                Comment = "Vacation day",
                TimeOffRequest = request
            };
            _context.TimeEntries.Add(timeEntry);

            await _context.SaveChangesAsync();

            // Act
            var result = await _calendarManager.GetAllEmployeeTimeOffRequestsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);

            var firstTimeOffEntry = result.Data.First();

            Assert.Equal(timeEntry.Id, firstTimeOffEntry.Id);
            Assert.Equal("Jane Doe - (8)", firstTimeOffEntry.Name);
            Assert.Equal(DateOnly.FromDateTime(timeEntry.Date), firstTimeOffEntry.Date);
        }

        [Fact]
        public async Task GetAllEmployeeTimeOffRequestsAsync_ReturnsEmptyList_WhenNoEntriesExist()
        {
            // Act
            var result = await _calendarManager.GetAllEmployeeTimeOffRequestsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Null(result.Data);
            Assert.Equal("No time off requests available.", result.Message);
        }

        [Fact]
        public async Task GetAllEmployeeTimeOffRequestsAsync_IgnoresNonTimeOffTasks()
        {
            // Arrange
            var user = new User
            {
                FirstName = "NonTimeOff",
                LastName = "User",
                Email = "nontimeoff@example.com",
                Password = "securepass",
                IsAdmin = false
            };
            _context.Users.Add(user);

            var nonTimeOffTask = new MyTimeEntryTask
            {
                Name = "Regular Work",
                IsActive = true,
                IsTimeOff = false // Important: should not be returned
            };
            _context.MyTimeEntryTasks.Add(nonTimeOffTask);

            var timeEntry = new TimeEntries
            {
                User = user,
                MyTimeEntryTask = nonTimeOffTask,
                Hours = 5,
                Date = DateTime.Today,
                Comment = "Just work"
            };
            _context.TimeEntries.Add(timeEntry);

            await _context.SaveChangesAsync();

            // Act
            var result = await _calendarManager.GetAllEmployeeTimeOffRequestsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Null(result.Data); // Should skip entries with IsTimeOff = false
        }

        [Fact]
        public async Task GetAllEmployeeTimeOffRequestsAsync_CanHandleMultipleEntries()
        {
            // Arrange
            var user = new User
            {
                FirstName = "Multiple",
                LastName = "Entries",
                Email = "multi@example.com",
                Password = "securepass",
                IsAdmin = false
            };
            _context.Users.Add(user);

            var timeOffTask = new MyTimeEntryTask
            {
                Name = "Sick Leave",
                IsActive = true,
                IsTimeOff = true
            };
            _context.MyTimeEntryTasks.Add(timeOffTask);

            var entry1 = new TimeEntries
            {
                User = user,
                MyTimeEntryTask = timeOffTask,
                Hours = 4,
                Date = DateTime.Today,
                Comment = "Morning sick leave"
            };

            var entry2 = new TimeEntries
            {
                User = user,
                MyTimeEntryTask = timeOffTask,
                Hours = 4,
                Date = DateTime.Today,
                Comment = "Afternoon sick leave"
            };

            _context.TimeEntries.AddRange(entry1, entry2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _calendarManager.GetAllEmployeeTimeOffRequestsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data.Count);
        }
    }
}