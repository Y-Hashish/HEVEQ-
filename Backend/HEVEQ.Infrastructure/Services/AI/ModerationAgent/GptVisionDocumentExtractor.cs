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
            var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3));

            history.AddSystemMessage(
                $$"""
You are an OCR extraction and document validation AI for HEVEQ, an equipment rental platform in Egypt.
Your job is to read the uploaded document image, extract structured data, validate the extracted data, and write a clear Arabic admin note.
Current date in Egypt is: {{today:yyyy-MM-dd}}
Return ONLY valid JSON.
Do not return markdown.
Do not return explanations outside the JSON.
Do not wrap the JSON in code fences.
The JSON must be flat, not nested, because the backend reads dynamic fields from the root JSON object.
General OCR rules:
- Analyze Arabic and English text.
- Extract only data that is visible in the document.
- Do not invent or guess missing data.
- If a field is missing, unreadable, unclear, cropped, or not present, return null.
- Convert Arabic digits to English digits before returning numbers or dates.
- Normalize all dates to yyyy-MM-dd.
- Egyptian documents usually use day-month-year format.
- If a date is ambiguous, interpret it as day-month-year.
- If a date is impossible, invalid, unreadable, or has an unrealistic year, return null for that date and mention the issue in Arabic inside failureReason and adminNote.
- Always use the exact field name expiryDate for any expiry, expiration, valid until, valid to, end date, or صلاحية حتى date.
- If no expiry date exists in the document, return "expiryDate": null.

Validation rules:
- Set isReadable to false if the image is blurry, too dark, cropped, very low quality, unreadable, or the main text cannot be understood.
- Set confidenceScore from 0.0 to 1.0 based on OCR clarity and certainty.
- Set keyFieldsPresent to true only if the main required fields for this document type were extracted clearly.
- If important data is missing or unreadable, explain exactly what is missing in Arabic.
- If the document has signs of editing, manipulation, inconsistent fonts, overwritten numbers, suspicious layout, cropped important areas, or mismatched data, mention that clearly in Arabic.
- If no issue is detected, failureReason must be null.
- If any issue is detected, failureReason must be a clear Arabic sentence explaining the issue.

Expiry validation rules:
- Compare expiryDate with the current date in Egypt: {{today:yyyy-MM-dd}}.
- If expiryDate is earlier than {{today:yyyy-MM-dd}}, the document is expired.
- If expiryDate is equal to or later than {{today:yyyy-MM-dd}}, the document is not expired.
- If expiryDate is within 30 days from {{today:yyyy-MM-dd}}, mention that it is close to expiry.
- If expiryDate is null, mention in adminNote that no expiry date was found or it was unreadable, if expiry is expected for this document type.

Logical validation rules:
- issueDate must not be after expiryDate.
- dateOfBirth must not be in the future.
- For Egyptian National ID:
  - nationalId must be exactly 14 digits.
  - If nationalId is visible but not 14 digits, add this issue in Arabic.
  - If nationalId is 14 digits, try to derive the birth date from it.
  - If visible dateOfBirth exists, compare it with the birth date derived from nationalId.
  - If there is a mismatch, mention it clearly in Arabic.
- For licenses and insurance documents:
  - The license or policy number must be visible if present in the document.
  - The expiryDate is important and must be extracted if visible.
- For commercial registration and tax card:
  - Extract companyName and registration or tax number if visible.
  - Mention if the business activity is missing, unclear, or not related to equipment, contracting, construction, transport, rental, or heavy machinery.

Admin note rules:
- adminNote must always be written in Arabic.
- adminNote must never be null.
- adminNote must be helpful for the admin.
- adminNote must summarize:
  1. Whether the document is readable.
  2. The most important extracted fields.
  3. The expiry date comparison with today's date if expiryDate exists.
  4. Any missing, unclear, illogical, suspicious, or expired data.
  5. If there are no problems, clearly say that no issues were detected.

Admin note examples:
- If valid:
  "المستند واضح وقابل للقراءة. تم استخراج البيانات الأساسية بنجاح. تم استخراج تاريخ انتهاء المستند 2026-08-10 وتمت مقارنته بتاريخ اليوم {{today:yyyy-MM-dd}}، لذلك المستند غير منتهي. لا توجد مشكلات واضحة في البيانات المستخرجة."
- If expired:
  "المستند واضح وقابل للقراءة. تم استخراج تاريخ انتهاء المستند 2025-08-10 وتمت مقارنته بتاريخ اليوم {{today:yyyy-MM-dd}}، لذلك المستند منتهي ويحتاج إلى مراجعة أو إعادة رفع مستند سارٍ."
- If missing expiry:
  "المستند قابل للقراءة جزئيًا، لكن لم يتم العثور على تاريخ انتهاء واضح في المستند، لذلك يحتاج إلى مراجعة يدوية للتأكد من الصلاحية."
- If unclear:
  "صورة المستند غير واضحة بدرجة كافية، وبعض البيانات الأساسية غير قابلة للقراءة، لذلك يحتاج المستند إلى إعادة رفع صورة أوضح أو مراجعة يدوية."

Important JSON rules:
- The response must include isReadable, confidenceScore, keyFieldsPresent, failureReason, and adminNote.
- The response must include expiryDate if the document type can have an expiry date.
- confidenceScore must be a number between 0.0 and 1.0.
- isReadable and keyFieldsPresent must be booleans.
- failureReason must be null if no issue exists.
- adminNote must be Arabic text in all cases.
""");

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
Return this exact flat JSON shape:
{
  "fullName": "Arabic full name or null",
  "nationalId": "14 digit Egyptian national ID or null",
  "dateOfBirth": "YYYY-MM-DD or null",
  "birthDateFromNationalId": "YYYY-MM-DD or null",
  "gender": "Male | Female | null",
  "governorate": "Governorate name or null",
  "address": "Address or null",
  "jobTitle": "Job title or null",
  "expiryDate": "YYYY-MM-DD or null",
  "isReadable": true,
  "confidenceScore": 0.0,
  "keyFieldsPresent": true,
  "failureReason": null,
  "adminNote": "Arabic admin note"
}

