using ServerSide.DTOs;
using ServerSide.Models.Entities;

namespace ServerSide.Mapper;

public static class TimeEntryMapper
{
    public static TimeEntryDTO ToDTO(this TimeEntries timeEntry)
    {
        return new TimeEntryDTO
        {
            Id = timeEntry.Id,
            Date = timeEntry.Date,
            Hours = timeEntry.Hours,
            User = timeEntry.User.MapUserToUserDTO(),
            Task = timeEntry.MyTimeEntryTask.ToDTO(),
            Comment = timeEntry.Comment,
            TimeOffRequest = timeEntry.TimeOffRequest == null ? null : new()
            {
                Id = timeEntry.TimeOffRequest.Id,
                Status = timeEntry.TimeOffRequest.Status.ToString(),
            }
        };
    }

    public static TimeEntryDTO ToTimeOffRequestDTO(this TimeEntries timeEntryDTO)
    {
        return new TimeEntryDTO
        {
            Id = timeEntryDTO.Id,
            Date = timeEntryDTO.Date,
            Hours = timeEntryDTO.Hours,
            Comment = timeEntryDTO.Comment,
            Task = timeEntryDTO.MyTimeEntryTask.ToDTO(),
            User = timeEntryDTO.User.MapUserToUserDTO(),
            TimeOffRequest = timeEntryDTO.TimeOffRequest == null ? null : new()
            {
                Id = timeEntryDTO.TimeOffRequest.Id,
                Status = timeEntryDTO.TimeOffRequest.Status.ToString(),
            }
        };
    }
}
