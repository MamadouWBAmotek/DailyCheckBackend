using System;
using System.ComponentModel.DataAnnotations;

namespace DailyCheckBackend.Models
{
    public class ToDoViewModel
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required Status Status { get; set; }
        public required string UserId { get; set; }
        public required DateTime Deadline { get; set; }
    }
}
