using System;

namespace DailyCheckBackend.Models
{
    public class UserUpdateViewModel
    {
        public required int Id { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public Role Role { get; set; }

        public required string Password { get; set; }
        public required string NewPassword { get; set; }
    }
}
