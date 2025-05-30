namespace ServerSide.Core.Services.IServices;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
