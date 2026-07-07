using HEVEQ.Domain.Identity;

namespace HEVEQ.Application.Common.Interfaces;
public interface IAccountEmailConfirmationService
{
    Task SendConfirmationEmailAsync(ApplicationUser user, CancellationToken cancellationToken = default);
}