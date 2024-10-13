using System;
using System.ComponentModel.DataAnnotations;
using static DailyCheckBackend.Models.User;

namespace DailyCheckBackend.Models
{
    public class RegistrationViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [MaxLength(20, ErrorMessage = "Max 20 characters allowed.")]
        public required string UserName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [MaxLength(100, ErrorMessage = "Max 100 characters allowed.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(80, MinimumLength = 10, ErrorMessage = "Password must be between 10 and 80 characters.")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public required string ConfirmPassword { get; set; }

        [Required]
        public Role Role { get; set; }

    }
}
