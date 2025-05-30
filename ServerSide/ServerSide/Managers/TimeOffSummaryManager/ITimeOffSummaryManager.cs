using ServerSide.DTOs;
using ServerSide.Models;

namespace ServerSide.Managers.TimeOffSummaryManager;

public interface ITimeOffSummaryManager
{
    Task<ManagerResult<List<TimeOffSummaryDTO>>> GetAllEmployeeTimeOffRequestsAsync(DateTime month);
}