using ServerSide.DTOs;
using ServerSide.Models.Entities;

namespace ServerSide.Mapper;

public static class MyTimeEntryTaskMapper
{
    public static MyTimeEntryTaskDTO ToDTO(this MyTimeEntryTask myTimeEntryTask)
    {
        return new MyTimeEntryTaskDTO
        {
            Id = myTimeEntryTask.Id,
            Name = myTimeEntryTask.Name,
            IsActive = myTimeEntryTask.IsActive,
            IsTimeOff = myTimeEntryTask.IsTimeOff
        };
    }
}
