using MediatR;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HEVEQ.Application.Features.EmployeeProfiles.Commands.SubmitFieldVisitEvidence
{
    public class SubmitFieldVisitEvidenceCommand : IRequest<SubmitFieldVisitEvidenceResponse>
    {
        [JsonIgnore]
        public Guid FieldVisitId { get; set; }
        public string EmployeeNotes { get; set; } = string.Empty;
        public string Outcome { get; set; } = string.Empty; // JobConfirmed, JobNotConfirmed, PartiallyConfirmed, Inconclusive
        public List<string> PhotoUrls { get; set; } = new();
    }

    public class SubmitFieldVisitEvidenceResponse
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
