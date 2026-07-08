//using HEVEQ.Application.Common.AI.Models.Search;
//using HEVEQ.Application.Features.Search.SearchServices.Queries;
//using MediatR;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;

//namespace HEVEQ.Api.Controllers
//{
//    [ApiController]
//    [Route("api/public/search")]
//    public sealed class SearchController : ControllerBase
//    {
//        private readonly ISender _sender;

//        public SearchController(ISender sender)
//        {
//            _sender = sender;
//        }

//        /// <summary>
//        /// Semantic AI Search utilizing Qdrant vector distance and Semantic Kernel.
//        /// [AllowAnonymous] because customers can search before logging in.
//        /// </summary>
//        //[HttpGet("ai-search")]
//        //[AllowAnonymous]
//        //public async Task<ActionResult<SearchServicesResult>> AISearch([FromQuery] SearchServicesQuery query, CancellationToken ct)
//        //{

//        //    var result = await _sender.Send(query, ct);
//        //    return Ok(result);


//        //}

//        [HttpPost("/api/public/ai-search")] 
//        [AllowAnonymous]
//        public async Task<ActionResult<SearchServicesResult>> AISearch([FromBody] SearchServicesQuery query, CancellationToken ct)
//        {
//            var result = await _sender.Send(query, ct);
//            return Ok(result);

//        }
//    }
//}



using HEVEQ.Application.Common.AI.Models;
using HEVEQ.Application.Features.Search.SearchServices.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HEVEQ.Api.Controllers;

public sealed record SearchRequestDto(
    string RawQuery,
    List<ConversationTurnDto>? ConversationHistory,
    string? SessionId);

public sealed record ConversationTurnDto(string Role, string Content);

[ApiController]
[Route("api/public/search")]
public sealed class SearchController : ControllerBase
{
    private readonly ISender _sender;

    public SearchController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("/api/public/ai-search")]
    [AllowAnonymous]
    public async Task<ActionResult> AISearch([FromBody] SearchRequestDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RawQuery))
            return BadRequest(new { message = "من فضلك اكتب ما تريد البحث عنه." });

        // فك الـ History
        var history = request.ConversationHistory?
            .Select(t => new ConversationTurn(t.Role, t.Content))
            .ToList() ?? [];

  
        Guid? userId = null;
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdClaim, out var parsedId))
            userId = parsedId;

 
        var query = new SearchServicesQuery(
            RawQuery: request.RawQuery,
            ConversationHistory: history,
            RequestingUserId: userId, 
            SessionId: request.SessionId
        );

        var result = await _sender.Send(query, ct);
        return Ok(result);
    }
}