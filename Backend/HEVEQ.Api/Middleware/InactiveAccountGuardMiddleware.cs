using System.Security.Claims;
using System.Text.Json;
using HEVEQ.Domain.Enums;
using HEVEQ.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HEVEQ.Api.Middleware;

public sealed class InactiveAccountGuardMiddleware
{
    private readonly RequestDelegate _next;

    public InactiveAccountGuardMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
    {
        if (!ShouldCheck(context))
        {
            await _next(context);
            return;
        }

        var userIdText = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? context.User.FindFirstValue("uid");

        if (!Guid.TryParse(userIdText, out var userId))
        {
            await _next(context);
            return;
        }

        var isRestrictedRole = context.User.IsInRole("Customer") || context.User.IsInRole("Provider");
        if (!isRestrictedRole)
        {
            await _next(context);
            return;
        }

        var userState = await userManager.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => new
            {
                x.IsActive,
                HasApprovedNationalId = x.Documents.Any(d =>
                    d.DocumentType == DocumentType.NationalId &&
                    d.Status == DocumentVerificationStatus.Approved)
            })
            .FirstOrDefaultAsync(context.RequestAborted);

        if (userState is null)
        {
            await WriteForbidden(context, "تعذر التحقق من حالة الحساب. من فضلك سجل الدخول مرة أخرى.");
            return;
        }

        if (!userState.IsActive)
        {
            await WriteForbidden(context, "حسابك غير نشط حالياً. يمكنك فقط توثيق الحساب أو التواصل مع الدعم، ولا يمكن تنفيذ الحجوزات أو الشراء أو إجراءات المزود قبل إعادة تفعيل الحساب.");
            return;
        }

        if (!userState.HasApprovedNationalId)
        {
            await WriteForbidden(context, "يجب توثيق البطاقة الشخصية أولاً قبل تنفيذ هذا الإجراء. يمكنك التصفح والبحث والتواصل مع الدعم حتى يتم اعتماد التوثيق.");
            return;
        }

        await _next(context);
    }

    private static Task WriteForbidden(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(new { message }), context.RequestAborted);
    }

    private static bool ShouldCheck(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
            return false;

        var method = context.Request.Method;
        if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method))
            return false;

        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

        // Allow authentication/session operations.
        if (path.StartsWith("/api/auth"))
            return false;

        // Allow account verification, document upload, media upload, profile completion and support communication.
        if (path.StartsWith("/api/documents") ||
            path.StartsWith("/api/media") ||
            path.StartsWith("/api/tickets") ||
            path.StartsWith("/api/customer-profile") ||
            path.StartsWith("/api/provider/profile") ||
            path.StartsWith("/api/profilecompletion"))
            return false;

        return true;
    }
}
