using HEVEQ.Application.Features.Auth.DTOs;
using MediatR;

namespace HEVEQ.Application.Features.Auth.Commad.ConfirmEmail;

public record ConfirmEmailCommand(Guid UserId, string Token) : IRequest<AuthResponse>;