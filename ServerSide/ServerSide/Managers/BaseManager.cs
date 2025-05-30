using ServerSide.DatabaseContext;

namespace ServerSide.Managers;

public abstract class BaseManager
{
    protected ILogger _logger;
    protected ApplicationDbContext DbContext;

    protected BaseManager(ILoggerFactory logger, ApplicationDbContext dbContext)
    {
        _logger = logger.CreateLogger(GetType());
        DbContext = dbContext;
    }
}
