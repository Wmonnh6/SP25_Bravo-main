using System.Text.Json.Serialization;
using ServerSide.Models;

namespace ServerSide.DTOs;

public class RejectTimeOffStatusRequest
{
    public int RequestId { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TimeOffRequestStatusEnum Status { get; set; } 
    public string Comment { get; set; } = string.Empty;// Optional comment for rejection
}