using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Features.Admin.DTOs
{
    public class AdminTicketDto
    {
        public Guid Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty; // Frontend compatibility
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusAr { get; set; } = string.Empty;
        public string SubmittedByName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty; // Frontend compatibility
        public string Priority { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class PaginatedAdminTicketsResponse
    {
        public List<AdminTicketDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
