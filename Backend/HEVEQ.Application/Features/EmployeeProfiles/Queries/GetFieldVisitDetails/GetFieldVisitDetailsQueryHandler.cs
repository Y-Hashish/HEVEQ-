using HEVEQ.Application.Common.Exceptions;
using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Features.Admin.DTOs;
using HEVEQ.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HEVEQ.Application.Features.EmployeeProfiles.Queries.GetFieldVisitDetails
{
    public class GetFieldVisitDetailsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetFieldVisitDetailsQuery, FieldVisitDetailsDto>
    {
        public async Task<FieldVisitDetailsDto> Handle(GetFieldVisitDetailsQuery request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.UserId ?? throw new ForbiddenAccessException("User is not authenticated.");
            var userRole = currentUserService.Role;

            if (userRole != "Employee" && userRole != "Admin")
            {
                throw new ForbiddenAccessException("Only field employees are permitted to access this workspace.");
            }

            var visit = await context.FieldVerificationForms
                .Include(v => v.Photos)
                .Include(v => v.DispatchedEmployee)
                .FirstOrDefaultAsync(v => v.Id == request.FieldVisitId, cancellationToken);

            if (visit == null)
            {
                throw new NotFoundException("FieldVerificationForm", request.FieldVisitId);
            }

            // Ensure the employee is the one assigned to this visit
            if (userRole == "Employee" && visit.DispatchedEmployeeId != userId)
            {
                throw new ForbiddenAccessException("You are not authorized to view this field visit.");
            }

            return new FieldVisitDetailsDto
            {
                Id = visit.Id,
                DispatchedEmployeeId = visit.DispatchedEmployeeId,
                DispatchedEmployeeName = visit.DispatchedEmployee != null 
                    ? $"{visit.DispatchedEmployee.FirstName} {visit.DispatchedEmployee.LastName}".Trim()
                    : "Employee",
                DispatchInstructions = visit.DispatchInstructions,
                VisitStatus = visit.VisitStatus.ToString(),
                VisitStatusAr = MapVisitStatusAr(visit.VisitStatus),
                EmployeeNotes = visit.EmployeeNotes,
                FieldVerificationOutcome = visit.FieldVerificationOutcome?.ToString(),
                AdminDecision = visit.AdminDecision.ToString(),
                AdminDecisionNote = visit.AdminDecisionNote,
                DispatchedAt = visit.DispatchedAt,
                VisitedAt = visit.VisitedAt,
                Photos = visit.Photos.Select(p => new FieldVisitPhotoDto
                {
                    Id = p.Id,
                    PhotoUrl = p.PhotoUrl,
                    Caption = p.Caption
                }).ToList()
            };
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
