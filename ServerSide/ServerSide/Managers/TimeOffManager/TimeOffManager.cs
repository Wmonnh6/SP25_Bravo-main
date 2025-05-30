using ServerSide.DTOs;
using System.Net.Mail;
using ServerSide.Mapper;
using ServerSide.Models;
using ServerSide.Requests;
using ServerSide.DatabaseContext;
using ServerSide.Models.Entities;
using Microsoft.EntityFrameworkCore;
using ServerSide.Core.Services.IServices;

namespace ServerSide.Managers.TimeOffManager;

public class TimeOffManager : BaseManager, ITimeOffManager
{
    private readonly IEmailService _emailService;
    public TimeOffManager(ILoggerFactory logger, ApplicationDbContext dbContext, IEmailService emailService) : base(logger, dbContext)
    {
        _emailService = emailService;
    }

    public async Task<ManagerResult<List<TimeEntryDTO>>> GetAllTimeOffRequests(GetTimeOffRequestsRequest request)
    {
        var statusFilter = Enum.TryParse<TimeOffRequestStatusEnum>(request.RequestStatus, true, out var parsedStatus) ? (TimeOffRequestStatusEnum?)parsedStatus : null;

        var timeOffRequests = await DbContext.TimeEntries
            .Include(x => x.User)
            .Include(x => x.TimeOffRequest)
            .Include(x => x.MyTimeEntryTask)
            .Where(x => x.MyTimeEntryTask.IsTimeOff
                        && x.TimeOffRequest != null
                        && (statusFilter == null || x.TimeOffRequest.Status == statusFilter)
                        && (request.UserId == null || x.UserId == request.UserId)
                        && (request.StartDate != null && request.EndDate != null
                            ? x.Date >= request.StartDate && x.Date <= request.EndDate
                            : true))
            .ToListAsync();

        return ManagerResult<List<TimeEntryDTO>>.Successful("See the results.", timeOffRequests.Select(x => x.ToTimeOffRequestDTO()).ToList());
    }

    // Method to retrieve all time off requests for a user
    public async Task<ManagerResult<List<TimeEntryDTO>>> GetUserTimeOffRequestsAsync(int userId)
    {
        // get the time entries for the current user in a list
        var timeOffRequests = await DbContext.TimeEntries
            .Include(te => te.User)
            .Include(te => te.MyTimeEntryTask)
            .Include(te => te.TimeOffRequest)
            .Where(te => te.UserId == userId 
                        && te.MyTimeEntryTask.IsTimeOff
                        && te.TimeOffRequest != null)
            .OrderByDescending(te => te.Date)
            .ToListAsync();

        if (timeOffRequests == null || timeOffRequests.Count == 0)
        {
            return ManagerResult<List<TimeEntryDTO>>.Unsuccessful("No time off requests found for this user.");
        }

        return ManagerResult<List<TimeEntryDTO>>.Successful("Time off requests retrieved successfully.", timeOffRequests.Select(x => x.ToDTO()).ToList());
    }

    // Method to delete a time off request 
    public async Task<ManagerResult<int>> DeleteUserTimeOffRequestAsync(DeleteTimeOffRequest request, int currentUserId)
    {
        // get the time off request from the table by the id
        var timeOffRequest = await DbContext.TimeOffRequests.FirstOrDefaultAsync(x => x.Id == request.TimeOffRequestId);
        if (timeOffRequest == null) 
        {
            return ManagerResult<int>.Unsuccessful("Couldn't find the time off request.");
        }

        // Make sure the requested time hasn't passed by comparing it to the current date
        DateTime now = DateTime.Now;
        var requestedDate = request.RequestedDate;
        if (requestedDate < now)
        {
            return ManagerResult<int>.Unsuccessful("Cannot delete past time off requests.");
        }

        // Validate that the user is trying to delete their own request
        var timeEntry = await DbContext.TimeEntries.FirstOrDefaultAsync(x => x.TimeOffRequestId == request.TimeOffRequestId);
        if (timeEntry.UserId != currentUserId) 
        {
            return ManagerResult<int>.Unsuccessful("You can only delete your own time off requests.");
        }

        // Remove the time off request from the table
        DbContext.TimeEntries.Remove(timeEntry);
        DbContext.TimeOffRequests.Remove(timeOffRequest);
        await DbContext.SaveChangesAsync();

        return ManagerResult<int>.Successful("Time off request deleted successfully.", timeOffRequest.Id);
    }

