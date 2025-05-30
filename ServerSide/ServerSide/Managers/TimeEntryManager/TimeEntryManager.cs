using ServerSide.DTOs;
using ServerSide.Mapper;
using ServerSide.Models;
using ServerSide.Requests;
using ServerSide.DatabaseContext;
using ServerSide.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ServerSide.Managers.TimeEntryManager;

public class TimeEntryManager : BaseManager, ITimeEntryManager
{
    // Constructor to inject ApplicationDbContext and logger
    public TimeEntryManager(
        ApplicationDbContext dbContext,
        ILoggerFactory logger) : base(logger, dbContext)
    {
    }

    // Method to retrieve all time entries for a user
    public async Task<ManagerResult<List<TimeEntryDTO>>> GetUserTimeEntriesAsync(int userId, DateTime date)
    {
        var startOfWeek = GetStartOfWeek(date);

        DateTime endOfWeek = startOfWeek.AddDays(7);

        var timeEntries = await DbContext.TimeEntries
            .Include(te => te.User)
            .Include(te => te.MyTimeEntryTask)
            .Include(te => te.TimeOffRequest)
            .Where(te => te.UserId == userId && te.Date >= startOfWeek && te.Date <= endOfWeek)
            .OrderBy(te => te.Date)
            .ToListAsync();

        if (timeEntries == null || timeEntries.Count == 0)
        {
            return ManagerResult<List<TimeEntryDTO>>.Unsuccessful("No time entries found for this user.");
        }

        return ManagerResult<List<TimeEntryDTO>>.Successful("Time entries retrieved successfully.", timeEntries.Select(x => x.ToDTO()).ToList());
    }

    // Method to add a new time entry
    public async Task<ManagerResult<TimeEntryDTO>> AddTimeEntryAsync(TimeEntryRequest request)
    {
        //Verify that the week is open for edits
        var startOfWeek = GetStartOfWeek(request.Date);
        var week = await DbContext.ClosedWeeks.FirstOrDefaultAsync(x => x.DateClosed == startOfWeek);
        if (week is not null)
        {
            return ManagerResult<TimeEntryDTO>.Unsuccessful("Selected week is closed.");
        }

        if (request == null || request.Hours <= 0)
        {
            return ManagerResult<TimeEntryDTO>.Unsuccessful("Invalid time entry data.");
        }

        // Fetch related entities from the database
        var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
        if (user == null)
        {
            return ManagerResult<TimeEntryDTO>.Unsuccessful("User not found.");
        }

        var task = await DbContext.MyTimeEntryTasks.FirstOrDefaultAsync(t => t.Id == request.TaskId);
        if (task == null)
        {
            return ManagerResult<TimeEntryDTO>.Unsuccessful("Task not found.");
        }

        var newTimeEntry = new TimeEntries
        {
            UserId = request.UserId,
            User = user,
            Comment = request.Comment,
            Hours = request.Hours,
            MyTimeEntryTaskId = request.TaskId,
            MyTimeEntryTask = task,
            Date = request.Date
        };

        if (task.IsTimeOff)
        {
            newTimeEntry.TimeOffRequest = new()
            {
                Status = TimeOffRequestStatusEnum.Pending,
            };
        }

        DbContext.TimeEntries.Add(newTimeEntry);
        await DbContext.SaveChangesAsync();

        return ManagerResult<TimeEntryDTO>.Successful("Time entry added successfully.", newTimeEntry.ToDTO());
    }

