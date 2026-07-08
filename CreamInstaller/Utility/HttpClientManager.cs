using System;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CreamInstaller.Utility;

internal static class HttpClientManager
{
    internal static HttpClient HttpClient;

    internal static void Setup()
    {
        HttpClient = new()
        {
            // Performance: limit request duration so the UI does not hang indefinitely
            // on slow or unresponsive endpoints (Steam API, Epic API, image CDNs).
            Timeout = TimeSpan.FromSeconds(60)
        };
        HttpClient.DefaultRequestHeaders.Add("User-Agent", $"CI{Program.Version.Replace(".", "")}");
    }

    // ANTIVIRUS FALSE POSITIVE WARNING:
    // This method makes outbound HTTPS GET requests to Steam Store / Epic Games Store APIs
    // to retrieve game and DLC metadata. All requests are read-only and user-initiated.
    // No data is sent to any third-party server beyond the query string.
    internal static async Task<string> EnsureGet(string url)
    {
        // Retry transient failures (dropped connections, timeouts, 5xx) with
        // exponential backoff so a momentary network hiccup no longer causes an
        // outright null result. 4xx and the final attempt keep the original
        // silent-failure contract (return null) to avoid changing callers.
        const int maxAttempts = 3;
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                using HttpRequestMessage request = new(HttpMethod.Get, url);
                using HttpResponseMessage response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                _ = response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e) when (attempt < maxAttempts && e.StatusCode is null or >= HttpStatusCode.InternalServerError)
            {
                // Connection failure (StatusCode null) or server error (5xx): back off and retry.
                await Task.Delay(TimeSpan.FromMilliseconds(500 * (1 << (attempt - 1))));
            }
            catch (TaskCanceledException) when (attempt < maxAttempts)
            {
                // Request timed out: back off and retry.
                await Task.Delay(TimeSpan.FromMilliseconds(500 * (1 << (attempt - 1))));
            }
            catch
            {
                return null;
            }
        }
    }

    internal static HtmlDocument ToHtmlDocument(this string html)
    {
        HtmlDocument document = new();
        document.LoadHtml(html);
        return document;
    }

    internal static async Task<HtmlNodeCollection> GetDocumentNodes(string url, string xpath)
        => (await EnsureGet(url))?.ToHtmlDocument()?.DocumentNode?.SelectNodes(xpath);

    internal static HtmlNodeCollection GetDocumentNodes(this HtmlDocument htmlDocument, string xpath) => htmlDocument.DocumentNode?.SelectNodes(xpath);

    // ANTIVIRUS FALSE POSITIVE WARNING:
    // Downloads game cover art images from Steam / Epic CDNs for display in the UI.
    // This is a standard image-fetch pattern, not a silent payload download.
    internal static async Task<Image> GetImageFromUrl(string url)
    {
        try
        {
            return new Bitmap(await HttpClient.GetStreamAsync(new Uri(url)));
        }
        catch
        {
            return null;
        }
    }

    internal static void Dispose() => HttpClient?.Dispose();
}