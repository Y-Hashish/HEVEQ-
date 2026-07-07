namespace HEVEQ.Application.Common.Realtime
{
    public class RealtimeMessageDto
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string? Body { get; set; }
        public string MessageType { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}