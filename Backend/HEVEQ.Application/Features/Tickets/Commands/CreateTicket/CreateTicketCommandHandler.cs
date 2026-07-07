using HEVEQ.Application.Common.AI.Interfaces;
using HEVEQ.Application.Common.Exceptions;
using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Features.Tickets.DTOs;
using HEVEQ.Domain.Entities;
using HEVEQ.Domain.Enums;
using HEVEQ.Application.Common.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HEVEQ.Application.Features.Tickets.Commands.CreateTicket;

public class CreateTicketCommandHandler
    : IRequestHandler<CreateTicketCommand, CreateTicketResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly NotificationHelper _notificationHelper;
    private readonly IComplaintAnalysisService _aiAnalysisService;

    public CreateTicketCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        NotificationHelper notificationHelper,
        IComplaintAnalysisService aiAnalysisService)
    {
        _context = context;
        _currentUser = currentUser;
        _notificationHelper = notificationHelper;
        _aiAnalysisService = aiAnalysisService;
    }

    public async Task<CreateTicketResult> Handle(
        CreateTicketCommand request,
        CancellationToken cancellationToken)
    {
        // Business Rule: authenticated only
        var userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("User is not authenticated.");

        // ── AI Analysis Phase ─────────────────────────────────────────────────
        // نقوم بتحليل رسالة التذكرة لاستخراج الملخص وتحديد الأولوية
        var aiResult = await _aiAnalysisService.AnalyzeComplaintAsync(request.Message, cancellationToken);

        // تعيين الأولوية الرقمية بناءً على مخرجات الـ AI
        // بافتراض أن: 1 = Normal, 2 = High, 3 = Urgent
        int ticketPriority = aiResult.Priority.ToUpper() switch
        {
            "URGENT" => 3,
            "HIGH" => 2,
            "NORMAL" => 1,
            _ => 1 // القيمة الافتراضية في حال حدوث خطأ
        };

        // ── Ticket Generation ─────────────────────────────────────────────────
        var count = await _context.Tickets.CountAsync(cancellationToken);
        var ticketNumber = $"TKT-{(count + 1):D4}";

        var ticket = new Ticket
        {
            SubmittedById = userId,
            TicketNumber = ticketNumber,
            Subject = request.Subject,
            Category = request.Category,
            BookingId = request.BookingId,
            MarketplaceOrderId = request.MarketplaceOrderId,
            Status = TicketStatus.Open,

            // تعيين قيم الذكاء الاصطناعي
            Priority = ticketPriority,
            AiSummary = aiResult.Summary, // يجب التأكد من إضافة هذه الخاصية في كيان Ticket

            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Tickets.Add(ticket);

        // Full Flow Step 4: create the first TicketMessage from the user's initial message
        var firstMessage = new TicketMessage
        {
            TicketId = ticket.Id,
            SenderId = userId,
            Body = request.Message,
            IsInternal = false,   // always false for user-submitted messages
            CreatedAt = DateTime.UtcNow
        };

        if (request.Attachments != null && request.Attachments.Any())
        {
            if (request.Attachments.Count > 5)
            {
                throw new ValidationException("Attachments", "Cannot upload more than 5 attachments.");
            }

            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".pdf", ".mp4", ".mov" };
            foreach (var att in request.Attachments)
            {
                if (!Uri.TryCreate(att.FileUrl, UriKind.Absolute, out var uri))
                {
                    throw new ValidationException("Attachments", $"Invalid URL format: {att.FileUrl}");
                }

                var path = uri.AbsolutePath.ToLowerInvariant();
                var ext = Path.GetExtension(path);
                if (!allowedExtensions.Contains(ext))
                {
                    throw new ValidationException("Attachments", $"File type '{ext}' is not allowed.");
                }

                firstMessage.Attachments.Add(new TicketAttachment
                {
                    Id = Guid.NewGuid(),
                    TicketMessageId = firstMessage.Id,
                    UploadedByUserId = userId,
                    FileUrl = att.FileUrl,
                    FileName = string.IsNullOrEmpty(att.FileName) ? Path.GetFileName(path) : att.FileName,
                    FileType = att.FileType,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        _context.TicketMessages.Add(firstMessage);

        await _notificationHelper.TicketCreatedForAdminsAsync(ticket.Id,ticket.TicketNumber,ticket.Subject);
        await _context.SaveChangesAsync(cancellationToken);

        // Full Flow Step 5: ticket now appears in admin queue
        return new CreateTicketResult
        {
            Id = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            Status = ticket.Status.ToString(),
            StatusAr = MapStatusAr(ticket.Status),
            Message = "Ticket created successfully"
        };
    }

    internal static string MapStatusAr(TicketStatus status) => status switch
    {
        TicketStatus.Open => "مفتوحة",
        TicketStatus.InProgress => "قيد المعالجة",
        TicketStatus.PendingCustomerReply => "بانتظار ردك",
        TicketStatus.PendingProviderReply => "بانتظار رد المزود",
        TicketStatus.PendingFieldVerification => "بانتظار التحقق الميداني",
        TicketStatus.Resolved => "محلولة",
        TicketStatus.Closed => "مغلقة",
        TicketStatus.Reopened => "معاد فتحها",
        _ => status.ToString()
    };
}