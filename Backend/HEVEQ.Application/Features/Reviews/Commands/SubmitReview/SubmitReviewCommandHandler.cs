using HEVEQ.Application.Common.AI.Interfaces;
using HEVEQ.Application.Common.Exceptions;
using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Features.Reviews.DTOs;
using HEVEQ.Domain.Entities;
using HEVEQ.Domain.Enums;
using HEVEQ.Application.Common.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HEVEQ.Application.Features.Reviews.Commands.SubmitReview;

public class SubmitReviewCommandHandler
    : IRequestHandler<SubmitReviewCommand, SubmitReviewResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly NotificationHelper _notificationHelper;
    private readonly IReviewFilterService _reviewFilterService;

    public SubmitReviewCommandHandler(IApplicationDbContext context,ICurrentUserService currentUser,NotificationHelper notificationHelper,IReviewFilterService reviewFilterService)
    {
        _context = context;
        _currentUser = currentUser;
        _notificationHelper = notificationHelper;
        _reviewFilterService = reviewFilterService;
    }

    public async Task<SubmitReviewResult> Handle(
        SubmitReviewCommand request,
        CancellationToken cancellationToken)
    {
        var reviewerId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("User is not authenticated.");

        Guid reviewedUserId;
        Guid? serviceListingId = null;
        Guid? marketplaceListingId = null;
        string serviceContext = string.Empty; // المتغير الذي سنرسله للـ AI لمعرفة السياق

        if (request.BookingId.HasValue)
        {
            // ── Booking path ──────────────────────────────────────────────────

            var booking = await _context.Bookings
                .AsNoTracking()
                .Include(b => b.ServiceListing)
                    .ThenInclude(l => l.ProviderProfile)
                .FirstOrDefaultAsync(b => b.Id == request.BookingId.Value, cancellationToken)
                ?? throw new NotFoundException(nameof(Booking), request.BookingId.Value);

            if (booking.Status != BookingStatus.Completed)
                throw new InvalidOperationException(
                    "A review can only be submitted after the booking is completed.");

            if (booking.CustomerId != reviewerId)
                throw new ForbiddenAccessException(
                    "You are not the customer of this booking.");

            var alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.ReviewerId == reviewerId
                               && r.BookingId == request.BookingId.Value
                               && r.ModerationStatus != ModerationStatus.Rejected,
                          cancellationToken);

            if (alreadyReviewed)
                throw new InvalidOperationException(
                    "You have already submitted a review for this booking.");

            reviewedUserId = booking.ServiceListing.ProviderProfile.UserId;
            serviceListingId = booking.ServiceListingId;

            // تحديد سياق الخدمة للـ AI (افترضت وجود خاصية Title، قم بتغييرها إن كانت Name)
            serviceContext = $"خدمة تأجير معدات ثقيلة: {booking.ServiceListing.Title}";
        }
        else
        {
            // ── Marketplace order path ────────────────────────────────────────

            var order = await _context.MarketplaceOrders
                .AsNoTracking()
                .Include(o => o.Listing)
                .FirstOrDefaultAsync(o => o.Id == request.MarketplaceOrderId!.Value, cancellationToken)
                ?? throw new NotFoundException(nameof(MarketplaceOrder), request.MarketplaceOrderId!.Value);

            if (order.Status != MarketplaceOrderStatus.Completed)
                throw new InvalidOperationException(
                    "A review can only be submitted after the order is completed.");

            if (order.BuyerId != reviewerId)
                throw new ForbiddenAccessException(
                    "You are not the buyer of this order.");

            var alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.ReviewerId == reviewerId
                               && r.MarketplaceOrderId == request.MarketplaceOrderId!.Value
                               && r.ModerationStatus != ModerationStatus.Rejected,
                          cancellationToken);

            if (alreadyReviewed)
                throw new InvalidOperationException(
                    "You have already submitted a review for this order.");

            reviewedUserId = order.Listing.SellerId;
            marketplaceListingId = order.ListingId;

            // تحديد سياق الخدمة للـ AI
            serviceContext = $"شراء معدة من السوق: {order.Listing.Title}";
        }

        // ── AI Moderation Phase ───────────────────────────────────────────────

        bool isPublished = true;
        ModerationStatus modStatus = ModerationStatus.Approved; 
        string aiReason = string.Empty;

        // نفحص التقييم فقط إذا قام العميل بكتابة نص، أما إذا كان تقييماً بالنجوم فقط فيعبر مباشرة
        if (!string.IsNullOrWhiteSpace(request.Comment))
        {
            var filterResult = await _reviewFilterService.FilterReviewAsync(
                request.Comment,
                serviceContext,
                cancellationToken);

            if (filterResult.RequiresAdminReview)
            {
                isPublished = false;
                modStatus = ModerationStatus.Pending; // يعود لحالة الانتظار ليراه الأدمن
                aiReason = filterResult.Reason; // يمكنك حفظ هذا السبب في قاعدة البيانات لاحقاً
            }
        }

        // ── Create Review ─────────────────────────────────────────────────────

        var review = new Review
        {
            ReviewerId = reviewerId,
            ReviewedUserId = reviewedUserId,
            BookingId = request.BookingId,
            ServiceListingId = serviceListingId,
            MarketplaceOrderId = request.MarketplaceOrderId,
            MarketplaceListingId = marketplaceListingId,
            Rating = request.Rating,
            Comment = request.Comment,
            ModerationStatus = modStatus,
            IsPublished = isPublished,
            // يمكنك إضافة حقل AdminNotes أو AiReason في الـ Entity الخاص بك لحفظ سبب الرفض
            // AdminNotes = aiReason, 
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        _notificationHelper.ReviewReceived(reviewedUserId, review.Id, "Review");

        // ── Step 6: Update rating aggregates on the reviewed user's profile ───

        // التعديل هنا: نقوم بتحديث إجمالي التقييمات فقط إذا كان التقييم منشوراً (سليم)
        if (request.BookingId.HasValue && review.IsPublished)
        {
            var providerProfile = await _context.ProviderProfiles
                .FirstOrDefaultAsync(p => p.UserId == reviewedUserId, cancellationToken);

            if (providerProfile is not null)
            {
                var existingRatings = await _context.Reviews
                    .Where(r => r.ReviewedUserId == reviewedUserId && r.IsPublished)
                    .Select(r => r.Rating)
                    .ToListAsync(cancellationToken);

                existingRatings.Add(request.Rating);

                providerProfile.TotalReviewsCount = existingRatings.Count;
                providerProfile.AverageRating = Math.Round(
                    (decimal)existingRatings.Average(), 2);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new SubmitReviewResult
        {
            Id = review.Id,
            Rating = review.Rating,
            IsPublished = review.IsPublished,
            Message = review.IsPublished
                ? "تم نشر التقييم بنجاح"
                : "تم استلام تقييمك وإرساله للمراجعة قبل النشر."
        };
    }
}