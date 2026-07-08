using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace HEVEQ.Api.Controllers;

[ApiController]
[Route("api/map-links")]
public class MapLinksController(IHttpClientFactory httpClientFactory, ILogger<MapLinksController> logger) : ControllerBase
{
    private static readonly Regex[] CoordinatePatterns =
    {
        new(@"@(-?\d+(?:\.\d+)?),\s*(-?\d+(?:\.\d+)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new(@"[?&](?:q|query|ll)=(-?\d+(?:\.\d+)?),\s*(-?\d+(?:\.\d+)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new(@"!3d(-?\d+(?:\.\d+)?)!4d(-?\d+(?:\.\d+)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new(@"(?:^|[^\d-])(-?\d{1,2}\.\d{4,})\s*,\s*(-?\d{1,3}\.\d{4,})(?:$|[^\d.])", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new(@"(?:lat|latitude)[=:]\s*(-?\d+(?:\.\d+)?).*?(?:lng|lon|longitude)[=:]\s*(-?\d+(?:\.\d+)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline)
    };

    [HttpPost("resolve")]
    [AllowAnonymous]
    public async Task<IActionResult> Resolve([FromBody] ResolveMapLinkRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Url))
            return BadRequest(new { message = "من فضلك أدخل رابط الموقع أولاً." });

        var direct = TryExtractCoordinates(request.Url);
        if (direct is not null)
            return Ok(direct);

        if (!Uri.TryCreate(request.Url.Trim(), UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
            return BadRequest(new { message = "رابط الموقع غير صحيح. يمكنك لصق الإحداثيات مباشرة بالشكل 30.123,31.456." });

        if (!IsAllowedGoogleMapsHost(uri.Host))
            return BadRequest(new { message = "من فضلك استخدم رابط Google Maps أو الصق الإحداثيات مباشرة." });

        try
        {
            using var handler = new HttpClientHandler { AllowAutoRedirect = true, MaxAutomaticRedirections = 10 };
            using var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(12)
            };

            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 HEVEQ Map Link Resolver");
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

            using var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, ct);
            var resolvedUrl = response.RequestMessage?.RequestUri?.ToString() ?? request.Url;

            var fromResolvedUrl = TryExtractCoordinates(resolvedUrl);
            if (fromResolvedUrl is not null)
                return Ok(fromResolvedUrl with { ResolvedUrl = resolvedUrl });

            var body = await ReadLimitedBodyAsync(response, ct);
            var fromBody = TryExtractCoordinates(body);
            if (fromBody is not null)
                return Ok(fromBody with { ResolvedUrl = resolvedUrl });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to resolve map link {Url}", request.Url);
        }

        return BadRequest(new
        {
            message = "تعذر استخراج الإحداثيات من الرابط المختصر. افتح الرابط في المتصفح ثم انسخ الرابط الكامل الذي يحتوي على @latitude,longitude أو الصق الإحداثيات مباشرة."
        });
    }

    private static async Task<string> ReadLimitedBodyAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var content = await response.Content.ReadAsStringAsync(ct);
        return content.Length <= 300_000 ? content : content[..300_000];
    }

    private static ResolveMapLinkResponse? TryExtractCoordinates(string value)
    {
        var text = SafeDecode(value ?? string.Empty);
        foreach (var pattern in CoordinatePatterns)
        {
            var match = pattern.Match(text);
            if (!match.Success) continue;

            if (!decimal.TryParse(match.Groups[1].Value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var latitude))
                continue;
            if (!decimal.TryParse(match.Groups[2].Value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var longitude))
                continue;

            if (latitude is >= -90 and <= 90 && longitude is >= -180 and <= 180)
                return new ResolveMapLinkResponse((double)latitude, (double)longitude, null);
        }

        return null;
    }

    private static bool IsAllowedGoogleMapsHost(string host)
    {
        var normalized = host.ToLowerInvariant();
        return normalized == "maps.app.goo.gl" ||
               normalized.EndsWith(".google.com") ||
               normalized == "google.com" ||
               normalized.EndsWith(".google.com.eg") ||
               normalized == "goo.gl";
    }

    private static string SafeDecode(string value)
    {
        try { return Uri.UnescapeDataString(value); }
        catch { return value; }
    }
}

public sealed record ResolveMapLinkRequest(string Url);
public sealed record ResolveMapLinkResponse(double Latitude, double Longitude, string? ResolvedUrl);
