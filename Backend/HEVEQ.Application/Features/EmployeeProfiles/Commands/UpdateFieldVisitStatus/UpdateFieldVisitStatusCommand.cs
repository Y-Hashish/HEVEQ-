using MediatR;
using System;
using System.Text.Json.Serialization;

namespace HEVEQ.Application.Features.EmployeeProfiles.Commands.UpdateFieldVisitStatus
{
    public class UpdateFieldVisitStatusCommand : IRequest<UpdateFieldVisitStatusResponse>
    {
        [JsonIgnore]
        public Guid FieldVisitId { get; set; }
        public string Status { get; set; } = string.Empty; // Dispatched, OnSite, Completed, FailedAccess
    }

    public class UpdateFieldVisitStatusResponse
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
