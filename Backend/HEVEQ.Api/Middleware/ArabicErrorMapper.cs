namespace HEVEQ.Api.Middleware;

public static class ArabicErrorMapper
{
    private static readonly Dictionary<string, string> Exact = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Invalid Refresh Token"] = "جلسة الدخول غير صالحة.",
        ["Logged out successfully"] = "تم تسجيل الخروج بنجاح.",
        ["User not found"] = "المستخدم غير موجود.",
        ["Invalid Token Claims"] = "بيانات جلسة الدخول غير صحيحة."
    };

    public static string ToArabic(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return "حدث خطأ غير متوقع، حاول مرة أخرى.";

        var value = message.Trim();
        if (value.Any(c => c >= '\u0600' && c <= '\u06FF'))
            return value;

        if (Exact.TryGetValue(value.TrimEnd('.'), out var exact))
            return exact;

        var lower = value.ToLowerInvariant();

        if (lower.Contains("booking time is outside service listing availability"))
            return "وقت الحجز خارج مواعيد الإتاحة لهذه الخدمة. اختر وقتاً يسمح بانتهاء الحجز قبل نهاية وقت العمل.";
        if (lower.Contains("no availability exists for the selected day"))
            return "اليوم المحدد غير متاح لهذه الخدمة. اختر يوماً من أيام الإتاحة المعروضة.";
        if (lower.Contains("booking is not allowed on a blackout date"))
            return "لا يمكن الحجز في هذا اليوم لأنه غير متاح لدى المزود.";
        if (lower.Contains("minimum booking duration"))
            return "مدة الحجز أقل من الحد الأدنى المسموح لهذه الخدمة.";
        if (lower.Contains("booking is allowed only for active service listings"))
            return "لا يمكن حجز هذه الخدمة قبل اعتمادها وتفعيلها.";
        if (lower.Contains("service listing hourly rate is not configured"))
            return "سعر الساعة غير محدد لهذه الخدمة.";
        if (lower.Contains("provider service zone is not configured"))
            return "نطاق خدمة المزود غير مكتمل، يرجى اختيار خدمة أخرى أو التواصل مع الدعم.";
        if (lower.Contains("booking location is outside provider service zone"))
            return "موقع الحجز خارج نطاق خدمة المزود. يجب الموافقة على التكلفة الإضافية خارج النطاق.";
        if (lower.Contains("selected address was not found"))
            return "العنوان المحدد غير موجود أو غير تابع لحسابك.";
        if (lower.Contains("selected address must have latitude and longitude"))
            return "العنوان المحدد لا يحتوي على إحداثيات الموقع. يرجى تحديث العنوان أو إدخال الموقع يدوياً.";
        if (lower.Contains("governorate and district are required"))
            return "يرجى إدخال المحافظة والمنطقة عند عدم استخدام عنوان محفوظ.";
        if (lower.Contains("latitude and longitude are required"))
            return "يرجى إدخال إحداثيات الموقع عند عدم استخدام عنوان محفوظ.";
        if (lower.Contains("not found")) return "العنصر المطلوب غير موجود.";
        if (lower.Contains("unauthorized") || lower.Contains("not authorized") || lower.Contains("forbidden")) return "ليس لديك صلاحية لتنفيذ هذا الإجراء.";
        if (lower.Contains("required")) return "يرجى استكمال جميع الحقول المطلوبة.";
        if (lower.Contains("invalid")) return "البيانات المدخلة غير صحيحة.";
        if (lower.Contains("already")) return "هذا الإجراء تم تنفيذه من قبل أو أن البيانات موجودة مسبقاً.";
        if (lower.Contains("cannot")) return "لا يمكن تنفيذ هذا الإجراء بالحالة الحالية.";
        if (lower.Contains("minimum requirements")) return "لا يمكن الموافقة لأن البيانات المطلوبة غير مكتملة.";
        if (lower.Contains("active held escrow") || lower.Contains("escrow")) return "لا يوجد سجل ضمان مالي نشط لهذا الطلب.";
        if (lower.Contains("unexpected error")) return "حدث خطأ غير متوقع، حاول مرة أخرى.";

        return "حدث خطأ غير متوقع، حاول مرة أخرى.";
    }

    public static object TranslatePayload(object? payload)
    {
        if (payload == null)
            return new { message = ToArabic(null) };

        if (payload is string text)
            return new { message = ToArabic(text) };

        var type = payload.GetType();
        var values = type.GetProperties().ToDictionary(p => p.Name, p => p.GetValue(payload));

        if (values.TryGetValue("message", out var message) && message is string msg)
            values["message"] = ToArabic(msg);

        if (values.TryGetValue("Message", out var messageUpper) && messageUpper is string msgUpper)
            values["Message"] = ToArabic(msgUpper);

        if (values.TryGetValue("title", out var title) && title is string t)
            values["title"] = ToArabic(t);

        if (values.TryGetValue("detail", out var detail) && detail is string d)
            values["detail"] = ToArabic(d);

        if (values.TryGetValue("errors", out var errors) && errors is IDictionary<string, string[]> dict)
            values["errors"] = dict.ToDictionary(x => x.Key, x => x.Value.Select(ToArabic).ToArray());

        if (values.TryGetValue("Errors", out var errorsUpper) && errorsUpper is IDictionary<string, string[]> dictUpper)
            values["Errors"] = dictUpper.ToDictionary(x => x.Key, x => x.Value.Select(ToArabic).ToArray());

        return values;
    }
}