    public async Task<ManagerResult<string>> ApproveTimeOffStatus(ApproveTimeOffStatusRequest request)
    {
        var req = await DbContext.TimeOffRequests
        .FirstOrDefaultAsync(x => x.Id == request.RequestId);

        if (req == null)
        {
            return ManagerResult<string>.Unsuccessful("Time off request not found.");
        }

        // Find the associated TimeEntry
        var timeEntry = await DbContext.TimeEntries
            .Include(x => x.User) // Load User data
            .FirstOrDefaultAsync(x => x.TimeOffRequestId == req.Id);

        if (timeEntry == null || timeEntry.User == null)
        {
            return ManagerResult<string>.Unsuccessful("User associated with the time off request not found or Time entry not found for the time off request.");
        }

        req.Status = request.Status;
        await DbContext.SaveChangesAsync();

        // Fire-and-forget email sending
        _ = Task.Run(() => SendApprovalEmailAsync(timeEntry));

        return ManagerResult<string>.Successful("Time off request status approved successfully and User notified");
    }

    public async Task<ManagerResult<string>> RejectTimeOffRequest(RejectTimeOffStatusRequest request)
    {
        var timeOffRequest = await DbContext.TimeOffRequests
            .FirstOrDefaultAsync(x => x.Id == request.RequestId);

        if (timeOffRequest == null)
        {
            return ManagerResult<string>.Unsuccessful("Time off request not found.");
        }

        // Find the corresponding TimeEntry object related to this TimeOffRequest
        var timeEntry = await DbContext.TimeEntries
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TimeOffRequestId == timeOffRequest.Id);

        if (timeEntry == null || timeEntry.User == null)
        {
            return ManagerResult<string>.Unsuccessful("User associated with the time off request not found or Time entry not found for the time off request.");
        }

        string adminComment = ExtractAdminComment(request.Comment);

        if (!string.IsNullOrEmpty(request.Comment))
        {
            timeEntry.Comment = request.Comment;
        }

        timeOffRequest.Status = request.Status;
        await DbContext.SaveChangesAsync();

        // Fire-and-forget email sending
        _ = Task.Run(() => SendRejectionEmailAsync(timeEntry, adminComment));

        return ManagerResult<string>.Successful("Time off request rejected successfully and User notified");
    }

    /// Extracts the admin comment from the rejection message.
    private string ExtractAdminComment(string comment)
    {
        if (string.IsNullOrEmpty(comment)) return "None";

        var parts = comment.Split("[RejectMessage] - ", 2, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 1 ? parts[1].Trim() : "None";
    }

    /// Sends an approval email asynchronously.
    private async Task SendApprovalEmailAsync(TimeEntries timeEntry)
    {
        try
        {
            var message = new MailMessage
            {
                Subject = "Your Time Off Request Status Update",
                Body = $"Hello {timeEntry.User.FirstName},\n\nYour time off request has been approved.\nDate: {timeEntry.Date}\nHours: {timeEntry.Hours}\nComment: {timeEntry.Comment}",
                IsBodyHtml = false
            };
            message.To.Add(timeEntry.User.Email);

            await _emailService.SendEmailAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to send approval email to {timeEntry.User.Email}: {ex.Message}");
        }
    }

    /// Sends a rejection email asynchronously.
    private async Task SendRejectionEmailAsync(TimeEntries timeEntry, string adminComment)
    {
        try
        {
            string rejectionMessage = $"Hello {timeEntry.User.FirstName},\n\nYour time off request has been rejected.";
            rejectionMessage += $"\nReason: {adminComment}";

            var message = new MailMessage
            {
                Subject = "Your Time Off Request Status Update",
                Body = rejectionMessage + $"\n\nFor Request:\nDate: {timeEntry.Date}\nHours: {timeEntry.Hours}\nComment: {timeEntry.Comment}",
                IsBodyHtml = false
            };
            message.To.Add(timeEntry.User.Email);

            await _emailService.SendEmailAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to send rejection email to {timeEntry.User.Email}: {ex.Message}");
        }
    }
}
