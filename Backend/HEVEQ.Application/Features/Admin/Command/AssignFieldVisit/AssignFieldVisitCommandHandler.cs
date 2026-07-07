using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Domain.Enums;
using HEVEQ.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HEVEQ.Application.Common.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HEVEQ.Application.Features.Admin.Command.AssignFieldVisit
{
    public class AssignFieldVisitCommandHandler(
        IApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        NotificationHelper notificationHelper)
        : IRequestHandler<AssignFieldVisitCommand, AssignFieldVisitResponse>
    {
        public async Task<AssignFieldVisitResponse> Handle(AssignFieldVisitCommand request, CancellationToken cancellationToken)
        {
            var visit = await context.FieldVerificationForms
                .Include(v => v.Booking)
                .FirstOrDefaultAsync(v => v.Id == request.FieldVisitId, cancellationToken);

            if (visit == null)
            {
                return new AssignFieldVisitResponse { IsSuccess = false, StatusCode = 404, Message = "Field visit not found." };
            }

            var employee = await userManager.Users
                .Include(u => u.EmployeeProfile)
                .FirstOrDefaultAsync(u => u.Id == request.EmployeeUserId, cancellationToken);

            if (employee == null)
            {
                return new AssignFieldVisitResponse { IsSuccess = false, StatusCode = 404, Message = "Employee not found." };
            }

            var isEmployeeRole = await userManager.IsInRoleAsync(employee, "Employee");
            if (!isEmployeeRole)
            {
                return new AssignFieldVisitResponse { IsSuccess = false, StatusCode = 400, Message = "Selected user does not have the Employee role." };
            }

            if (employee.EmployeeProfile == null || !employee.EmployeeProfile.IsAvailableForDispatch)
            {
                return new AssignFieldVisitResponse { IsSuccess = false, StatusCode = 400, Message = "Employee is currently not available for dispatch." };
            }

            visit.DispatchedEmployeeId = employee.Id;
            visit.DispatchedByAdminId = request.AdminId;
            visit.VisitStatus = VisitStatus.Dispatched; // reset status to dispatched upon new assignment

            notificationHelper.FieldVerificationAssigned(employee.Id, visit.Id, visit.Booking?.BookingNumber ?? "N/A");
            await context.SaveChangesAsync(cancellationToken);

            return new AssignFieldVisitResponse
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Field visit successfully reassigned."
            };
        }
    }
}
