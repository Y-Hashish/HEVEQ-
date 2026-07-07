using HEVEQ.Application.Features.Admin.DTOs;
using MediatR;
using System;

namespace HEVEQ.Application.Features.Admin.Query.GetTicketDecisionContext
{
    public record GetTicketDecisionContextQuery(Guid TicketId) : IRequest<TicketDecisionContextDto>;
}
