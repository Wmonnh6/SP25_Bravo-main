using ServerSide.Requests;
using ServerSide.DatabaseContext;
using ServerSide.Models.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ServerSide.Managers.ClosedWeekManager;

namespace UnitTest
{
    public class ClosedWeekManagerTests : IClassFixture<TestSetup>, IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ClosedWeekManager _closedWeekManager;

        public ClosedWeekManagerTests(TestSetup setup)
        {
            _context = new(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "ClosedWeekManagerTests")
                .Options
            );
            var loggerFactory = setup.GetService<ILoggerFactory>();
            _closedWeekManager = new ClosedWeekManager(_context, loggerFactory);

            Dispose();
        }

        // Dispose the in-memory database after each test
        public void Dispose()
        {
            _context.ClosedWeeks.RemoveRange(_context.ClosedWeeks);
            _context.SaveChanges();
        }

        [Fact]
        public async Task CloseWeek_ShouldCloseWeek_WhenWeekIsNotClosed()
        {
            // Arrange
            var request = new ClosedWeekRequest { Date = new DateTime(2025, 3, 30) };

            // Act
            var result = await _closedWeekManager.CloseWeek(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Week closed successfully.", result.Message);
            Assert.Equal(request.Date.Date, result.Data); // Assert that the week was closed on the correct date.
        }

        [Fact]
        public async Task CloseWeek_ShouldReturnError_WhenWeekIsAlreadyClosed()
        {
            // Arrange
            var closedDate = new DateTime(2025, 3, 30);
            _context.ClosedWeeks.Add(new ClosedWeek { DateClosed = closedDate });
            await _context.SaveChangesAsync();

            var request = new ClosedWeekRequest { Date = closedDate };

            // Act
            var result = await _closedWeekManager.CloseWeek(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Week is already closed.", result.Message);
        }

        [Fact]
        public async Task OpenWeek_ShouldOpenWeek_WhenWeekIsClosed()
        {
            // Arrange
            var closedDate = new DateTime(2025, 3, 30);
            _context.ClosedWeeks.Add(new ClosedWeek { DateClosed = closedDate });
            await _context.SaveChangesAsync();

            var request = new ClosedWeekRequest { Date = closedDate };

            // Act
            var result = await _closedWeekManager.OpenWeek(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Week opened successfully.", result.Message);
        }

        [Fact]
        public async Task OpenWeek_ShouldReturnError_WhenWeekIsNotClosed()
        {
            // Arrange
            var openDate = new DateTime(2025, 4, 3);
            var request = new ClosedWeekRequest { Date = openDate };

            // Act
            var result = await _closedWeekManager.OpenWeek(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Cannot open a week that is not closed.", result.Message);
        }

        [Fact]
        public async Task CheckWeekStatus_ShouldReturnClosed_WhenWeekIsClosed()
        {
            // Arrange
            var closedDate = new DateTime(2025, 3, 30);
            _context.ClosedWeeks.Add(new ClosedWeek { DateClosed = closedDate });
            await _context.SaveChangesAsync();

            var request = new ClosedWeekRequest { Date = closedDate };

            // Act
            var result = await _closedWeekManager.CheckWeekStatus(closedDate);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Week is closed.", result.Message);
            Assert.True(result.Data); // The week is closed.
        }

        [Fact]
        public async Task CheckWeekStatus_ShouldReturnOpen_WhenWeekIsOpen()
        {
            // Arrange
            var openDate = new DateTime(2025, 4, 3);

            // Act
            var result = await _closedWeekManager.CheckWeekStatus(openDate);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Week is open.", result.Message);
            Assert.False(result.Data); // The week is open.
        }

        // Edge case: Test with boundary date (start of the week)
        [Fact]
        public async Task CloseWeek_ShouldHandleBoundaryConditions()
        {
            // Arrange
            var startOfWeekDate = new DateTime(2025, 3, 30); 
            var request = new ClosedWeekRequest { Date = startOfWeekDate };

            // Act
            var result = await _closedWeekManager.CloseWeek(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Week closed successfully.", result.Message);
            Assert.Equal(startOfWeekDate.Date, result.Data); // Ensure it is closed on the correct date
        }

        [Fact]
        public async Task CloseWeek_ShouldHandleEdgeCases_WhenDateIsBoundary()
        {
            var boundaryDate = new DateTime(2025, 4, 5); 
            var request = new ClosedWeekRequest { Date = boundaryDate };
            var result = await _closedWeekManager.CloseWeek(request);
            Assert.True(result.Success);
            Assert.Equal("Week closed successfully.", result.Message);
            Assert.Equal(new DateTime(2025, 3, 30), result.Data); 
        }

        [Fact]
        public async Task OpenWeek_ShouldReturnError_WhenNoWeekIsClosed()
        {
            var openDate = new DateTime(2025, 4, 3); 
            var request = new ClosedWeekRequest { Date = openDate };
            var result = await _closedWeekManager.OpenWeek(request);
            Assert.False(result.Success);
            Assert.Equal("Cannot open a week that is not closed.", result.Message);
        }

        [Fact]
        public async Task CheckWeekStatus_ShouldHandleEdgeCases_WhenDateIsFirstOrLastOfWeek()
        {
            var firstDayOfWeek = new DateTime(2025, 3, 30); 
            var lastDayOfWeek = new DateTime(2025, 4, 5); 

            var request = new ClosedWeekRequest { Date = firstDayOfWeek };
            await _closedWeekManager.CloseWeek(request);

            var firstResult = await _closedWeekManager.CheckWeekStatus(firstDayOfWeek);
            Assert.True(firstResult.Success);
            Assert.Equal("Week is closed.", firstResult.Message);
            Assert.True(firstResult.Data);

            var lastResult = await _closedWeekManager.CheckWeekStatus(lastDayOfWeek);
            Assert.True(lastResult.Success);
            Assert.Equal("Week is closed.", lastResult.Message);
            Assert.True(lastResult.Data);
        }
    }
}