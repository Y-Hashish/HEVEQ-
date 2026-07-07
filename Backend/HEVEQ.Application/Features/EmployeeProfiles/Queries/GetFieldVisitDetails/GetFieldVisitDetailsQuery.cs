using HEVEQ.Application.Features.Admin.DTOs;
using MediatR;
using System;

namespace HEVEQ.Application.Features.EmployeeProfiles.Queries.GetFieldVisitDetails
{
    public record GetFieldVisitDetailsQuery(Guid FieldVisitId) : IRequest<FieldVisitDetailsDto>;
}
