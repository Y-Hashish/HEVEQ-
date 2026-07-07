using HEVEQ.Application.Common.Exceptions;
using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Features.Admin.DTOs;
using HEVEQ.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HEVEQ.Application.Features.Admin.Query.GetFieldVisitsForTicket
{
    public class GetFieldVisitsForTicketQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetFieldVisitsForTicketQuery, List<FieldVisitDetailsDto>>
    {
        public async Task<List<FieldVisitDetailsDto>> Handle(GetFieldVisitsForTicketQuery request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.UserId ?? throw new ForbiddenAccessException("User is not authenticated.");
            var userRole = currentUserService.Role;

            if (userRole != "Admin" && userRole != "Employee")
            {
                throw new ForbiddenAccessException("You do not have access to this resource.");
            }

            var ticket = await context.Tickets
                .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);

            if (ticket == null)
            {
                throw new NotFoundException("Ticket", request.TicketId);
            }

            // Employee Access check
            if (userRole == "Employee")
            {
                if (ticket.AssignedToUserId.HasValue && ticket.AssignedToUserId.Value != userId)
                {
                    throw new ForbiddenAccessException("You do not have access to this ticket.");
                }
            }

            var visitsQuery = context.FieldVerificationForms
                .Include(v => v.Photos)
                .Include(v => v.DispatchedEmployee)
                .AsNoTracking();

            if (ticket.BookingId.HasValue)
            {
                visitsQuery = visitsQuery.Where(v => v.BookingId == ticket.BookingId.Value || (v.TicketId.HasValue && v.TicketId.Value == ticket.Id));
            }
            else
            {
                visitsQuery = visitsQuery.Where(v => v.TicketId.HasValue && v.TicketId.Value == ticket.Id);
            }

            var visits = await visitsQuery.ToListAsync(cancellationToken);

            return visits.Select(v => new FieldVisitDetailsDto
            {
                Id = v.Id,
                DispatchedEmployeeId = v.DispatchedEmployeeId,
                DispatchedEmployeeName = v.DispatchedEmployee != null 
                    ? $"{v.DispatchedEmployee.FirstName} {v.DispatchedEmployee.LastName}".Trim()
                    : "Unknown Employee",
                DispatchInstructions = v.DispatchInstructions,
                VisitStatus = v.VisitStatus.ToString(),
                VisitStatusAr = MapVisitStatusAr(v.VisitStatus),
                EmployeeNotes = v.EmployeeNotes,
                FieldVerificationOutcome = v.FieldVerificationOutcome?.ToString(),
                AdminDecision = v.AdminDecision.ToString(),
                AdminDecisionNote = v.AdminDecisionNote,
                DispatchedAt = v.DispatchedAt,
                VisitedAt = v.VisitedAt,
                Photos = v.Photos.Select(p => new FieldVisitPhotoDto
                {
                    Id = p.Id,
                    PhotoUrl = p.PhotoUrl,
                    Caption = p.Caption
                }).ToList()
            }).ToList();
        }

        private static string MapVisitStatusAr(VisitStatus status)
        {
            return status switch
            {
                VisitStatus.Dispatched => "تم الإرسال",
                VisitStatus.OnSite => "في الموقع",
                VisitStatus.Completed => "تم الانتهاء",
                VisitStatus.FailedAccess => "فشل الدخول",
                _ => status.ToString()
            };
        }
    }
}
