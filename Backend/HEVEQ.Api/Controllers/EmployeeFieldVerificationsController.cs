using HEVEQ.Application.Features.EmployeeProfiles.Queries.GetMyFieldVisits;
using HEVEQ.Application.Features.EmployeeProfiles.Queries.GetFieldVisitDetails;
using HEVEQ.Application.Features.EmployeeProfiles.Commands.SubmitFieldVisitEvidence;
using HEVEQ.Application.Features.EmployeeProfiles.Commands.UpdateFieldVisitStatus;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace HEVEQ.Api.Controllers
{
    [Route("api/employee/field-visits")]
    [ApiController]
    [Authorize(Roles = "Employee,Admin")]
    public class EmployeeFieldVerificationsController(IMediator _mediator) : ControllerBase
    {
        [HttpGet("my")]
        public async Task<IActionResult> GetMyFieldVisits()
        {
            var query = new GetMyFieldVisitsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{fieldVisitId:guid}")]
        public async Task<IActionResult> GetFieldVisitDetails(Guid fieldVisitId)
        {
            var query = new GetFieldVisitDetailsQuery(fieldVisitId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("{fieldVisitId:guid}/evidence")]
        public async Task<IActionResult> SubmitFieldVisitEvidence(Guid fieldVisitId, [FromBody] SubmitFieldVisitEvidenceCommand command)
        {
            command.FieldVisitId = fieldVisitId;
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return result.StatusCode switch
                {
                    404 => NotFound(new { message = result.Message }),
                    403 => StatusCode(StatusCodes.Status403Forbidden, new { message = result.Message }),
                    _ => BadRequest(new { message = result.Message })
                };
            }

            return Ok(result);
        }

        [HttpPatch("{fieldVisitId:guid}/status")]
        public async Task<IActionResult> UpdateFieldVisitStatus(Guid fieldVisitId, [FromBody] UpdateFieldVisitStatusCommand command)
        {
            command.FieldVisitId = fieldVisitId;
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return result.StatusCode switch
                {
                    404 => NotFound(new { message = result.Message }),
                    403 => StatusCode(StatusCodes.Status403Forbidden, new { message = result.Message }),
                    _ => BadRequest(new { message = result.Message })
                };
            }

            return Ok(result);
        }
    }
}
