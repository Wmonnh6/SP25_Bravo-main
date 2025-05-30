using System.ComponentModel.DataAnnotations;

namespace ServerSide.Models.Entities;

public class MyTimeEntryTask
{
    public int Id { get; set; }

    [MaxLength(512)]
    public string Name { get; set; } = null!;

    public bool IsActive { get; set; } 

    public bool IsTimeOff { get; set; }
}