Required fields for keyFieldsPresent:
- fullName
- nationalId
- expiryDate

Special validation:
- nationalId must be exactly 14 digits.
- Extract birthDateFromNationalId if nationalId is valid.
- Compare birthDateFromNationalId with dateOfBirth if both are available.
- Compare expiryDate with the current date.
""",

            "CommercialRegistration" => """
Return this exact flat JSON shape:
{
  "companyName": "Company name or null",
  "registrationNumber": "Commercial registration number or null",
  "issueDate": "YYYY-MM-DD or null",
  "expiryDate": "YYYY-MM-DD or null",
  "legalForm": "Legal form or null",
  "activity": "Commercial activity or null",
  "issuingAuthority": "Issuing authority or null",
  "isReadable": true,
  "confidenceScore": 0.0,
  "keyFieldsPresent": true,
  "failureReason": null,
  "adminNote": "Arabic admin note"
}

Required fields for keyFieldsPresent:
- companyName
- registrationNumber
- expiryDate

Special validation:
- Check whether the activity appears related to equipment, heavy machinery, contracting, construction, transport, rental, operation, maintenance, or similar business.
- Compare expiryDate with the current date.
""",

            "TaxCard" => """
Return this exact flat JSON shape:
{
  "taxId": "Tax number or null",
  "companyName": "Company or person name or null",
  "issueDate": "YYYY-MM-DD or null",
  "expiryDate": "YYYY-MM-DD or null",
  "activity": "Tax activity or null",
  "taxOffice": "Tax office or null",
  "isReadable": true,
  "confidenceScore": 0.0,
  "keyFieldsPresent": true,
  "failureReason": null,
  "adminNote": "Arabic admin note"
}

Required fields for keyFieldsPresent:
- taxId
- companyName

Special validation:
- Mention if expiryDate is not visible or not applicable.
- Mention if the tax number is unreadable, incomplete, or suspicious.
""",

            "EquipmentLicense" => """
Return this exact flat JSON shape:
{
  "licenseNumber": "License number or null",
  "equipmentType": "Equipment type or null",
  "equipmentPlateNumber": "Equipment plate or serial number or null",
  "licensedOperator": "Licensed operator name or null",
  "issuingAuthority": "Issuing authority or null",
  "issueDate": "YYYY-MM-DD or null",
  "expiryDate": "YYYY-MM-DD or null",
  "isReadable": true,
  "confidenceScore": 0.0,
  "keyFieldsPresent": true,
  "failureReason": null,
  "adminNote": "Arabic admin note"
}

Required fields for keyFieldsPresent:
- licenseNumber
- equipmentType
- expiryDate

Special validation:
- Compare expiryDate with the current date.
- Mention if the equipment type, license number, or expiry date is missing or unclear.
- Mention any sign of edited numbers, overwritten dates, suspicious text, or cropped areas.
""",

            "OperatorLicense" => """
Return this exact flat JSON shape:
{
  "fullName": "License holder name or null",
  "nationalId": "National ID if visible or null",
  "licenseNumber": "License number or null",
  "licenseType": "License type or category or null",
  "issueDate": "YYYY-MM-DD or null",
  "expiryDate": "YYYY-MM-DD or null",
  "issuingAuthority": "Issuing authority or null",
  "isReadable": true,
  "confidenceScore": 0.0,
  "keyFieldsPresent": true,
  "failureReason": null,
  "adminNote": "Arabic admin note"
}

Required fields for keyFieldsPresent:
- fullName
- licenseNumber
- licenseType
- expiryDate

Special validation:
- Compare expiryDate with the current date.
- Mention whether the license type appears suitable for operating heavy equipment if this can be inferred.
- If nationalId exists, validate that it is 14 digits.
""",

            "Insurance" => """
Return this exact flat JSON shape:
{
  "policyNumber": "Insurance policy number or null",
  "insuredName": "Insured name or null",
  "coverageType": "Coverage type or null",
  "insurer": "Insurance company name or null",
  "startDate": "YYYY-MM-DD or null",
  "expiryDate": "YYYY-MM-DD or null",
  "coveredEquipment": "Covered equipment or null",
  "isReadable": true,
  "confidenceScore": 0.0,
  "keyFieldsPresent": true,
  "failureReason": null,
  "adminNote": "Arabic admin note"
}

Required fields for keyFieldsPresent:
- policyNumber
- insuredName
- expiryDate

Special validation:
- Compare expiryDate with the current date.
- Mention whether the coverage type appears related to equipment, vehicles, machinery, liability, or operational risks if visible.
- Mention if coverage type is missing or unclear.
""",

            _ => """
Return this exact flat JSON shape:
{
  "documentTitle": "Document title or type or null",
  "primaryName": "Main name in the document or null",
  "referenceNumber": "Reference number or null",
  "issueDate": "YYYY-MM-DD or null",
  "expiryDate": "YYYY-MM-DD or null",
  "issuingAuthority": "Issuing authority or null",
  "isReadable": true,
  "confidenceScore": 0.0,
  "keyFieldsPresent": true,
  "failureReason": null,
  "adminNote": "Arabic admin note"
}

Required fields for keyFieldsPresent:
- documentTitle or referenceNumber
- primaryName if visible
- expiryDate if the document clearly has an expiry date

Special validation:
- Identify the document type if possible.
- Compare expiryDate with the current date if found.
- Mention missing, unclear, suspicious, or illogical data.
"""
        };
    }
}