using System;

namespace DailyCheckBackend.Models
{
    public class GoogleUser
    {
        public required string Email { get; set; }
        public required string Username { get; set; }

        public required string Id { get; set; }
        public required Role Role { get; set; }

    }
}
