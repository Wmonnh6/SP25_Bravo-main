using ServerSide.DTOs;
using ServerSide.Models;

namespace ServerSide.Managers.Calendar;

public interface ICalendarManager
{
    Task<ManagerResult<List<EmployeeTimeOffRequestsDTO>>> GetAllEmployeeTimeOffRequestsAsync();
}
