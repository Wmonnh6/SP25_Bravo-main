using ServerSide.DTOs;
using ServerSide.Models;
using ServerSide.Mapper;
using ServerSide.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace ServerSide.Managers.Calendar;

public class CalendarManager : BaseManager, ICalendarManager
{
    public CalendarManager(ILoggerFactory logger, ApplicationDbContext dbContext) : base(logger, dbContext)
    {
    }

    public async Task<ManagerResult<List<EmployeeTimeOffRequestsDTO>>> GetAllEmployeeTimeOffRequestsAsync()
    {
        var timeOffRequests = await DbContext.TimeEntries
            .Include(x => x.MyTimeEntryTask)
            .Include(x => x.User)
            .Where(x => x.MyTimeEntryTask.IsTimeOff)
            .ToListAsync();

        if (timeOffRequests == null || timeOffRequests.Count == 0)
        {
            return ManagerResult<List<EmployeeTimeOffRequestsDTO>>.Successful("No time off requests available.");
        }

        return ManagerResult<List<EmployeeTimeOffRequestsDTO>>.Successful("Time off requests retrieved successfully", [.. timeOffRequests.Select(x => x.ToCalendarViewDTO())]);
    }
}
