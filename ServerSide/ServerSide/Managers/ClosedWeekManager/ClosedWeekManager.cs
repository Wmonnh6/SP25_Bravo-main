using ServerSide.Models;
using ServerSide.Requests;
using ServerSide.DatabaseContext;
using ServerSide.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ServerSide.Managers.ClosedWeekManager;

public class ClosedWeekManager : BaseManager, IClosedWeekManager
{
    // Constructor to inject ApplicationDbContext and logger
    public ClosedWeekManager(
        ApplicationDbContext dbContext,
        ILoggerFactory logger) : base(logger, dbContext)
    {
    }

    // Method to close a week based on the first day of the week
    public async Task<ManagerResult<DateTime>> CloseWeek(ClosedWeekRequest request)
    {
        var weekDate = GetStartOfWeek(request.Date);
        // Check if the week is already closed (tracked in the table)
        var week = await DbContext.ClosedWeeks.FirstOrDefaultAsync(x => x.DateClosed == weekDate);
        if (week is not null)
        {
            return ManagerResult<DateTime>.Unsuccessful("Week is already closed.");
        }

        // Create the new closed week
        var closedWeek = new ClosedWeek { DateClosed = weekDate };

        // Close the week by adding it to the table to track it
        DbContext.ClosedWeeks.Add(closedWeek);
        await DbContext.SaveChangesAsync();

        return ManagerResult<DateTime>.Successful("Week closed successfully.", weekDate);
    }

    // Method to open a week based on the first day of the week
    public async Task<ManagerResult<DateTime>> OpenWeek(ClosedWeekRequest request)
    {
        var weekDate = GetStartOfWeek(request.Date);
        //Check if the week is closed (tracked in the table)
        var week = await DbContext.ClosedWeeks.FirstOrDefaultAsync(x => x.DateClosed == weekDate);
        if (week is null)
        {
            return ManagerResult<DateTime>.Unsuccessful("Cannot open a week that is not closed.");
        }

        //Open the week by removing it from the table
        DbContext.ClosedWeeks.Remove(week);
        await DbContext.SaveChangesAsync();

        return ManagerResult<DateTime>.Successful("Week opened successfully.", weekDate);
    }

    // Method to check if a week is closed based on the first day of the week
    public async Task<ManagerResult<bool>> CheckWeekStatus(DateTime date)
    {
        date = GetStartOfWeek(date);
        var week = await DbContext.ClosedWeeks.FirstOrDefaultAsync(x => x.DateClosed == date);
        if (week is null) //week is open
        {
            return ManagerResult<bool>.Successful("Week is open.", false);
        }

        // Week exists in the table, so it is closed
        return ManagerResult<bool>.Successful("Week is closed.", true);
    }

    // (method taken from time entry manager to get the start of the week)
    public static DateTime GetStartOfWeek(DateTime date, DayOfWeek startOfWeek = DayOfWeek.Sunday)
    {
        int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
        return date.AddDays(-diff).Date;
    }
}
