using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Features.Admin.DTOs
{
    public class AdminTicketDetailsDto
    {
        public Guid Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty; // Frontend compatibility
        public string Status { get; set; } = string.Empty;
        public string StatusAr { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string SubmittedByName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty; // Frontend compatibility
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public TicketSubmitterDto SubmittedBy { get; set; } = null!;
        public List<TicketMessageDto> Messages { get; set; } = new();

        public LinkedBookingDto? LinkedBooking { get; set; }
        public LinkedMarketplaceOrderDto? LinkedMarketplaceOrder { get; set; }

        public Guid? AssignedToUserId { get; set; }
        public string? AssignedToUserName { get; set; }
        public EscrowSummaryDto? EscrowSummary { get; set; }
        public List<string> AvailableDisputeDecisions { get; set; } = new();
    }

    public class EscrowSummaryDto
    {
        public Guid Id { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal PlatformCommission { get; set; }
        public decimal ProviderPayout { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
