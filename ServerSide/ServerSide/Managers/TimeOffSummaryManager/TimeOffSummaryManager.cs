using ServerSide.DTOs;
using ServerSide.Models;
using ServerSide.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace ServerSide.Managers.TimeOffSummaryManager;

public class TimeOffSummaryManager : BaseManager, ITimeOffSummaryManager
{
    public TimeOffSummaryManager(
        ApplicationDbContext dbContext,
        ILoggerFactory logger) : base(logger, dbContext)
    {
    }

    public async Task<ManagerResult<List<TimeOffSummaryDTO>>> GetAllEmployeeTimeOffRequestsAsync(DateTime month)
    {
        var startOfMonth = new DateTime(month.Year, month.Month, 1);

        var endOfMonth = new DateTime(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month));

        var timeEntries = await DbContext.TimeEntries
            .Include(te => te.MyTimeEntryTask) // Include the related Task entity
            .Include(te => te.User)
            .Where(te => te.MyTimeEntryTask.IsTimeOff && te.Date >= startOfMonth && te.Date <= endOfMonth) // Filter by tasks marked as time off
            .ToListAsync();

        if (timeEntries == null || !timeEntries.Any())
        {
            return ManagerResult<List<TimeOffSummaryDTO>>.Unsuccessful("No time-off entries found.");
        }

        // Group by User and calculate total time-off hours
        var timeOffSummaries = timeEntries
            .GroupBy(te => te.User)
            .Select(group => new TimeOffSummaryDTO(
                group.Key.Id,
                $"{group.Key.FirstName} {group.Key.LastName}", // Concatenating First & Last Name
                group.Sum(te => te.Hours))) // Summing the logged hours
            .ToList();

        return ManagerResult<List<TimeOffSummaryDTO>>.Successful("Time-off summaries retrieved successfully", timeOffSummaries);
    }
}
