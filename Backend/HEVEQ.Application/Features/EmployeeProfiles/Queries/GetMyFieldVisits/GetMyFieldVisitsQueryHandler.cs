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

namespace HEVEQ.Application.Features.EmployeeProfiles.Queries.GetMyFieldVisits
{
    public class GetMyFieldVisitsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetMyFieldVisitsQuery, List<FieldVisitDetailsDto>>
    {
        public async Task<List<FieldVisitDetailsDto>> Handle(GetMyFieldVisitsQuery request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.UserId ?? throw new ForbiddenAccessException("User is not authenticated.");
            var userRole = currentUserService.Role;

            if (userRole != "Employee" && userRole != "Admin")
            {
                throw new ForbiddenAccessException("Only field employees are permitted to access this workspace.");
            }

            var visits = await context.FieldVerificationForms
                .Include(v => v.Photos)
                .Include(v => v.DispatchedEmployee)
                .Where(v => v.DispatchedEmployeeId == userId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return visits.Select(v => new FieldVisitDetailsDto
            {
                Id = v.Id,
                DispatchedEmployeeId = v.DispatchedEmployeeId,
                DispatchedEmployeeName = v.DispatchedEmployee != null 
                    ? $"{v.DispatchedEmployee.FirstName} {v.DispatchedEmployee.LastName}".Trim()
                    : "Employee",
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
