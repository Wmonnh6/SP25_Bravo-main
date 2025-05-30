using ServerSide.Models;
using System.Text.Json.Serialization;

namespace ServerSide.DTOs;

public class ApproveTimeOffStatusRequest
{
    public int RequestId { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TimeOffRequestStatusEnum Status { get; set; } 
}