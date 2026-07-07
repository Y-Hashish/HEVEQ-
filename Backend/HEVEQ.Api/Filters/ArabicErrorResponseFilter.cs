using HEVEQ.Api.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HEVEQ.Api.Filters;

public class ArabicErrorResponseFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult && ((objectResult.StatusCode ?? context.HttpContext.Response.StatusCode) >= 400))
        {
            objectResult.Value = ArabicErrorMapper.TranslatePayload(objectResult.Value);
        }
        else if (context.Result is BadRequestObjectResult badRequest)
        {
            badRequest.Value = ArabicErrorMapper.TranslatePayload(badRequest.Value);
        }
        else if (context.Result is NotFoundObjectResult notFound)
        {
            notFound.Value = ArabicErrorMapper.TranslatePayload(notFound.Value);
        }
        else if (context.Result is UnauthorizedObjectResult unauthorized)
        {
            unauthorized.Value = ArabicErrorMapper.TranslatePayload(unauthorized.Value);
        }

        await next();
    }
}
