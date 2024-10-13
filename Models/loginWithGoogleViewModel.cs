using System;
using System.ComponentModel.DataAnnotations;

namespace DailyCheckBackend.Models
{
    public class loginWithGoogleViewModel
    {
        [Required]
        public required string Email { get; set; }
        public required string Username { get; set; }

        public required string Id { get; set; }


    }
}
