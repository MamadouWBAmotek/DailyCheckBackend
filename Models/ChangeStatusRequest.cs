using System;

namespace DailyCheckBackend.Models
{
    public class ChangeStatusRequest
    {
        public required int TodoId { get; set; }
        public Status NewStatus { get; set; }
    }
}
