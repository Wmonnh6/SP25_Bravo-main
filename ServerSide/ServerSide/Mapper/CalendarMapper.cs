using ServerSide.DTOs;
using ServerSide.Models.Entities;

namespace ServerSide.Mapper;

public static class CalendarMapper
{
    public static EmployeeTimeOffRequestsDTO ToCalendarViewDTO(this TimeEntries timeEntries)
    {
        return new EmployeeTimeOffRequestsDTO
        {
            Id = timeEntries.Id,
            Name = $"{timeEntries.User.FirstName} {timeEntries.User.LastName} - ({timeEntries.Hours})",
            Date = DateOnly.FromDateTime(timeEntries.Date)
        };
    }
}
