using System;
using System.ComponentModel.DataAnnotations;

namespace IssueTracker.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public int IssueId { get; set; } // FK to Issue

        [Required]
        [StringLength(1000)]
        public string Text { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? CreatedByUserId { get; set; }
    }
}
