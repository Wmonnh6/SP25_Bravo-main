using ServerSide.Core.Services.IServices;

namespace ServerSide.Core.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
