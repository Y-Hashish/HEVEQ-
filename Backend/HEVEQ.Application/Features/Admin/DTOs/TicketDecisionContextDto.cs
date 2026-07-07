using System;
using System.Collections.Generic;

namespace HEVEQ.Application.Features.Admin.DTOs
{
    public class TicketDecisionContextDto
    {
        public Guid Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusAr { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public TicketSubmitterDto SubmittedBy { get; set; } = null!;
        public Guid? AssignedToUserId { get; set; }
        public string? AssignedToUserName { get; set; }

        // Linked Object Summaries (Always returned if the ticket is linked)
        public LinkedBookingDto? LinkedBooking { get; set; }
        public LinkedMarketplaceOrderDto? LinkedMarketplaceOrder { get; set; }

        // Explicit Dispute Details
        public BookingDisputeDetailsDto? BookingDispute { get; set; }
        public MarketplaceOrderDisputeDetailsDto? MarketplaceOrderDispute { get; set; }
        public EscrowSummaryDto? EscrowSummary { get; set; }

        // Aggregated customer attachments from all messages
        public List<TicketAttachmentDto> CustomerAttachments { get; set; } = new();

        // Provider completion evidence
        public ProviderCompletionEvidenceDto? ProviderCompletionEvidence { get; set; }

        // Field verifications list
        public List<FieldVisitDetailsDto> FieldVisits { get; set; } = new();

        // Ticket message log
        public List<TicketMessageDto> Messages { get; set; } = new();

        public List<string> AvailableDisputeDecisions { get; set; } = new();

        public string? AiSummary { get; set; }
        public string? AiIdentifiedIssue { get; set; }
        public string? AiClaimedImpact { get; set; }
        public int? AiEscalationPriority { get; set; }
    }

    public class BookingDisputeDetailsDto
    {
        public Guid BookingId { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string ProviderName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime? DisputeOpenedAt { get; set; }
        public string DisputeReason { get; set; } = string.Empty;
    }

    public class MarketplaceOrderDisputeDetailsDto
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string ListingTitle { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public string BuyerName { get; set; } = string.Empty;
        public DateTime? DisputeOpenedAt { get; set; }
        public string DisputeReason { get; set; } = string.Empty;
    }



    public class TicketAttachmentDto
    {
        public Guid Id { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ProviderCompletionEvidenceDto
    {
        public Guid Id { get; set; }
        public string? ProviderNotes { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? SubmittedAt { get; set; }
        public List<JobCompletionEvidencePhotoDto> Photos { get; set; } = new();
    }

    public class JobCompletionEvidencePhotoDto
    {
        public Guid Id { get; set; }
        public string PhotoUrl { get; set; } = string.Empty;
        public string? Caption { get; set; }
    }

    public class FieldVisitDetailsDto
    {
        public Guid Id { get; set; }
        public Guid DispatchedEmployeeId { get; set; }
        public string DispatchedEmployeeName { get; set; } = string.Empty;
        public string? DispatchInstructions { get; set; }
        public string VisitStatus { get; set; } = string.Empty;
        public string VisitStatusAr { get; set; } = string.Empty;
        public string? EmployeeNotes { get; set; }
        public string? FieldVerificationOutcome { get; set; }
        public string AdminDecision { get; set; } = string.Empty;
        public string? AdminDecisionNote { get; set; }
        public DateTime DispatchedAt { get; set; }
        public DateTime? VisitedAt { get; set; }
        public List<FieldVisitPhotoDto> Photos { get; set; } = new();
    }

    public class FieldVisitPhotoDto
    {
        public Guid Id { get; set; }
        public string PhotoUrl { get; set; } = string.Empty;
        public string? Caption { get; set; }
    }
}

