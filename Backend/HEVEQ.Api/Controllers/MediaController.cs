using HEVEQ.Application.Features.Media.Commands.UploadImage;
using HEVEQ.Application.Features.Media.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HEVEQ.Api.Requests.Media;

namespace HEVEQ.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/media")]
    public class MediaController(IMediator mediator) : ControllerBase
    {
        [HttpPost("images")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ImageUploadResponse>> UploadImage([FromForm] UploadImageFormRequest request,CancellationToken cancellationToken)
        {
            if (request.File is null || request.File.Length == 0)
            {
                return BadRequest(new{ message = "Image file is required." });
            }

            await using var stream = request.File.OpenReadStream();

            var result = await mediator.Send(new UploadImageCommand(stream,request.File.FileName,request.File.ContentType,request.File.Length,request.Purpose,request.ReferenceId),cancellationToken);
            return Ok(result);
        }
    }
}