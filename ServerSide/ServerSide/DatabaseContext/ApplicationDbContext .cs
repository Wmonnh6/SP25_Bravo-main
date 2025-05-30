using ServerSide.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ServerSide.DatabaseContext;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }

    public DbSet<User> Users { get; set; }
    public DbSet<Invitation> Invitations { get; set; }
    public DbSet<ResetPasswordRequest> ResetPasswordRequests { get; set; }
    public DbSet<MyTimeEntryTask> MyTimeEntryTasks { get; set; }
    public DbSet<TimeEntries> TimeEntries { get; set; }
    public DbSet<ClosedWeek> ClosedWeeks { get; set; }
    public DbSet<TimeOffRequest> TimeOffRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure your entity relationships, indexes, etc.
    }
}
