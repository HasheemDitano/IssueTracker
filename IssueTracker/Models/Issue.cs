using System;
using System.ComponentModel.DataAnnotations;

namespace IssueTracker.Models
{
    public class Issue
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = "";

        [StringLength(1000)]
        public string Description { get; set; } = "";

        [Required]
        public string Status { get; set; } = "Open"; // Open / In Progress / Waiting for User / Resolved / Closed

        [Range(1, 5)]
        public int Priority { get; set; } = 3; // 1 = Highest, 5 = Lowest

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // who created this issue (Identity user Id)
        public string? CreatedByUserId { get; set; }

        // who is assigned to resolve this issue (Engineer/Admin user Id)
        public string? AssignedToUserId { get; set; }
    }
}
