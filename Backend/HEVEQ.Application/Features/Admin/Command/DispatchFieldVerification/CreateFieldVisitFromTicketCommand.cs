using HEVEQ.Application.Features.Admin.DTOs;
using MediatR;
using System;
using System.Text.Json.Serialization;

namespace HEVEQ.Application.Features.Admin.Command.DispatchFieldVerification
{
    public class CreateFieldVisitFromTicketCommand : IRequest<DispatchFieldVerificationResponse>
    {
        [JsonIgnore]
        public Guid TicketId { get; set; }
        public Guid EmployeeUserId { get; set; }
        public string DispatchInstructions { get; set; }
        [JsonIgnore]
        public Guid AdminId { get; set; }
    }
}
