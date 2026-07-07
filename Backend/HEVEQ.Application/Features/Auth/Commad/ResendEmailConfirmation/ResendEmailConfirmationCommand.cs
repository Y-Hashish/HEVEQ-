using HEVEQ.Application.Features.Auth.DTOs;
using MediatR;

namespace HEVEQ.Application.Features.Auth.Commad.ResendEmailConfirmation;

public record ResendEmailConfirmationCommand(string Email) : IRequest<AuthResponse>;