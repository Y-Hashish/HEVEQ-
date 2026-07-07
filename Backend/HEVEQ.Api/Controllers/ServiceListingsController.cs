using HEVEQ.Application.Common.AI.Models.Search;
using HEVEQ.Application.Common.Persistence.Interfaces;
using HEVEQ.Application.Features.Search.SearchServices.Queries;
using HEVEQ.Application.Features.ServiceListings.Commands;
using HEVEQ.Application.Features.ServiceListings.Commands.AddBlackoutDate;
using HEVEQ.Application.Features.ServiceListings.Commands.AddServiceListingAvailability;
using HEVEQ.Application.Features.ServiceListings.Commands.AddServiceListingPhoto;
using HEVEQ.Application.Features.ServiceListings.Commands.CreateServiceListing;
using HEVEQ.Application.Features.ServiceListings.Commands.DeleteBlackoutDate;
using HEVEQ.Application.Features.ServiceListings.Commands.DeleteServiceListing;
using HEVEQ.Application.Features.ServiceListings.Commands.DeleteServiceListingAvailability;
using HEVEQ.Application.Features.ServiceListings.Commands.DeleteServiceListingPhoto;
using HEVEQ.Application.Features.ServiceListings.Commands.LinkOperatorToListing;
using HEVEQ.Application.Features.ServiceListings.Commands.SubmitServiceListingForReview;
using HEVEQ.Application.Features.ServiceListings.Commands.UnlinkOperatorFromListing;
using HEVEQ.Application.Features.ServiceListings.Commands.UpdateServiceListing;
using HEVEQ.Application.Features.ServiceListings.Commands.UpdateServiceListingAvailability;
using HEVEQ.Application.Features.ServiceListings.DTOs;
using HEVEQ.Application.Features.ServiceListings.Queries;
using HEVEQ.Application.Features.ServiceListings.Queries.GetManageServiceListing;
using HEVEQ.Application.Features.ServiceListings.Queries.GetProviderServiceListings;
using HEVEQ.Application.Features.ServiceListings.Queries.PublicSearch;
using HEVEQ.Infrastructure.Persistence.Qdrant;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel.Embeddings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HEVEQ.Application.Common.AI.Models.Search;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel.Embeddings;

namespace HEVEQ.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/service-listings")]

