using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Features.Auth.DTOs;
using HEVEQ.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace HEVEQ.Application.Features.Auth.Commad.ResendEmailConfirmation;

public class ResendEmailConfirmationCommandHandler(UserManager<ApplicationUser> userManager, IAccountEmailConfirmationService accountEmailConfirmationService) : IRequestHandler<ResendEmailConfirmationCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(ResendEmailConfirmationCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null)
            return new AuthResponse { Message = "User not found." };

        var roles = await userManager.GetRolesAsync(user);

        if (user.EmailConfirmed)
        {
            return new AuthResponse
            {
                Succeeded = true,
                UserId = user.Id,
                Email = user.Email ?? request.Email,
                UserName = user.UserName ?? string.Empty,
                DisplayName = user.FirstName + " " + user.LastName,
                Roles = roles.ToList(),
                IsEmailConfirmed = true,
                Message = "Email is already confirmed. You can login now."
            };
        }

        await accountEmailConfirmationService.SendConfirmationEmailAsync(user, cancellationToken);

        return new AuthResponse
        {
            Succeeded = true,
            UserId = user.Id,
            Email = user.Email ?? request.Email,
            UserName = user.UserName ?? string.Empty,
            DisplayName = user.FirstName + " " + user.LastName,
            Roles = roles.ToList(),
            IsEmailConfirmed = false,
            RequiresEmailConfirmation = true,
            EmailConfirmationSent = true,
            Message = "Confirmation email has been sent again."
        };
    }
}