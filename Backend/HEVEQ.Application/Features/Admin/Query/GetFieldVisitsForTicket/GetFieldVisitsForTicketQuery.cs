using HEVEQ.Application.Features.Admin.DTOs;
using MediatR;
using System;
using System.Collections.Generic;

namespace HEVEQ.Application.Features.Admin.Query.GetFieldVisitsForTicket
{
    public record GetFieldVisitsForTicketQuery(Guid TicketId) : IRequest<List<FieldVisitDetailsDto>>;
}
