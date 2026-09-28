using System.Net;
using System.Text;
using ForeverPin.Redirect.Api.Application.Routing.Models;

namespace ForeverPin.Redirect.Api.Endpoints;

/// <summary>
/// Renders the pages a scanner sees when a code has no destination. A scan always comes from a phone's browser, so a
/// bare status would show a blank error screen; the page says what happened and never echoes request input.
/// </summary>
public static class ScanPages
{
    /// <summary>Renders the page for a slug no code carries.</summary>
    public static IResult Unknown() => Page(
        "This code doesn't exist",
        "Check that the whole code was in view when you scanned it, or ask whoever shared it for a new one.");

    /// <summary>Renders the page for an existing code without a destination for this scan.</summary>
    /// <param name="miss">Why the scan has no destination.</param>
    public static IResult For(RoutingMiss miss) => miss switch
    {
        RoutingMiss.Paused => Page(
            "This code is paused",
            "Its owner has paused it for now. Try scanning again later."),
        RoutingMiss.Undeliverable => Page(
            "This code can't open here yet",
            "Its content can't be shown in a browser yet. Scan it with your camera app to use it."),
        _ => Page(
            "Nothing to open here",
            "This code has no destination for your device or location."),
    };

    private static IResult Page(string title, string message)
    {
        var html = $$"""
            <!doctype html>
            <html lang="en">
            <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <meta name="robots" content="noindex">
            <title>{{WebUtility.HtmlEncode(title)}} · ForeverPin</title>
            <style>
            :root { color-scheme: light dark; }
            body { margin: 0; min-height: 100vh; display: grid; place-items: center; padding: 24px; box-sizing: border-box;
              font-family: system-ui, -apple-system, "Segoe UI", sans-serif; background: #f4f3fb; color: #1c1c26; }
            main { max-width: 26rem; text-align: center; }
            h1 { font-size: 1.35rem; margin: 0 0 .5rem; }
            p { margin: 0; line-height: 1.5; color: #54596f; }
            a { display: inline-block; margin-top: 1.5rem; font-size: .85rem; color: #6d28d9; text-decoration: none; }
            @media (prefers-color-scheme: dark) { body { background: #141418; color: #ececf1; } p { color: #a9abb8; } a { color: #a78bfa; } }
            </style>
            </head>
            <body>
            <main>
            <h1>{{WebUtility.HtmlEncode(title)}}</h1>
            <p>{{WebUtility.HtmlEncode(message)}}</p>
            <a href="https://foreverpin.com">Powered by ForeverPin</a>
            </main>
            </body>
            </html>
            """;
        return new ScanPageResult(html);
    }

    /// <summary>Writes a scan page as an uncached 404, so a paused or deleted code is re-checked on the next scan.</summary>
    private sealed class ScanPageResult(string html) : IResult
    {
        public Task ExecuteAsync(HttpContext httpContext)
        {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            httpContext.Response.ContentType = "text/html; charset=utf-8";
            httpContext.Response.Headers.CacheControl = "no-store";
            return httpContext.Response.WriteAsync(html, Encoding.UTF8);
        }
    }
}
