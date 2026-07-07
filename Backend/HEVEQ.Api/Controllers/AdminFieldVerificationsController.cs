using HEVEQ.Application.Features.Admin.Command.DispatchFieldVerification;
using HEVEQ.Application.Features.Admin.Command.FieldVerificationDecision;
using HEVEQ.Application.Features.Admin.Command.AssignFieldVisit;
using HEVEQ.Application.Features.Admin.Query.GetFieldVerifications;
using HEVEQ.Application.Features.Admin.Query.GetFieldVerificationDetails;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HEVEQ.Api.Controllers
{
    [Route("api/admin/field-verifications")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminFieldVerificationsController(IMediator _mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetVerifications([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null)
        {
            var query = new GetFieldVerificationsQuery
            {
                Page = page,
                PageSize = pageSize,
                Status = status
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVerificationDetails(Guid id)
        {
            var query = new GetFieldVerificationDetailsQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(new { message = "Verification not found." });
            }

            return Ok(result);
        }

        public class DispatchRequest
        {
            public Guid EmployeeId { get; set; }
            public string? ScheduledDate { get; set; }
            public string? DispatchInstructions { get; set; }
        }

        [HttpPost("{bookingId}/dispatch")]
        public async Task<IActionResult> DispatchVerification(Guid bookingId, [FromBody] DispatchRequest request)
        {
            var adminIdString = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminIdString) || !Guid.TryParse(adminIdString, out Guid adminIdGuid))
            {
                return Unauthorized(new { message = "Invalid Token Claims" });
            }

            var command = new DispatchFieldVerificationCommand
            {
                BookingId = bookingId,
                EmployeeUserId = request.EmployeeId,
                DispatchInstructions = request.DispatchInstructions ?? $"Scheduled Date: {request.ScheduledDate}",
                AdminId = adminIdGuid
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return result.StatusCode switch
                {
                    404 => NotFound(new { message = result.Message }),
                    400 => BadRequest(new { message = result.Message }),
                    _ => StatusCode(500, new { message = result.Message })
                };
            }

            return Ok(result);
        }

        public class DecisionRequest
        {
            public string Decision { get; set; } = string.Empty;
            public string AdminNote { get; set; } = string.Empty;
        }

        [HttpPost("{id}/decision")]
        public async Task<IActionResult> SaveDecision(Guid id, [FromBody] DecisionRequest request)
        {
            var adminIdString = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminIdString) || !Guid.TryParse(adminIdString, out Guid adminIdGuid))
            {
                return Unauthorized(new { message = "Invalid Token Claims" });
            }

            var decision = request.Decision;
            if (decision.Equals("Approve", StringComparison.OrdinalIgnoreCase))
            {
                decision = "ReleaseToProvider";
            }
            else if (decision.Equals("Reject", StringComparison.OrdinalIgnoreCase))
            {
                decision = "RefundToCustomer";
            }

            var command = new FieldVerificationDecisionCommand
            {
                Id = id,
                AdminId = adminIdGuid,
                AdminDecision = decision,
                AdminDecisionNote = request.AdminNote
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return result.StatusCode switch
                {
                    404 => NotFound(new { message = result.Message }),
                    400 => BadRequest(new { message = result.Message }),
                    _ => StatusCode(500, new { message = result.Message })
                };
            }

            return Ok(result);
        }

        public class AssignEmployeeRequest
        {
            public Guid EmployeeUserId { get; set; }
        }

        [HttpPost("~/api/admin/field-visits/{fieldVisitId}/assign")]
        public async Task<IActionResult> AssignFieldVisit(Guid fieldVisitId, [FromBody] AssignEmployeeRequest request)
        {
            var adminIdString = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminIdString) || !Guid.TryParse(adminIdString, out Guid adminIdGuid))
            {
                return Unauthorized(new { message = "Invalid Token Claims" });
            }

            var command = new AssignFieldVisitCommand
            {
                FieldVisitId = fieldVisitId,
                EmployeeUserId = request.EmployeeUserId,
                AdminId = adminIdGuid
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return result.StatusCode switch
                {
                    404 => NotFound(new { message = result.Message }),
                    400 => BadRequest(new { message = result.Message }),
                    _ => StatusCode(500, new { message = result.Message })
                };
            }

            return Ok(result);
        }
    }
}
