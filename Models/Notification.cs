using System.ComponentModel.DataAnnotations;

namespace FeedbackMap.Models;

/// <summary>Varsel til leverandøren om en tildeling (høyst ett per tildeling).</summary>
public class Notification
{
    public int Id { get; set; }

    public int AssignmentId { get; set; }   // 1 : 0..1 mot Assignment
    public Assignment Assignment { get; set; } = null!;

    [Required, MaxLength(500)] public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }
}
