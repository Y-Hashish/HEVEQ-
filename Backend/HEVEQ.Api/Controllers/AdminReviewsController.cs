using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HEVEQ.Api.Controllers;

[ApiController]
[Route("api/admin/reviews")]
[Authorize(Roles = "Admin")]
public class AdminReviewsController(IApplicationDbContext context) : ControllerBase
{
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = context.Reviews
            .AsNoTracking()
            .Include(r => r.Reviewer)
            .Include(r => r.ReviewedUser)
            .Include(r => r.Booking)
                .ThenInclude(b => b.ServiceListing)
            .Include(r => r.MarketplaceOrder)
                .ThenInclude(o => o.Listing)
            .Where(r => !r.IsPublished && (r.ModerationStatus == ModerationStatus.Pending || r.ModerationStatus == ModerationStatus.Flagged))
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new AdminReviewModerationDto
            {
                Id = r.Id,
                ReviewerName = (r.Reviewer.FirstName + " " + r.Reviewer.LastName).Trim(),
                ReviewedUserName = (r.ReviewedUser.FirstName + " " + r.ReviewedUser.LastName).Trim(),
                Rating = r.Rating,
                Comment = r.Comment,
                ModerationStatus = r.ModerationStatus.ToString(),
                CreatedAt = r.CreatedAt,
                SourceType = r.BookingId.HasValue ? "Booking" : "MarketplaceOrder",
                SourceTitle = r.BookingId.HasValue
                    ? (r.Booking != null && r.Booking.ServiceListing != null ? r.Booking.ServiceListing.Title : "حجز خدمة")
                    : (r.MarketplaceOrder != null && r.MarketplaceOrder.Listing != null ? r.MarketplaceOrder.Listing.Title : "طلب سوق"),
                BookingId = r.BookingId,
                MarketplaceOrderId = r.MarketplaceOrderId
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            items,
            totalCount,
            page,
            pageSize
        });
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var review = await context.Reviews.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (review is null)
        {
            return NotFound(new { message = "التقييم غير موجود" });
        }

        review.ModerationStatus = ModerationStatus.Approved;
        review.IsPublished = true;
        review.PublishedAt = DateTime.UtcNow;

        await UpdateProviderRatingIfNeeded(review.ReviewedUserId, review.Id, review.Rating, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "تم نشر التقييم بنجاح" });
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, CancellationToken cancellationToken)
    {
        var review = await context.Reviews.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (review is null)
        {
            return NotFound(new { message = "التقييم غير موجود" });
        }

        review.ModerationStatus = ModerationStatus.Rejected;
        review.IsPublished = false;
        review.PublishedAt = null;

        await context.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "تم رفض التقييم" });
    }

    private async Task UpdateProviderRatingIfNeeded(Guid reviewedUserId, Guid currentReviewId, int currentRating, CancellationToken cancellationToken)
    {
        var providerProfile = await context.ProviderProfiles
            .FirstOrDefaultAsync(p => p.UserId == reviewedUserId, cancellationToken);

        if (providerProfile is null)
        {
            return;
        }

        var ratings = await context.Reviews
            .Where(r => r.ReviewedUserId == reviewedUserId && r.IsPublished && r.Id != currentReviewId)
            .Select(r => r.Rating)
            .ToListAsync(cancellationToken);

        ratings.Add(currentRating);

        providerProfile.TotalReviewsCount = ratings.Count;
        providerProfile.AverageRating = Math.Round((decimal)ratings.Average(), 2);
    }
}

public class AdminReviewModerationDto
{
    public Guid Id { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public string ReviewedUserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string ModerationStatus { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string SourceTitle { get; set; } = string.Empty;
    public Guid? BookingId { get; set; }
    public Guid? MarketplaceOrderId { get; set; }
    public DateTime CreatedAt { get; set; }
}
