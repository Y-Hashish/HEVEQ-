using HEVEQ.Application.Features.Admin.DTOs;
using MediatR;
using System;
using System.Text.Json.Serialization;

namespace HEVEQ.Application.Features.Admin.Command.DisputeDecision
{
    public class DisputeDecisionCommand : IRequest<TicketActionResponse>
    {
        [JsonIgnore] public Guid TicketId { get; set; }
        [JsonIgnore] public Guid AdminId { get; set; }
        [JsonIgnore] public string AdminRole { get; set; } = string.Empty;

        public string DecisionType { get; set; } = string.Empty;
        public string DecisionNote { get; set; } = string.Empty;
        public decimal CustomerAmount { get; set; }
        public decimal ProviderAmount { get; set; }
        public Guid? EmployeeId { get; set; } // Required only for SendFieldVerification dispatch flow
    }
}
