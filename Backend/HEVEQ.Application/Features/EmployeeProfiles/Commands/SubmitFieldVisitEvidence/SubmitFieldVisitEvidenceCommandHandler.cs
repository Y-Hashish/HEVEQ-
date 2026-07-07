using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Common.Exceptions;
using HEVEQ.Domain.Entities;
using HEVEQ.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HEVEQ.Application.Features.EmployeeProfiles.Commands.SubmitFieldVisitEvidence
{
    public class SubmitFieldVisitEvidenceCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
        : IRequestHandler<SubmitFieldVisitEvidenceCommand, SubmitFieldVisitEvidenceResponse>
    {
        public async Task<SubmitFieldVisitEvidenceResponse> Handle(SubmitFieldVisitEvidenceCommand request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.UserId ?? throw new ForbiddenAccessException("User is not authenticated.");
            var userRole = currentUserService.Role;

            if (userRole != "Employee" && userRole != "Admin")
            {
                return new SubmitFieldVisitEvidenceResponse { IsSuccess = false, StatusCode = 403, Message = "Only assigned field employees can submit evidence." };
            }

            var visit = await context.FieldVerificationForms
                .Include(v => v.Photos)
                .FirstOrDefaultAsync(v => v.Id == request.FieldVisitId, cancellationToken);

            if (visit == null)
            {
                return new SubmitFieldVisitEvidenceResponse { IsSuccess = false, StatusCode = 404, Message = "Field visit not found." };
            }

            if (userRole == "Employee" && visit.DispatchedEmployeeId != userId)
            {
                return new SubmitFieldVisitEvidenceResponse { IsSuccess = false, StatusCode = 403, Message = "You are not authorized to upload evidence for this field visit." };
            }

            // Map Outcome string to Enum
            if (Enum.TryParse<FieldVerificationOutcome>(request.Outcome, true, out var parsedOutcome))
            {
                visit.FieldVerificationOutcome = parsedOutcome;
            }
            else
            {
                visit.FieldVerificationOutcome = FieldVerificationOutcome.Inconclusive;
            }

            visit.EmployeeNotes = request.EmployeeNotes;
            visit.VisitStatus = VisitStatus.Completed;
            visit.VisitedAt = DateTime.UtcNow;
            visit.FormSubmittedAt = DateTime.UtcNow;

            // Clear old photos and add new ones
            context.FieldVerificationPhotos.RemoveRange(visit.Photos);
            visit.Photos.Clear();

            if (request.PhotoUrls != null && request.PhotoUrls.Any())
            {
                int order = 1;
                foreach (var url in request.PhotoUrls)
                {
                    visit.Photos.Add(new FieldVerificationPhoto
                    {
                        Id = Guid.NewGuid(),
                        FieldVerificationFormId = visit.Id,
                        PhotoUrl = url,
                        Caption = $"Evidence photo {order}",
                        DisplayOrder = order++,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await context.SaveChangesAsync(cancellationToken);

            return new SubmitFieldVisitEvidenceResponse
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Field visit evidence submitted successfully."
            };
        }
    }
}
