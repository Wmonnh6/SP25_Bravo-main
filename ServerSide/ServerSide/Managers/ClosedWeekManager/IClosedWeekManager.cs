using ServerSide.Models;
using ServerSide.Requests;

namespace ServerSide.Managers.ClosedWeekManager;

public interface IClosedWeekManager
{
    Task<ManagerResult<DateTime>> CloseWeek(ClosedWeekRequest request);

    Task<ManagerResult<DateTime>> OpenWeek(ClosedWeekRequest request);

    Task<ManagerResult<bool>> CheckWeekStatus(DateTime date);

}
