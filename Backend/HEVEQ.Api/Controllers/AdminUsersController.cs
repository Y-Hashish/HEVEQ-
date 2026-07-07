using HEVEQ.Application.Features.Admin.Command.UpdateUserStatus;
using HEVEQ.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using HEVEQ.Application.Features.Admin.Query.GetAdminUsers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace HEVEQ.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController(IMediator _mediator, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager) : ControllerBase
    {
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] GetAdminUsersQuery query)
        {
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        [HttpPost("admins")]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) ||
                string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "من فضلك املأ بيانات الأدمن المطلوبة" });
            }

            if (await userManager.FindByEmailAsync(request.Email) is not null)
            {
                return BadRequest(new { message = "البريد الإلكتروني مستخدم بالفعل" });
            }

            if (await userManager.FindByNameAsync(request.UserName) is not null)
            {
                return BadRequest(new { message = "اسم المستخدم مستخدم بالفعل" });
            }

            var user = new ApplicationUser
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                UserName = request.UserName.Trim(),
                Email = request.Email.Trim(),
                PhoneNumber = request.PhoneNumber,
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                return BadRequest(new { message = string.Join(" - ", createResult.Errors.Select(e => e.Description)) });
            }

            const string adminRole = "Admin";
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(adminRole));
            }

            await userManager.AddToRoleAsync(user, adminRole);

            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, new
            {
                id = user.Id,
                user.FirstName,
                user.LastName,
                user.UserName,
                user.Email,
                user.PhoneNumber,
                role = adminRole,
                message = "تم إنشاء حساب الأدمن بنجاح"
            });
        }

        [HttpPatch("users/{id}/status")]
        public async Task<IActionResult> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusCommand command)
        {
            var adminIdString = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminIdString) || !Guid.TryParse(adminIdString, out Guid adminIdGuid))
            {
                return Unauthorized(new { message = "بيانات الدخول غير صالحة" });
            }

            command.TargetUserId = id;
            command.AdminId = adminIdGuid;

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return result.StatusCode switch
                {
                    400 => BadRequest(new { message = result.Message }),
                    404 => NotFound(new { message = result.Message }),
                    _ => StatusCode(500, new { message = result.Message })
                };
            }

            return Ok(new
            {
                id = result.Id,
                isActive = result.IsActive,
                statusText = result.StatusText,
                statusAr = result.StatusAr,
                message = result.Message
            });
        }
    }

    public class CreateAdminUserRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
    }
}
