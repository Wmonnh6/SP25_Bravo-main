using ServerSide.DTOs;
using ServerSide.Models;
using ServerSide.Requests;

namespace ServerSide.Managers.TimeOffManager;

public interface ITimeOffManager
{
    Task<ManagerResult<List<TimeEntryDTO>>> GetAllTimeOffRequests(GetTimeOffRequestsRequest request);
    Task<ManagerResult<List<TimeEntryDTO>>> GetUserTimeOffRequestsAsync(int userId);
    Task<ManagerResult<int>> DeleteUserTimeOffRequestAsync(DeleteTimeOffRequest request, int currentUserId);
    Task<ManagerResult<string>> ApproveTimeOffStatus(ApproveTimeOffStatusRequest request);
    Task<ManagerResult<string>> RejectTimeOffRequest(RejectTimeOffStatusRequest request);
}
