using HEVEQ.Application.Common.Exceptions;
using HEVEQ.Api.Middleware;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HEVEQ.WebApi.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "خطأ في التحقق من البيانات"),
            FluentValidation.ValidationException => (StatusCodes.Status400BadRequest, "خطأ في التحقق من البيانات"),
            BadRequestException => (StatusCodes.Status400BadRequest, "طلب غير صحيح"),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "لا يمكن تنفيذ الإجراء"),
            ArgumentException => (StatusCodes.Status400BadRequest, "البيانات المدخلة غير صحيحة"),
            NotFoundException => (StatusCodes.Status404NotFound, "العنصر غير موجود"),
            ForbiddenAccessException => (StatusCodes.Status403Forbidden, "غير مصرح"),
            _ => (StatusCodes.Status500InternalServerError, "خطأ في الخادم")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }
        else
        {
            logger.LogWarning("{ExceptionType}: {Message}", exception.GetType().Name, exception.Message);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = ArabicErrorMapper.ToArabic(exception.Message),
            Instance = httpContext.Request.Path
        };

        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors;
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}