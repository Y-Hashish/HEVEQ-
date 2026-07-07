using HEVEQ.Application.Common.AI.Interfaces;
using HEVEQ.Application.Common.AI.Models;
using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Domain.Entities;
using HEVEQ.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace HEVEQ.Infrastructure.Services.AI.BackgroundJobs
{
    public class DocumentOcrJob(
        IApplicationDbContext context,
        IDocumentVisionExtractor extractor,
        IHttpClientFactory httpClientFactory,
        ILogger<DocumentOcrJob> logger)
    {
        private const decimal MinConfidenceScore = 0.7m;
        private const int ExpiryWarnDays = 30;

        public async Task RunAsync(Guid documentId, CancellationToken ct = default)
        {
            var document = await context.Documents.FirstOrDefaultAsync(d => d.Id == documentId, ct);
            if (document is null)
            {
                logger.LogWarning("DocumentOcrJob: Document {DocumentId} not found — skipping.", documentId);
                return;
            }

            Stream fileStream;
            string mimeType;
            try
            {
                (fileStream, mimeType) = await FetchFileAsync(document.FileUrl, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DocumentOcrJob: Failed to fetch file for document {DocumentId}.", documentId);
                document.AdminNote = "تعذّر جلب ملف المستند لمعالجته آليًا. يُرجى مراجعة رابط الملف أو إعادة رفعه.";
                await context.SaveChangesAsync(ct);
                return;
            }

            var sw = Stopwatch.StartNew();
            DocumentExtractionResult result;
            try
            {
                result = await extractor.ExtractAsync(
                    documentId, fileStream, mimeType, document.DocumentType.ToString(), ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DocumentOcrJob: AI extraction failed for document {DocumentId}.", documentId);
                throw;
            }
            finally
            {
                sw.Stop();
                await fileStream.DisposeAsync();
            }

            ApplyExtractionResult(document, result);

            if (!result.IsReadable || result.ConfidenceScore < MinConfidenceScore)
            {
                HandleLowConfidence(document, result);
                LogAiInteraction(document, sw.ElapsedMilliseconds);
                await context.SaveChangesAsync(ct);
                return;
            }

            HandleExpiryEvaluation(document);

            await context.SaveChangesAsync(ct);

            logger.LogInformation(
                "DocumentOcrJob: Document {DocumentId} processed. Confidence={Score:P0}, ExpiryStatus={ExpiryStatus}, KeyFields={Keys}",
                documentId, result.ConfidenceScore, document.ExpiryStatus, result.KeyFieldsPresent);
        }

        /// <summary>Writes the AI's raw output onto the document. Never touches FailureReason —
        /// that field is reserved exclusively for an admin's rejection reason.</summary>
        private static void ApplyExtractionResult(Document document, DocumentExtractionResult result)
        {
            document.ExtractedText = result.ExtractedFields.Count > 0
                ? string.Join("\n", result.ExtractedFields.Select(kv => $"{kv.Key}: {kv.Value}"))
                : null;

            document.ConfidenceScore = result.ConfidenceScore;
            document.KeyFieldsPresent = result.KeyFieldsPresent;

            document.AdminNote = !string.IsNullOrWhiteSpace(result.AdminNote)
                ? result.AdminNote
                : result.FailureReason;

            if (result.ExpiryDate.HasValue)
                document.ExpiryDate = result.ExpiryDate;
        }

        private void HandleLowConfidence(Document document, DocumentExtractionResult result)
        {
            document.ExpiryStatus = DocumentExpiryStatus.NotApplicable;

            var note = $"جودة صورة المستند منخفضة (نسبة الثقة: {result.ConfidenceScore:P0})، مما قد يستدعي إعادة رفع صورة أوضح.";
            if (string.IsNullOrWhiteSpace(document.AdminNote))
                document.AdminNote = note;

            NotifyProvider(document, "DocumentLowConfidence", "مطلوب إعادة رفع المستند", note);

            logger.LogWarning(
                "DocumentOcrJob: Document {DocumentId} low confidence ({Score:P0}) — provider notified.",
                document.Id, result.ConfidenceScore);
        }

        private void HandleExpiryEvaluation(Document document)
        {
            document.ExpiryStatus = EvaluateExpiry(document.ExpiryDate);

            switch (document.ExpiryStatus)
            {
                case DocumentExpiryStatus.Expired:
                    // Do NOT touch Status or FailureReason — Admin sees ExpiryStatus = Expired and decides.
                    NotifyProvider(document, "DocumentExpired", "انتهت صلاحية المستند",
                        $"انتهت صلاحية المستند بتاريخ {document.ExpiryDate:yyyy-MM-dd}.");
                    logger.LogWarning(
                        "DocumentOcrJob: Document {DocumentId} expired on {Date} — Admin notified via report.",
                        document.Id, document.ExpiryDate);
                    break;

                case DocumentExpiryStatus.ExpiringSoon:
                    NotifyProvider(document, "DocumentExpiringSoon", "المستند على وشك الانتهاء",
                        $"تنبيه: تنتهي صلاحية المستند بتاريخ {document.ExpiryDate:yyyy-MM-dd} (خلال {ExpiryWarnDays} يومًا).");
                    logger.LogInformation(
                        "DocumentOcrJob: Document {DocumentId} expiring soon ({Date}).",
                        document.Id, document.ExpiryDate);
                    break;

                case DocumentExpiryStatus.Valid:
                case DocumentExpiryStatus.NotApplicable:
                    break;
            }
        }

        private void NotifyProvider(Document document, string eventType, string title, string? body)
        {
            if (!document.UserId.HasValue)
                return;

            context.Notifications.Add(new Notification
            {
                UserId = document.UserId.Value,
                EventType = eventType,
                Title = title,
                Body = body,
                ReferenceId = document.Id.ToString(),
                ReferenceType = nameof(Document),
                Channel = NotificationChannel.InApp,
                IsRead = false,
                SentAt = DateTime.UtcNow
            });
        }

        private void LogAiInteraction(Document document, long latencyMs)
        {
            context.AiInteractionLogs.Add(new AiInteractionLog
            {
                AgentType = AiAgentType.Ocr,
                InvocationContext = $"Document.OCR.{document.DocumentType}",
                EntityType = nameof(Document),
                EntityId = document.Id,
                AiRecommendation = document.ExpiryStatus?.ToString() ?? "Processed",
                AiRiskScore = null,
                LatencyMs = (int)latencyMs,
                CreatedAt = DateTime.UtcNow
            });
        }

        private static DocumentExpiryStatus EvaluateExpiry(DateOnly? expiryDate)
        {
            if (!expiryDate.HasValue)
                return DocumentExpiryStatus.NotApplicable;

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var daysLeft = expiryDate.Value.DayNumber - today.DayNumber;

            return daysLeft < 0 ? DocumentExpiryStatus.Expired
                : daysLeft <= ExpiryWarnDays ? DocumentExpiryStatus.ExpiringSoon
                : DocumentExpiryStatus.Valid;
        }

        private async Task<(Stream stream, string mimeType)> FetchFileAsync(string fileUrl, CancellationToken ct)
        {
            var client = httpClientFactory.CreateClient();
            var response = await client.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();

            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/pdf";
            var stream = await response.Content.ReadAsStreamAsync(ct);
            return (stream, contentType);
        }
    }
}