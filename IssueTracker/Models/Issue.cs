    namespace IssueTracker.Models
    {
        public class Issue
        {
            public int Id { get; set; }
            public string Title { get; set; } = "";
            public string Description { get; set; } = "";
            public string Status { get; set; } = "Open"; // Open/InProgress/Resolved
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        }
    }
