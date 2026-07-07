using HEVEQ.Application.Features.Admin.DTOs;
using MediatR;
using System;
using System.Text.Json.Serialization;

namespace HEVEQ.Application.Features.Admin.Command.ClaimTicket
{
    public class ClaimTicketCommand : IRequest<TicketActionResponse>
    {
        [JsonIgnore] public Guid TicketId { get; set; }
        [JsonIgnore] public Guid CurrentUserId { get; set; }
        [JsonIgnore] public string CurrentUserRole { get; set; } = string.Empty;
    }
}
