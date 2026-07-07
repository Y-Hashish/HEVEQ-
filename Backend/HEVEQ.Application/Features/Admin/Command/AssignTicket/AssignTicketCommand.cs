using HEVEQ.Application.Features.Admin.DTOs;
using MediatR;
using System;
using System.Text.Json.Serialization;

namespace HEVEQ.Application.Features.Admin.Command.AssignTicket
{
    public class AssignTicketCommand : IRequest<TicketActionResponse>
    {
        [JsonIgnore] public Guid TicketId { get; set; }
        [JsonIgnore] public Guid AdminId { get; set; }
        [JsonIgnore] public string AdminRole { get; set; } = string.Empty;

        public Guid AssignedToUserId { get; set; }
    }
}
