using HEVEQ.Application.Features.Admin.DTOs;
using MediatR;
using System;
using System.Collections.Generic;

namespace HEVEQ.Application.Features.EmployeeProfiles.Queries.GetMyFieldVisits
{
    public record GetMyFieldVisitsQuery : IRequest<List<FieldVisitDetailsDto>>;
}
