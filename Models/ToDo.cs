using System;
using System.ComponentModel.DataAnnotations;

namespace DailyCheckBackend.Models
{
    public class ToDo
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public required string Title { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public required string Description { get; set; }

        public Status Status { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public DateTime Deadline { get; set; }
    }
}
