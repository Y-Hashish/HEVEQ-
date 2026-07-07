using HEVEQ.Application.Common.AI.Interfaces;
using HEVEQ.Application.Common.AI.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HEVEQ.Infrastructure.Services.AI.ModerationAgent
{
    internal class GptVisionDocumentExtractor(Kernel kernel) : IDocumentVisionExtractor
    {
        private static readonly string[] ExpiryKeyHints = { "expiry", "validuntil", "expirydate" };

        public async Task<DocumentExtractionResult> ExtractAsync(
            Guid documentId, Stream fileStream, string mimeType, string documentType, CancellationToken ct = default)
        {
            using var ms = new MemoryStream();
            await fileStream.CopyToAsync(ms, ct);
            var bytes = ms.ToArray();

            var chat = kernel.GetRequiredService<IChatCompletionService>();

            var history = new ChatHistory();
            history.AddSystemMessage(
                "You are an OCR extraction AI for an equipment rental platform in Egypt. " +
                "Extract structured data from document images. " +
                "Respond ONLY with valid JSON — no markdown, no explanation. " +
                "Analyze both Arabic and English text in the document, " +
                "but always write the values of 'failureReason' and 'adminNote' in Arabic.");

            var userMessage = new ChatMessageContentItemCollection
            {
                new TextContent(BuildPromptForDocumentType(documentType)),
                new ImageContent(new ReadOnlyMemory<byte>(bytes), mimeType)
            };
            history.Add(new ChatMessageContent(AuthorRole.User, userMessage));

            var settings = new OpenAIPromptExecutionSettings { ResponseFormat = "json_object" };

            ChatMessageContent response;
            try
            {
                response = await chat.GetChatMessageContentAsync(history, settings, kernel, ct);
            }
            catch (Exception)
            {
                return Failure(documentId, "تعذّر الوصول إلى خدمة تحليل المستندات بالذكاء الاصطناعي. يُرجى المحاولة مرة أخرى لاحقًا.");
            }

            RawExtractionResponse? raw;
            try
            {
                raw = JsonSerializer.Deserialize<RawExtractionResponse>(response.Content ?? "{}");
            }
            catch (JsonException)
            {
                return Failure(documentId, "تعذّر تحليل رد الذكاء الاصطناعي. يُرجى المحاولة مرة أخرى.");
            }

            if (raw is null)
                return Failure(documentId, "لم يُرجع الذكاء الاصطناعي أي بيانات. يُرجى المحاولة مرة أخرى.");

            var fields = ExtractDynamicFields(raw.ExtraFields);
            var expiryDate = ResolveExpiryDate(fields);

            return new DocumentExtractionResult(
                DocumentId: documentId,
                IsReadable: raw.IsReadable,
                ConfidenceScore: raw.ConfidenceScore,
                KeyFieldsPresent: raw.KeyFieldsPresent,
                ExpiryDate: expiryDate,
                ExtractedFields: fields,
                FailureReason: raw.FailureReason,
                AdminNote: raw.AdminNote);
        }

        private static Dictionary<string, string> ExtractDynamicFields(Dictionary<string, JsonElement>? extra)
        {
            var fields = new Dictionary<string, string>();
            if (extra is null)
                return fields;

            foreach (var (name, value) in extra)
            {
                if (value.ValueKind != JsonValueKind.String)
                    continue;

                var str = value.GetString();
                if (!string.IsNullOrWhiteSpace(str))
                    fields[name] = str;
            }

            return fields;
        }

        private static DateOnly? ResolveExpiryDate(Dictionary<string, string> fields)
        {
            var expiryKey = fields.Keys.FirstOrDefault(k =>
                ExpiryKeyHints.Any(hint => k.Contains(hint, StringComparison.OrdinalIgnoreCase)));

            return expiryKey is not null && DateOnly.TryParse(fields[expiryKey], out var parsed)
                ? parsed
                : null;
        }

        private static DocumentExtractionResult Failure(Guid documentId, string reasonAr) => new(
            DocumentId: documentId,
            IsReadable: false,
            ConfidenceScore: 0,
            KeyFieldsPresent: false,
            ExpiryDate: null,
            ExtractedFields: new Dictionary<string, string>(),
            FailureReason: reasonAr,
            AdminNote: null);

        /// <summary>Shape of the AI's JSON response. Fixed fields are strongly typed;
        /// everything else (document-type-specific fields) lands in <see cref="ExtraFields"/>.</summary>
        private sealed class RawExtractionResponse
        {
            [JsonPropertyName("isReadable")] public bool IsReadable { get; set; }
            [JsonPropertyName("confidenceScore")] public decimal ConfidenceScore { get; set; }
            [JsonPropertyName("keyFieldsPresent")] public bool KeyFieldsPresent { get; set; }
            [JsonPropertyName("failureReason")] public string? FailureReason { get; set; }
            [JsonPropertyName("adminNote")] public string? AdminNote { get; set; }

            [JsonExtensionData] public Dictionary<string, JsonElement>? ExtraFields { get; set; }
        }

        private static string BuildPromptForDocumentType(string documentType) => documentType switch
        {
            "NationalId" => """
        استخرج البيانات من بطاقة الرقم القومي المصرية وقيّم مصداقيتها.

        أعد هذا JSON بالضبط:
        {
          "fullName": "الاسم الكامل بالعربي أو null",
          "nationalId": "رقم قومي مكون من 14 رقم أو null",
          "dateOfBirth": "YYYY-MM-DD أو null",
          "gender": "Male | Female | null",
          "governorate": "اسم المحافظة أو null",
          "expiryDate": "YYYY-MM-DD أو null",
          "isReadable": true | false,
          "confidenceScore": 0.0-1.0,
          "keyFieldsPresent": true | false,
          "failureReason": "سبب عدم القراءة أو null",
          "adminNote": "ملخص للأدمن بالعربي أو null"
        }

        keyFieldsPresent: true فقط إذا تم استخراج fullName و nationalId معاً.
        adminNote: اكتب ملخصاً واحداً للأدمن يشمل:
        - هل المستند واضح وقابل للقراءة؟
        - هل توجد علامات تلاعب أو تعديل رقمي؟
        - هل البيانات متسقة (تاريخ الميلاد مع رقم الهيكل مثلاً)؟
        - أي ملاحظات مهمة يجب أن يعرفها الأدمن.
        اكتب null إذا كان المستند نظيفاً ولا توجد ملاحظات.
        """,

            "CommercialRegistration" => """
        استخرج البيانات من السجل التجاري المصري وقيّم مصداقيته.

        أعد هذا JSON بالضبط:
        {
          "companyName": "اسم الشركة أو null",
          "registrationNumber": "رقم السجل التجاري أو null",
          "issueDate": "YYYY-MM-DD أو null",
          "expiryDate": "YYYY-MM-DD أو null",
          "legalForm": "الشكل القانوني أو null",
          "activity": "النشاط التجاري أو null",
          "isReadable": true | false,
          "confidenceScore": 0.0-1.0,
          "keyFieldsPresent": true | false,
          "failureReason": "سبب عدم القراءة أو null",
          "adminNote": "ملخص للأدمن بالعربي أو null"
        }

        keyFieldsPresent: true فقط إذا تم استخراج companyName و registrationNumber معاً.
        adminNote: اكتب ملخصاً واحداً للأدمن يشمل:
        - هل المستند واضح وأختام الجهة الرسمية واضحة؟
        - هل توجد علامات تلاعب أو تعديل في التواريخ أو الأرقام؟
        - هل النشاط التجاري مناسب لمنصة تأجير المعدات؟
        - أي ملاحظات مهمة للأدمن.
        اكتب null إذا كان المستند نظيفاً.
        """,

            "TaxCard" => """
        استخرج البيانات من البطاقة الضريبية المصرية وقيّم مصداقيتها.

        أعد هذا JSON بالضبط:
        {
          "taxId": "الرقم الضريبي أو null",
          "companyName": "اسم الشركة أو الفرد أو null",
          "issueDate": "YYYY-MM-DD أو null",
          "expiryDate": "YYYY-MM-DD أو null",
          "isReadable": true | false,
          "confidenceScore": 0.0-1.0,
          "keyFieldsPresent": true | false,
          "failureReason": "سبب عدم القراءة أو null",
          "adminNote": "ملخص للأدمن بالعربي أو null"
        }

        keyFieldsPresent: true فقط إذا تم استخراج taxId و companyName معاً.
        adminNote: ملخص للأدمن: هل المستند واضح؟ توجد علامات تلاعب؟ أي ملاحظات مهمة؟ null إذا نظيف.
        """,

            "EquipmentLicense" => """
        استخرج البيانات من رخصة تشغيل المعدة وقيّم مصداقيتها.

        أعد هذا JSON بالضبط:
        {
          "licenseNumber": "رقم الرخصة أو null",
          "equipmentType": "نوع المعدة أو null",
          "licensedOperator": "اسم المشغل المرخص أو null",
          "issuingAuthority": "جهة الإصدار أو null",
          "issueDate": "YYYY-MM-DD أو null",
          "expiryDate": "YYYY-MM-DD أو null",
          "isReadable": true | false,
          "confidenceScore": 0.0-1.0,
          "keyFieldsPresent": true | false,
          "failureReason": "سبب عدم القراءة أو null",
          "adminNote": "ملخص للأدمن بالعربي أو null"
        }

        keyFieldsPresent: true فقط إذا تم استخراج licenseNumber و expiryDate معاً.
        adminNote: ملخص للأدمن: هل الرخصة واضحة؟ جهة الإصدار معروفة؟ توجد علامات تلاعب في التواريخ؟ null إذا نظيفة.
        """,

            "OperatorLicense" => """
        استخرج البيانات من رخصة القيادة وقيّم مصداقيتها.

        أعد هذا JSON بالضبط:
        {
          "fullName": "اسم حامل الرخصة أو null",
          "nationalId": "رقم قومي إن وُجد أو null",
          "licenseNumber": "رقم الرخصة أو null",
          "licenseType": "فئة الرخصة أو null",
          "issueDate": "YYYY-MM-DD أو null",
          "expiryDate": "YYYY-MM-DD أو null",
          "isReadable": true | false,
          "confidenceScore": 0.0-1.0,
          "keyFieldsPresent": true | false,
          "failureReason": "سبب عدم القراءة أو null",
          "adminNote": "ملخص للأدمن بالعربي أو null"
        }

        keyFieldsPresent: true فقط إذا تم استخراج fullName و expiryDate معاً.
        adminNote: ملخص للأدمن: هل الرخصة واضحة؟ الصورة متطابقة؟ توجد علامات تلاعب في تاريخ الانتهاء؟ الفئة مناسبة لتشغيل معدات ثقيلة؟ null إذا نظيفة.
        """,

            "Insurance" => """
        استخرج البيانات من وثيقة التأمين وقيّم مصداقيتها.

        أعد هذا JSON بالضبط:
        {
          "policyNumber": "رقم الوثيقة أو null",
          "insuredName": "اسم المؤمَّن عليه أو null",
          "coverageType": "نوع التغطية أو null",
          "insurer": "شركة التأمين أو null",
          "startDate": "YYYY-MM-DD أو null",
          "expiryDate": "YYYY-MM-DD أو null",
          "isReadable": true | false,
          "confidenceScore": 0.0-1.0,
          "keyFieldsPresent": true | false,
          "failureReason": "سبب عدم القراءة أو null",
          "adminNote": "ملخص للأدمن بالعربي أو null"
        }

        keyFieldsPresent: true فقط إذا تم استخراج policyNumber و expiryDate معاً.
        adminNote: ملخص للأدمن: شركة التأمين معروفة؟ التغطية كافية لمنصة معدات؟ توجد علامات تلاعب؟ null إذا نظيفة.
        """,

            _ => """
        استخرج أي بيانات منظمة من هذا المستند وقيّم مصداقيته.

        أعد هذا JSON بالضبط:
        {
          "documentTitle": "عنوان المستند أو نوعه أو null",
          "primaryName": "الاسم الرئيسي في المستند أو null",
          "referenceNumber": "أي رقم مرجعي أو null",
          "issueDate": "YYYY-MM-DD أو null",
          "expiryDate": "YYYY-MM-DD أو null",
          "isReadable": true | false,
          "confidenceScore": 0.0-1.0,
          "keyFieldsPresent": true | false,
          "failureReason": "سبب عدم القراءة أو null",
          "adminNote": "ملخص للأدمن بالعربي أو null"
        }

        keyFieldsPresent: true فقط إذا تم استخراج primaryName و referenceNumber معاً.
        adminNote: ملخص للأدمن: ما نوع المستند؟ هل يبدو موثوقاً؟ أي ملاحظات مهمة؟ null إذا نظيف.
        """
        };
    }
}