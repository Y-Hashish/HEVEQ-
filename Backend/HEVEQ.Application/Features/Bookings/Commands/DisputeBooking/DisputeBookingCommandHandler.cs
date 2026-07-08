using HEVEQ.Application.Common.AI.Interfaces;
using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Common.Services;
using HEVEQ.Application.Features.Bookings.DTOs;
using HEVEQ.Application.Features.Bookings.Helpers;
using HEVEQ.Domain.Entities;
using HEVEQ.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HEVEQ.Application.Features.Bookings.Commands.DisputeBooking
{
    public sealed class DisputeBookingCommandHandler : IRequestHandler<DisputeBookingCommand, DisputeBookingResponseDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly NotificationHelper _notificationHelper;
        private readonly IComplaintAnalysisService _aiAnalysisService;

        public DisputeBookingCommandHandler(
            IApplicationDbContext context,
            NotificationHelper notificationHelper,
            IComplaintAnalysisService aiAnalysisService)
        {
            _context = context;
            _notificationHelper = notificationHelper;
            _aiAnalysisService = aiAnalysisService;
        }

        public async Task<DisputeBookingResponseDto> Handle(DisputeBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _context.Bookings
                .Include(x => x.ServiceListing)
                    .ThenInclude(x => x.ProviderProfile)
                .FirstOrDefaultAsync(x => x.Id == request.BookingId, cancellationToken);

            if (booking is null)
                throw new InvalidOperationException("Booking was not found.");

            if (booking.CustomerId != request.CustomerId)
                throw new InvalidOperationException("Only the booking customer can open a dispute.");

            if (booking.Status != BookingStatus.PendingCustomerConfirmation)
                throw new InvalidOperationException("Booking must be pending customer confirmation before opening a dispute.");

            var now = DateTime.UtcNow;
            booking.Status = BookingStatus.Disputed;
            booking.DisputeOpenedAt = DateTime.UtcNow;

            var escrow = await _context.EscrowRecords.FirstOrDefaultAsync(x => x.BookingId == booking.Id && x.Status == EscrowStatus.Held, cancellationToken);

            if (escrow is not null) { 
                escrow.Status = EscrowStatus.Frozen;
                escrow.FrozenAt = now;
                escrow.FreezeReason = request.Reason;
            }


            var ticketCount = await _context.Tickets.CountAsync(cancellationToken);
            var aiResult = await AnalyzeDisputeSafelyAsync(booking, request.Reason, cancellationToken);

            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                SubmittedById = request.CustomerId,
                TicketNumber = $"TKT-{(ticketCount + 1):D4}",
                Subject = $"نزاع على الحجز رقم {booking.BookingNumber}",
                Category = TicketCategory.CompletionDispute,
                BookingId = booking.Id,
                MarketplaceOrderId = null,
                Status = TicketStatus.Open,
                Priority = MapAiPriority(aiResult.Priority),
                AiSummary = aiResult.Summary,
                AiIdentifiedIssue = "نزاع على اكتمال الحجز",
                AiClaimedImpact = request.Reason,
                AiEscalationPriority = MapAiPriority(aiResult.Priority),
                CreatedAt = now,
                UpdatedAt = now
            };
            var ticketMessage = new TicketMessage
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                SenderId = request.CustomerId,
                Body = request.Reason,
                MessageType = TicketMessageType.User,
                IsInternal = false,
                CreatedAt = now
            };

            foreach (var url in request.EvidencePhotoUrls.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                ticketMessage.Attachments.Add(new TicketAttachment
                {
                    Id = Guid.NewGuid(),
                    TicketMessageId = ticketMessage.Id,
                    UploadedByUserId = request.CustomerId,
                    FileUrl = url,
                    FileName = ExtractFileName(url),
                    FileType = AttachmentFileType.Image,
                    CreatedAt = now
                });
            }
            _context.Tickets.Add(ticket);
            _context.TicketMessages.Add(ticketMessage);
            _notificationHelper.BookingDisputed(booking.ServiceListing.ProviderProfile.UserId, booking.Id, booking.BookingNumber);
            await _notificationHelper.BookingDisputedForAdminsAsync(booking.Id, booking.BookingNumber);
            await _context.SaveChangesAsync(cancellationToken);
            return new DisputeBookingResponseDto
            {
                BookingId = booking.Id,
                Status = booking.Status.ToString(),
                StatusAr = BookingDisplayHelper.GetStatusAr(booking.Status),
                TicketId = ticket.Id,
                Message = "تم فتح النزاع بنجاح"
            };
        }
        private async Task<HEVEQ.Application.Common.AI.Models.ComplaintAnalysisResult> AnalyzeDisputeSafelyAsync(Booking booking, string reason, CancellationToken cancellationToken)
        {
            var complaintText = $"""
                نوع التذكرة: نزاع على إكمال حجز خدمة.
                رقم الحجز: {booking.BookingNumber}.
                عنوان المهمة: {booking.JobTitle}.
                وصف المهمة: {booking.JobDescription}.
                الخدمة: {booking.ServiceListing?.Title}.
                المحافظة: {booking.Governorate}.
                المنطقة: {booking.District}.
                قيمة الحجز: {booking.EstimatedTotal}.
                سبب النزاع من العميل: {reason}
                """;

            try
            {
                return await _aiAnalysisService.AnalyzeComplaintAsync(complaintText, cancellationToken);
            }
            catch
            {
                return new HEVEQ.Application.Common.AI.Models.ComplaintAnalysisResult
                {
                    Priority = "HIGH",
                    Summary = "تعذّر تحليل النزاع آليًا. يُرجى مراجعة سبب النزاع ورسائل العميل والمرفقات يدويًا."
                };
            }
        }

        private static int MapAiPriority(string? priority) => priority?.Trim().ToUpperInvariant() switch
        {
            "URGENT" => 3,
            "HIGH" => 2,
            _ => 1
        };

        private static string ExtractFileName(string url)
        {
            try
            {
                var uri = new Uri(url);
                var fileName = Path.GetFileName(uri.LocalPath);
                return string.IsNullOrWhiteSpace(fileName) ? "evidence-image" : fileName;
            }
            catch
            {
                return "evidence-image";
            }
        }
    }
}