public sealed class ServiceListingsController(
    ISender sender,
    ITextEmbeddingGenerationService embeddingService,
    IQdrantListingIndex qdrantListingIndex,
    IServiceListingReadRepository listingRepository) : ControllerBase
{
    // ---------- Core (Service Listings) ----------

    [HttpPost]
    [Authorize(Roles = "Provider")]
    public async Task<ActionResult> Create([FromBody] CreateServiceListingCommand command)
    {
        var result = await sender.Send(command);
        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = "Provider")]
    public async Task<ActionResult> GetApproved()
    {
        var result = await sender.Send(new GetApprovedServiceListingsQuery());
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Provider")]
    public async Task<ActionResult> GetById(Guid id)
    {
        var result = await sender.Send(new GetServiceListingByIdQuery(id));
        return Ok(result);
    }

    [HttpGet("/api/provider/service-listings")]
    [Authorize(Roles = "Provider")]
    public async Task<ActionResult> GetMine()
    {
        var result = await sender.Send(new GetProviderServiceListingsQuery());
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Provider")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateServiceListingCommand command)
    {
        if (id != command.Id) return BadRequest("Id mismatch");

        await sender.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Provider")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await sender.Send(new DeleteServiceListingCommand(id));
        return NoContent();
    }

    // ---------- Photos ----------

    [HttpPost("{id:guid}/photos")]
    [Authorize(Roles = "Provider")]
    public async Task<ActionResult<Guid>> AddPhoto(Guid id, [FromBody] AddServiceListingPhotoInput input)
    {
        var command = new AddServiceListingPhotoCommand(id, input.PhotoUrl, input.DisplayOrder);
        var photoId = await sender.Send(command);
        return Ok(photoId);
    }

    [HttpDelete("{id:guid}/photos/{photoId:guid}")]
    [Authorize(Roles = "Provider")]
    public async Task<IActionResult> DeletePhoto(Guid id, Guid photoId)
    {
        await sender.Send(new DeleteServiceListingPhotoCommand(id, photoId));
        return NoContent();
    }

    // ---------- Operators ----------

    [HttpPost("{id:guid}/operators")]
    [Authorize(Roles = "Provider")]
    public async Task<IActionResult> LinkOperator(Guid id, [FromBody] Guid operatorId)
    {
        var result = await sender.Send(new LinkOperatorToListingCommand(id, operatorId));
        return Ok(result);
    }

    [HttpDelete("{id:guid}/operators/{operatorId:guid}")]
    [Authorize(Roles = "Provider")]
    public async Task<IActionResult> UnlinkOperator(Guid id, Guid operatorId)
    {
        await sender.Send(new UnlinkOperatorFromListingCommand(id, operatorId));
        return NoContent();
    }

    // ---------- Availability ----------

    [HttpPost("{id:guid}/availability")]
    [Authorize(Roles = "Provider")]
    public async Task<ActionResult> AddAvailability(Guid id, [FromBody] AddAvailabilityInput input)
    {
        var command = new AddServiceListingAvailabilityCommand(id, input.DayOfWeek, input.OpenTime, input.CloseTime);
        var result = await sender.Send(command);
        return Ok(result);
    }

    [HttpPut("{id:guid}/availability/{availabilityId:guid}")]
    [Authorize(Roles = "Provider")]
    public async Task<IActionResult> UpdateAvailability(Guid id, Guid availabilityId, [FromBody] UpdateAvailabilityInput input)
    {
        var command = new UpdateServiceListingAvailabilityCommand(id, availabilityId, input.DayOfWeek, input.OpenTime, input.CloseTime);
        await sender.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}/availability/{availabilityId:guid}")]
    [Authorize(Roles = "Provider")]
    public async Task<IActionResult> DeleteAvailability(Guid id, Guid availabilityId)
    {
        await sender.Send(new DeleteServiceListingAvailabilityCommand(id, availabilityId));
        return NoContent();
    }

    // ---------- Blackout Dates ----------

    [HttpPost("{id:guid}/blackout-dates")]
    [Authorize(Roles = "Provider")]
    public async Task<ActionResult> AddBlackoutDate(Guid id, [FromBody] AddBlackoutInput input)
    {
        var command = new AddBlackoutDateCommand(id, input.Date, input.Reason, input.OperatorId);
        var result = await sender.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id:guid}/blackout-dates/{blackoutDateId:guid}")]
    [Authorize(Roles = "Provider")]
    public async Task<IActionResult> DeleteBlackoutDate(Guid id, Guid blackoutDateId)
    {
        await sender.Send(new DeleteBlackoutDateCommand(id, blackoutDateId));
        return NoContent();
    }

    // ---------- provider manage its listing ----------
    [HttpGet("/api/provider/service-listings/{id:guid}/manage")]
    [Authorize(Roles = "Provider")]
    public async Task<ActionResult> GetManageListing(Guid id, [FromHeader(Name = "Authorization")] string token)
    {
        var result = await sender.Send(new GetManageServiceListingQuery(id));
        return Ok(result);
    }

    [HttpPost("{id:guid}/submit-for-review")]
    [Authorize(Roles = "Provider")]
    public async Task<ActionResult> SubmitForReview(Guid id)
    {
        var result = await sender.Send(new SubmitServiceListingForReviewCommand(id));
        return Ok(result);
    }

    // ---------- Search Listings ----------
    [HttpGet("/api/public/search")]
    [AllowAnonymous]
    public async Task<ActionResult> PublicSearch([FromQuery] PublicSearchQuery query)
    {
        try
        {
            var result = await sender.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = "Caught an error during AI Search!",
                errorType = ex.GetType().Name,
                errorMessage = ex.Message,
                innerError = ex.InnerException?.Message,
                stackTrace = ex.StackTrace
            });
        }
    }

    
}


public record AddServiceListingPhotoInput(string PhotoUrl, int DisplayOrder);
public record AddAvailabilityInput(int DayOfWeek, TimeOnly OpenTime, TimeOnly CloseTime);
public record UpdateAvailabilityInput(int DayOfWeek, TimeOnly OpenTime, TimeOnly CloseTime);
public record AddBlackoutInput(DateOnly Date, string? Reason, Guid? OperatorId);