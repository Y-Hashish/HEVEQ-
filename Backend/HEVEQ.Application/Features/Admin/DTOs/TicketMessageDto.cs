using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Features.Admin.DTOs
{
    public class TicketMessageDto
    {
        public Guid Id { get; set; }
        public string SenderName { get; set; }
        public string Body { get; set; }
        public string Content { get; set; } = string.Empty;
        public string SenderRole { get; set; } = string.Empty;
        public Guid SenderId { get; set; }
        public bool IsInternal { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
