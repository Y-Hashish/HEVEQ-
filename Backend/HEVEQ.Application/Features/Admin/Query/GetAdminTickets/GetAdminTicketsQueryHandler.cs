using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Features.Admin.DTOs;
using HEVEQ.Domain.Enums;
using HEVEQ.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using HEVEQ.Application.Common.Exceptions;

namespace HEVEQ.Application.Features.Admin.Query.GetAdminTickets
{
    public class GetAdminTicketsQueryHandler(
        IApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetAdminTicketsQuery, PaginatedAdminTicketsResponse>
    {
        public async Task<PaginatedAdminTicketsResponse> Handle(GetAdminTicketsQuery request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.UserId ?? throw new ForbiddenAccessException("User is not authenticated.");
            var userRole = currentUserService.Role;

            if (userRole != "Admin" && userRole != "Employee")
            {
                throw new ForbiddenAccessException("You do not have access to this resource.");
            }

            var query = context.Tickets.AsNoTracking();

            if (userRole == "Employee")
            {
                query = query.Where(t => t.AssignedToUserId == userId || t.AssignedToUserId == null);
            }

            if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<TicketStatus>(request.Status, true, out var parsedStatus))
            {
                query = query.Where(t => t.Status == parsedStatus);
            }

            if (!string.IsNullOrEmpty(request.Category) && Enum.TryParse<TicketCategory>(request.Category, true, out var parsedCategory))
            {
                query = query.Where(t => t.Category == parsedCategory);
            }

            if (!string.IsNullOrEmpty(request.Priority))
            {
                int priorityInt = request.Priority.ToLower() switch
                {
                    "urgent" => 3,
                    "high" => 2,
                    "medium" => 1,
                    "low" => 0,
                    _ => -1
                };
                if (priorityInt >= 0)
                {
                    query = query.Where(t => t.Priority == priorityInt);
                }
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var pagedData = await query
                .OrderByDescending(t => t.CreatedAt) 
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(t => new
                {
                    t.Id,
                    t.TicketNumber,
                    t.Subject,
                    Category = t.Category.ToString(),
                    Status = t.Status.ToString(),
                    t.SubmittedById, 
                    t.Priority,
                    t.CreatedAt
                })
                .ToListAsync(cancellationToken);

            if (!pagedData.Any())
            {
                return new PaginatedAdminTicketsResponse { TotalCount = totalCount, Page = request.Page, PageSize = request.PageSize };
            }

            // 4. جلب أسماء المستخدمين من Identity
            var userIds = pagedData.Select(x => x.SubmittedById).Distinct().ToList();
            var usersDict = await userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}".Trim(), cancellationToken);

            // 5. تجميع النتيجة النهائية وتعريب الحالة
            var items = pagedData.Select(t => new AdminTicketDto
            {
                Id = t.Id,
                TicketNumber = t.TicketNumber,
                Subject = t.Subject,
                Title = t.Subject, // Frontend compatibility
                Category = t.Category,
                Status = t.Status,
                StatusAr = GetArabicStatus(t.Status),
                SubmittedByName = usersDict.GetValueOrDefault(t.SubmittedById, "Unknown User"),
                UserName = usersDict.GetValueOrDefault(t.SubmittedById, "Unknown User"), // Frontend compatibility
                Priority = GetPriorityString(t.Priority),
                CreatedAt = t.CreatedAt
            }).ToList();

            return new PaginatedAdminTicketsResponse
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        private static string GetArabicStatus(string status)
        {
            return status switch
            {
                "Open" => "مفتوحة",
                "InProgress" => "قيد المعالجة",
                "Resolved" => "محلولة",
                "Closed" => "مغلقة",
                _ => status
            };
        }

        private static string GetPriorityString(int priority)
        {
            return priority switch
            {
                3 => "Urgent",
                2 => "High",
                1 => "Medium",
                0 => "Low",
                _ => "Medium"
            };
        }
    }
}
