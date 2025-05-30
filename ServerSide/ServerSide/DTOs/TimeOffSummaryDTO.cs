namespace ServerSide.DTOs;

public class TimeOffSummaryDTO
{
    public int UserId { get; set; }        // Unique ID of the employee
    public string UserName { get; set; }   // Employee's name
    public double TotalHours { get; set; } // Total hours logged as time off

    public TimeOffSummaryDTO(int userId, string userName, double totalHours)
    {
        UserId = userId;
        UserName = userName;
        TotalHours = totalHours;
    }
}