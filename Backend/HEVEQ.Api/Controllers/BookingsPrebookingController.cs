using HEVEQ.Application.Features.Bookings.PreBookingEvaluation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HEVEQ.Api.Controllers;

[Route("api/bookings/prebooking")]
[ApiController]
public class BookingsPrebookingController : ControllerBase
{
    private readonly IMediator _mediator;

    public BookingsPrebookingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("{bookingId:guid}/evaluate")]
    [Authorize(Roles = "Customer,Provider,Admin")]
    public async Task<IActionResult> EvaluatePreBooking([FromRoute] Guid bookingId, CancellationToken ct)
    {
        var result = await _mediator.Send(new EvaluatePreBookingCommand(bookingId), ct);
        return Ok(result);
    }
}
