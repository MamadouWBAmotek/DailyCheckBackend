using System;
using System.ComponentModel.DataAnnotations;

namespace DailyCheckBackend.Models
{
    public enum Role
    {
        Admin,
        User
    }
    public class User
    {

        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Username is required.")]
        public required string UserName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        public required string Email { get; set; }
        [Required(ErrorMessage = "Role is required.")]
        public Role Role { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public required string Password { get; set; }


    }
}

