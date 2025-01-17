using System;

namespace DailyCheckBackend.Models
{
    public class TodoRequest
    {
        public required string UserId { get; set; }

        // public required Role? UserStatus { get; set; }
        public Status? Status { get; set; }
    }
}