    /// <summary>
    /// This method updates the time entry in the database
    /// </summary>
    /// <param name="request">The UpdateTimeEntryRequest object that contains the data to update the time entry</param>
    /// <returns>Returns unsuccessful if the time entry can't be found. Returns sucessful if the time entry is updated successfully.</returns>
	public async Task<ManagerResult<TimeEntryDTO>> UpdateTimeEntryAsync(TimeEntryRequest request)
    {
        //Verify that the week is open for edits
        var startOfWeek = GetStartOfWeek(request.Date);
        var week = await DbContext.ClosedWeeks.FirstOrDefaultAsync(x => x.DateClosed == startOfWeek);
        if (week is not null)
        {
            return ManagerResult<TimeEntryDTO>.Unsuccessful("Selected week is closed.");
        }

        if (request == null || request.Hours <= 0)
        {
            return ManagerResult<TimeEntryDTO>.Unsuccessful("Invalid time entry data.");
        }

        // Fetch related entities from the database
        var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
        if (user == null)
        {
            return ManagerResult<TimeEntryDTO>.Unsuccessful("User not found.");
        }

        var task = await DbContext.MyTimeEntryTasks.FirstOrDefaultAsync(t => t.Id == request.TaskId);
        if (task == null)
        {
            return ManagerResult<TimeEntryDTO>.Unsuccessful("Task not found.");
        }

        var timeEntry = DbContext.TimeEntries.FirstOrDefault(entry => entry.Id == request.Id);
        if (timeEntry == null)
        {
            _logger.LogError("Unable to find time entry with ID: {0}", request.Id);
            return ManagerResult<TimeEntryDTO>.Unsuccessful("Couldn't find the time entry.");
        }

        // This check prevents switching a time entry to a completely different user, intentionally or not
        if (timeEntry.UserId != request.UserId)
        {
            _logger.LogError("UserId: {0} didn't match UserId: {1} from the request", timeEntry.UserId, request.UserId);
            return ManagerResult<TimeEntryDTO>.Unsuccessful("Malformed request.");
        }

        // The user id and task id should not be changed during update
        // The Comment, Date, and Hours, should be updated.
        timeEntry.MyTimeEntryTask = task;
        timeEntry.MyTimeEntryTaskId = request.TaskId;
        timeEntry.Hours = request.Hours;
        timeEntry.Comment = request.Comment;
        timeEntry.Date = request.Date;

        DbContext.TimeEntries.Update(timeEntry);
        await DbContext.SaveChangesAsync();

        return ManagerResult<TimeEntryDTO>.Successful("Time entry updated successfully.", timeEntry.ToDTO());
    }

    // Method to delete a time entry
    public async Task<ManagerResult<int>> DeleteTimeEntryAsync(DeleteTimeEntryRequest request, int currentUserId, bool isAdmin)
    {
        var timeEntry = await DbContext.TimeEntries.FirstOrDefaultAsync(te => te.Id == request.TimeEntryId);
        if (timeEntry == null)
        {
            _logger.LogError("Couldn't find the time entry with ID: {TimeEntryId}", request.TimeEntryId);
            return ManagerResult<int>.Unsuccessful("Couldn't find the time entry.");
        }

        //Verify that the week is open for edits
        var startOfWeek = GetStartOfWeek(timeEntry.Date);
        var week = await DbContext.ClosedWeeks.FirstOrDefaultAsync(x => x.DateClosed == startOfWeek);
        if (week is not null)
        {
            return ManagerResult<int>.Unsuccessful("Week is closed.");
        }

        if (timeEntry.UserId != currentUserId && !isAdmin)
        {
            return ManagerResult<int>.Unsuccessful("You can only delete your own time entries.");
        }

        DbContext.TimeEntries.Remove(timeEntry);
        await DbContext.SaveChangesAsync();

        return ManagerResult<int>.Successful("Time entry deleted successfully.", timeEntry.Id);
    }

    // Method for admins to delete any time entry
    public async Task<ManagerResult<int>> DeleteAnyTimeEntryAsync(DeleteTimeEntryRequest request)
    {
        var timeEntry = await DbContext.TimeEntries.FirstOrDefaultAsync(te => te.Id == request.TimeEntryId);
        if (timeEntry == null)
        {
            _logger.LogError("Couldn't find the time entry with ID: {TimeEntryId}", request.TimeEntryId);
            return ManagerResult<int>.Unsuccessful("Couldn't find the time entry.");
        }

		// Verify that the week is open for edits
		var startOfWeek = GetStartOfWeek(timeEntry.Date);
		var week = await DbContext.ClosedWeeks.FirstOrDefaultAsync(x => x.DateClosed == startOfWeek);
		if (week is not null)
		{
			return ManagerResult<int>.Unsuccessful("Week is closed.");
		}

		// No need to check ownership - admin can delete any entry
		DbContext.TimeEntries.Remove(timeEntry);
        await DbContext.SaveChangesAsync();

        return ManagerResult<int>.Successful("Time entry deleted successfully.", timeEntry.Id);
    }
    public static DateTime GetStartOfWeek(DateTime date, DayOfWeek startOfWeek = DayOfWeek.Sunday)
    {
        int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
        return date.AddDays(-diff).Date;
    }
}
