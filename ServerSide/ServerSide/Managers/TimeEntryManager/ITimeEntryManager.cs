using ServerSide.DTOs;
using ServerSide.Models;
using ServerSide.Requests;

namespace ServerSide.Managers.TimeEntryManager;

public interface ITimeEntryManager
{
    Task<ManagerResult<List<TimeEntryDTO>>> GetUserTimeEntriesAsync(int userId, DateTime date);
    Task<ManagerResult<TimeEntryDTO>> AddTimeEntryAsync(TimeEntryRequest request);
    Task<ManagerResult<int>> DeleteTimeEntryAsync(DeleteTimeEntryRequest request, int currentUserId, bool isAdmin);
	Task<ManagerResult<TimeEntryDTO>> UpdateTimeEntryAsync(TimeEntryRequest request);
    Task<ManagerResult<int>> DeleteAnyTimeEntryAsync(DeleteTimeEntryRequest request);
}
