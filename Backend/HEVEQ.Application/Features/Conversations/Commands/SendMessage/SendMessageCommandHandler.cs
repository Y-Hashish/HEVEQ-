using MediatR;
using Microsoft.EntityFrameworkCore;
using HEVEQ.Application.Common.Exceptions;
using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Features.Conversations.DTOs;
using HEVEQ.Domain.Entities;
using HEVEQ.Application.Common.Services;
using HEVEQ.Application.Common.Realtime;

namespace HEVEQ.Application.Features.Conversations.Commands.SendMessage;

public class SendMessageCommandHandler
    : IRequestHandler<SendMessageCommand, SendMessageResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly NotificationHelper _notificationHelper;
    private readonly IRealtimeEventPublisher _realtimeEventPublisher;

    public SendMessageCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        NotificationHelper notificationHelper,
        IRealtimeEventPublisher realtimeEventPublisher)
    {
        _context = context;
        _currentUser = currentUser;
        _notificationHelper = notificationHelper;
        _realtimeEventPublisher = realtimeEventPublisher;
    }

    public async Task<SendMessageResult> Handle(
        SendMessageCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("User is not authenticated.");

        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken)
            ?? throw new NotFoundException("Conversation", request.ConversationId);

        // Business Rule: user must be a participant
        if (conversation.InitiatedById != userId && conversation.ParticipantId != userId)
            throw new ForbiddenAccessException(
                "You are not a participant in this conversation.");

        // Business Rule: locked conversations do not accept new messages
        if (conversation.IsLocked)
            throw new BadRequestException(
                "This conversation is locked and cannot receive new messages.");

        var message = new Message
        {
            ConversationId = conversation.Id,
            SenderId = userId,
            Content = request.Body,
            MessageType = request.MessageType,
            IsBlocked = false,           // contact-info check done in validator
            SentAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        var receiverId = conversation.InitiatedById == userId ? conversation.ParticipantId : conversation.InitiatedById;

        _notificationHelper.NewMessageReceived(receiverId, message.Id, "مستخدم");
        await _context.SaveChangesAsync(cancellationToken);

        var realtimeMessage = new RealtimeMessageDto
        {
            Id = message.Id,
            ConversationId = conversation.Id,
            SenderId = userId,
            SenderName = "مستخدم",
            Body = message.Content,
            MessageType = message.MessageType.ToString(),
            SentAt = message.SentAt
        };

        await _realtimeEventPublisher.SendMessageToUserAsync(receiverId, realtimeMessage, cancellationToken);
        await _realtimeEventPublisher.SendMessageToUserAsync(userId, realtimeMessage, cancellationToken);

        return new SendMessageResult(message.Id, "Message sent successfully");
    }
}