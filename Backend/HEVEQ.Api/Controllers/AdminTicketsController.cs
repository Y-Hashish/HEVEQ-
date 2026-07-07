using HEVEQ.Application.Features.Admin.Command.AddTicketMessage;
using HEVEQ.Application.Features.Admin.Command.ResolveTicket;
using HEVEQ.Application.Features.Admin.Command.ClaimTicket;
using HEVEQ.Application.Features.Admin.Command.AssignTicket;
using HEVEQ.Application.Features.Admin.Command.TakeoverTicket;
using HEVEQ.Application.Features.Admin.Command.DisputeDecision;
using HEVEQ.Application.Features.Admin.Query.GetAdminDisputes;
using HEVEQ.Application.Features.Admin.Query.GetAdminTicketDetails;
using HEVEQ.Application.Features.Admin.Query.GetAdminTickets;
using HEVEQ.Application.Features.Admin.Query.GetTicketDecisionContext;
using HEVEQ.Application.Features.Admin.Query.GetFieldVisitsForTicket;
using HEVEQ.Application.Features.Admin.Command.DispatchFieldVerification;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HEVEQ.Api.Controllers
{
    [Route("api/admin/tickets")]
    [ApiController]
    [Authorize(Roles = "Admin,Employee")]
    public class AdminTicketsController(IMediator _mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetTickets([FromQuery] GetAdminTicketsQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketDetails(Guid id)
        {
            var query = new GetAdminTicketDetailsQuery { Id = id };
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(new { message = "Ticket not found." });
            }
            return Ok(result);
        }

        [HttpGet("{id}/decision-context")]
        public async Task<IActionResult> GetTicketDecisionContext(Guid id)
        {
            var query = new GetTicketDecisionContextQuery(id);
            var result = await _mediator.Send(query);
            if (result == null)
            {
                return NotFound(new { message = "Ticket not found." });
            }
            return Ok(result);
        }

        public class CreateFieldVisitRequest
        {
            public Guid EmployeeUserId { get; set; }
            public string DispatchInstructions { get; set; } = string.Empty;
        }

        [HttpPost("{id}/field-visits")]
        public async Task<IActionResult> CreateFieldVisit(Guid id, [FromBody] CreateFieldVisitRequest request)
        {
            var adminIdString = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminIdString) || !Guid.TryParse(adminIdString, out Guid adminIdGuid))
            {
                return Unauthorized(new { message = "Invalid Token Claims" });
            }

            var command = new CreateFieldVisitFromTicketCommand
            {
                TicketId = id,
                EmployeeUserId = request.EmployeeUserId,
                DispatchInstructions = request.DispatchInstructions,
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

        [HttpGet("{id}/field-visits")]
        public async Task<IActionResult> GetFieldVisits(Guid id)
        {
            var query = new GetFieldVisitsForTicketQuery(id);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("{id}/messages")]
        public async Task<IActionResult> AddMessage(Guid id, [FromBody] AddTicketMessageCommand command)
        {
            var adminIdString = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(adminIdString) && Guid.TryParse(adminIdString, out Guid adminIdGuid))
            {
                command.AdminId = adminIdGuid;
            }

            command.TicketId = id;

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return result.StatusCode switch
                {
                    404 => NotFound(new { message = result.Message }),
                    400 => BadRequest(new { message = result.Message }),
                    403 => StatusCode(403, new { message = result.Message }),
                    _ => StatusCode(500, new { message = result.Message })
                };
            }

            return Ok(result);
        }

        [HttpPost("{id}/resolve")]
        public async Task<IActionResult> ResolveTicket(Guid id, [FromBody] ResolveTicketCommand command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return result.StatusCode switch
                {
                    400 => BadRequest(new { message = result.Message }),
                    404 => NotFound(new { message = result.Message }),
                    _ => StatusCode(500, new { message = result.Message })
                };
            }

            return Ok(result);
        }

        [HttpPost("{id}/claim")]
        public async Task<IActionResult> ClaimTicket(Guid id)
        {
            var userIdString = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role) ?? (User.IsInRole("Admin") ? "Admin" : User.IsInRole("Employee") ? "Employee" : string.Empty);

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userIdGuid))
            {
                return Unauthorized(new { message = "Invalid Token Claims" });
            }

            var command = new ClaimTicketCommand
            {
                TicketId = id,
                CurrentUserId = userIdGuid,
                CurrentUserRole = role
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return result.StatusCode switch
                {
                    404 => NotFound(new { message = result.Message }),
                    400 => BadRequest(new { message = result.Message }),
                    403 => StatusCode(403, new { message = result.Message }),
                    _ => StatusCode(500, new { message = result.Message })
                };
            }

            return Ok(result);
        }

        [HttpPost("{id}/assign")]
        public async Task<IActionResult> AssignTicket(Guid id, [FromBody] AssignTicketCommand command)
        {
            var adminIdString = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role) ?? (User.IsInRole("Admin") ? "Admin" : User.IsInRole("Employee") ? "Employee" : string.Empty);

            if (string.IsNullOrEmpty(adminIdString) || !Guid.TryParse(adminIdString, out Guid adminIdGuid))
            {
                return Unauthorized(new { message = "Invalid Token Claims" });
            }

            command.TicketId = id;
            command.AdminId = adminIdGuid;
            command.AdminRole = role;

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return result.StatusCode switch
                {
                    404 => NotFound(new { message = result.Message }),
                    400 => BadRequest(new { message = result.Message }),
                    403 => StatusCode(403, new { message = result.Message }),
                    _ => StatusCode(500, new { message = result.Message })
                };
            }

            return Ok(result);
        }

        [HttpPost("{id}/takeover")]
        public async Task<IActionResult> TakeoverTicket(Guid id, [FromBody] TakeoverTicketCommand command)
        {
            var adminIdString = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role) ?? (User.IsInRole("Admin") ? "Admin" : User.IsInRole("Employee") ? "Employee" : string.Empty);

            if (string.IsNullOrEmpty(adminIdString) || !Guid.TryParse(adminIdString, out Guid adminIdGuid))
            {
                return Unauthorized(new { message = "Invalid Token Claims" });
            }

            command.TicketId = id;
            command.AdminId = adminIdGuid;
            command.AdminRole = role;

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return result.StatusCode switch
                {
                    404 => NotFound(new { message = result.Message }),
                    400 => BadRequest(new { message = result.Message }),
                    403 => StatusCode(403, new { message = result.Message }),
                    _ => StatusCode(500, new { message = result.Message })
                };
            }

            return Ok(result);
        }

        [HttpPost("{id}/dispute-decision")]
        public async Task<IActionResult> SubmitDisputeDecision(Guid id, [FromBody] DisputeDecisionCommand command)
        {
            var adminIdString = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role) ?? (User.IsInRole("Admin") ? "Admin" : User.IsInRole("Employee") ? "Employee" : string.Empty);

            if (string.IsNullOrEmpty(adminIdString) || !Guid.TryParse(adminIdString, out Guid adminIdGuid))
            {
                return Unauthorized(new { message = "Invalid Token Claims" });
            }

            command.TicketId = id;
            command.AdminId = adminIdGuid;
            command.AdminRole = role;

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return result.StatusCode switch
                {
                    404 => NotFound(new { message = result.Message }),
                    400 => BadRequest(new { message = result.Message }),
                    403 => StatusCode(403, new { message = result.Message }),
                    _ => StatusCode(500, new { message = result.Message })
                };
            }

            return Ok(result);
        }
    }
}
