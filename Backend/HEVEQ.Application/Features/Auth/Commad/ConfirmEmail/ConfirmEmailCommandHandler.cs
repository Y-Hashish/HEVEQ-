using HEVEQ.Application.Features.Auth.DTOs;
using HEVEQ.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace HEVEQ.Application.Features.Auth.Commad.ConfirmEmail;

public class ConfirmEmailCommandHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<ConfirmEmailCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            return new AuthResponse
            {
                Succeeded = false,
                IsEmailConfirmed = false,
                Message = "Invalid email confirmation link."
            };
        }

        var roles = await userManager.GetRolesAsync(user);
        if (user.EmailConfirmed)
        {
            return new AuthResponse
            {
                Succeeded = true,
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                DisplayName = user.FirstName + " " + user.LastName,
                Roles = roles.ToList(),
                IsEmailConfirmed = true,
                Message = "Email is already confirmed. You can login now."
            };
        }

        var result = await userManager.ConfirmEmailAsync(user, request.Token);
        if (!result.Succeeded)
        {
            var errors = string.Join(" ,", result.Errors.Select(error => error.Description));

            return new AuthResponse
            {
                Succeeded = false,
                IsEmailConfirmed = false,
                Message = string.IsNullOrWhiteSpace(errors) ? "Invalid email confirmation link." : errors
            };
        }

        return new AuthResponse
        {
            Succeeded = true,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            DisplayName = user.FirstName + " " + user.LastName,
            Roles = roles.ToList(),
            IsEmailConfirmed = true,
            Message = "Email confirmed successfully. You can login now."
        };
    }
}