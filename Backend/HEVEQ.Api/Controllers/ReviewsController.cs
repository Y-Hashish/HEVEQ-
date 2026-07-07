using HEVEQ.Application.Features.Reviews.Commands.SubmitReview;
using HEVEQ.Application.Features.Reviews.Queries.GetUserReviews;
using HEVEQ.Application.Features.Reviews.Queries.GetServiceListingReviews;
using HEVEQ.Application.Features.Reviews.Queries.GetMarketplaceSellerReviews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HEVEQ.Api.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly ISender _mediator;

    public ReviewsController(ISender mediator)
    {
        _mediator = mediator;
    }

    // ── POST /api/reviews ─────────────────────────────────────────────────────
    // Authenticated only.
    // Reviewer must be the customer (for booking) or buyer (for marketplace order).
    // Only allowed after the transaction reaches Completed status.
    // No AI moderation in this phase — auto-published.
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Submit(
        [FromBody] SubmitReviewCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Created(string.Empty, result);
    }

    // ── GET /api/reviews/for-user/{userId} ────────────────────────────────────
    // Public — no auth required.
    // Returns only IsPublished=true reviews for the given userId.
    // Used by: Provider profile panel, Seller profile panel, Service details.
    [HttpGet("for-user/{userId:guid}")]
    public async Task<IActionResult> GetForUser(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserReviewsQuery(userId), cancellationToken);
        return Ok(result);
    }


    [HttpGet("service-listing/{serviceListingId:guid}")]
    public async Task<IActionResult> GetForServiceListing(
        Guid serviceListingId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetServiceListingReviewsQuery(serviceListingId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("marketplace-seller/{sellerId:guid}")]
    public async Task<IActionResult> GetForMarketplaceSeller(
        Guid sellerId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMarketplaceSellerReviewsQuery(sellerId), cancellationToken);
        return Ok(result);
    }
}