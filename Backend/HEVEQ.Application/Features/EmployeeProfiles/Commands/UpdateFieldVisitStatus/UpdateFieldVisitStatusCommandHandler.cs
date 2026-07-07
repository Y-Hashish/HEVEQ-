using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Common.Exceptions;
using HEVEQ.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HEVEQ.Application.Features.EmployeeProfiles.Commands.UpdateFieldVisitStatus
{
    public class UpdateFieldVisitStatusCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
        : IRequestHandler<UpdateFieldVisitStatusCommand, UpdateFieldVisitStatusResponse>
    {
        public async Task<UpdateFieldVisitStatusResponse> Handle(UpdateFieldVisitStatusCommand request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.UserId ?? throw new ForbiddenAccessException("User is not authenticated.");
            var userRole = currentUserService.Role;

            if (userRole != "Employee" && userRole != "Admin")
            {
                return new UpdateFieldVisitStatusResponse { IsSuccess = false, StatusCode = 403, Message = "Only assigned field employees can update visit status." };
            }

            var visit = await context.FieldVerificationForms
                .FirstOrDefaultAsync(v => v.Id == request.FieldVisitId, cancellationToken);

            if (visit == null)
            {
                return new UpdateFieldVisitStatusResponse { IsSuccess = false, StatusCode = 404, Message = "Field visit not found." };
            }

            if (userRole == "Employee" && visit.DispatchedEmployeeId != userId)
            {
                return new UpdateFieldVisitStatusResponse { IsSuccess = false, StatusCode = 403, Message = "You are not authorized to update this field visit status." };
            }

            if (Enum.TryParse<VisitStatus>(request.Status, true, out var parsedStatus))
            {
                visit.VisitStatus = parsedStatus;
                
                if (parsedStatus == VisitStatus.OnSite && !visit.VisitedAt.HasValue)
                {
                    visit.VisitedAt = DateTime.UtcNow;
                }
            }
            else
            {
                return new UpdateFieldVisitStatusResponse { IsSuccess = false, StatusCode = 400, Message = $"Invalid visit status: {request.Status}." };
            }

            await context.SaveChangesAsync(cancellationToken);

            return new UpdateFieldVisitStatusResponse
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = $"Field visit status successfully updated to {parsedStatus}."
            };
        }
    }
}
