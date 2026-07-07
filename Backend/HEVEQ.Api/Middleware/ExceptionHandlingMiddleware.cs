using System.Net;
using System.Text.Json;
using HEVEQ.Application.Common.Exceptions;

namespace HEVEQ.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Always log the FULL exception (type, message, stack trace, inner exception).
            // The response body intentionally stays generic for unexpected errors —
            // but we must never lose the real error from the server console/logs.
            _logger.LogError(ex,
                "Unhandled exception on {Method} {Path}",
                context.Request.Method, context.Request.Path);

            context.Response.ContentType = "application/json";

            var (statusCode, payload) = ex switch
            {
                NotFoundException =>
                    (HttpStatusCode.NotFound, (object)new { message = ArabicErrorMapper.ToArabic(ex.Message) }),

                ForbiddenAccessException =>
                    (HttpStatusCode.Forbidden, (object)new { message = ArabicErrorMapper.ToArabic(ex.Message) }),

                BadRequestException =>
                    (HttpStatusCode.BadRequest, (object)new { message = ArabicErrorMapper.ToArabic(ex.Message) }),

                InvalidOperationException =>
                    (HttpStatusCode.BadRequest, (object)new { message = ArabicErrorMapper.ToArabic(ex.Message) }),

                ArgumentException =>
                    (HttpStatusCode.BadRequest, (object)new { message = ArabicErrorMapper.ToArabic(ex.Message) }),

                FluentValidation.ValidationException ve =>
                    (HttpStatusCode.BadRequest, (object)new
                    {
                        errors = ve.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(g => g.Key, g => g.Select(e => ArabicErrorMapper.ToArabic(e.ErrorMessage)).ToArray())
                    }),
                ValidationException ve =>
                   (HttpStatusCode.BadRequest, (object)new
                   {
                       errors = ve.Errors.ToDictionary(x => x.Key, x => x.Value.Select(ArabicErrorMapper.ToArabic).ToArray())
                   }),

                _ =>
                    (HttpStatusCode.InternalServerError, _env.IsDevelopment()
                        ? (object)new
                        {
                            message = "حدث خطأ غير متوقع، حاول مرة أخرى.",
                            exceptionType = ex.GetType().FullName,
                            exceptionMessage = ArabicErrorMapper.ToArabic(ex.Message),
                            innerException = ArabicErrorMapper.ToArabic(ex.InnerException?.Message),
                            stackTrace = ex.StackTrace
                        }
                        : (object)new { message = "حدث خطأ غير متوقع، حاول مرة أخرى." })
            };

            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}