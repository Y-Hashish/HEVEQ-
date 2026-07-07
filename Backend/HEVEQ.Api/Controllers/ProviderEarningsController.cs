using HEVEQ.Application.Features.MarketPlaceOrders.DTOs;
using HEVEQ.Application.Features.MarketPlaceOrders.Queries.GetMarketplaceEarningsSummary;
using HEVEQ.Application.Features.ProviderEarnings.DTOs;
using HEVEQ.Application.Features.ProviderEarnings.Queries.GetProviderEarningsServiceSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HEVEQ.Api.Controllers;

[ApiController]
[Authorize(Roles = "Provider")]
[Route("api/provider/earnings")]
public class ProviderEarningsController(ISender sender) : ControllerBase
{
    [HttpGet("service-summary")]
    [ProducesResponseType(typeof(ProviderEarningsServiceSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProviderEarningsServiceSummaryDto>> GetServiceSummary(
        [FromQuery] DateOnly from, [FromQuery] DateOnly to)
    {
        var result = await sender.Send(new GetProviderEarningsServiceSummaryQuery(from, to));
        return Ok(result);
    }

    [HttpGet("marketplace-summary")]
    [ProducesResponseType(typeof(MarketplaceEarningsSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MarketplaceEarningsSummaryDto>> GetMarketplaceSummary(
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to)
    {
        var result = await sender.Send(new GetMarketplaceEarningsSummaryQuery
        {
            From = from,
            To = to
        });

        return Ok(result);
    }
}
