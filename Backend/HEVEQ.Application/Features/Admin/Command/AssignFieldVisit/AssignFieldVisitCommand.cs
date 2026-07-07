using MediatR;
using System;
using System.Text.Json.Serialization;

namespace HEVEQ.Application.Features.Admin.Command.AssignFieldVisit
{
    public class AssignFieldVisitCommand : IRequest<AssignFieldVisitResponse>
    {
        [JsonIgnore]
        public Guid FieldVisitId { get; set; }
        public Guid EmployeeUserId { get; set; }
        [JsonIgnore]
        public Guid AdminId { get; set; }
    }

    public class AssignFieldVisitResponse
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
