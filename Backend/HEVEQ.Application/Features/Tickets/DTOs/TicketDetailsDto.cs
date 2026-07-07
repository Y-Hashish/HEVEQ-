using System;
using System.Collections.Generic;

namespace HEVEQ.Application.Features.Tickets.DTOs;

public class TicketAttachmentDto
{
    public Guid Id { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Single message in a ticket — internal admin notes are never included
public class TicketMessageDto
{
    public Guid Id { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<TicketAttachmentDto> Attachments { get; set; } = new();
}

// Full ticket details for GET /api/tickets/{id}
public class TicketDetailsDto
{
    public Guid Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusAr { get; set; } = string.Empty;
    public List<TicketMessageDto> Messages { get; set; } = new();
}