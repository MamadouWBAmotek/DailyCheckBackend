using System;
using System.ComponentModel.DataAnnotations;

namespace DailyCheckBackend.Models
{
    public class LoginViewModel
    {
        [Required]
        [MaxLength(100)]
        public required string UserNameOrEmail { get; set; }

        [Required()]
        // [StringLength(80, MinimumLength = 10)]
        // [DataType(DataType.Password)]
        public required string Password { get; set; }
    }
}
