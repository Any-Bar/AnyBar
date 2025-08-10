using Flow.Bar.Models.Enums;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Flow.Bar.Helper.Http;

public static class HttpHelper
{
    private static readonly string ClassName = nameof(HttpHelper);

    private const string UserAgent = @"Mozilla/5.0 (Trident/7.0; rv:11.0) like Gecko";

    private static readonly HttpClient client = new();

    static HttpHelper()
    {
        // need to be added so it would work on a win10 machine
#pragma warning disable SYSLIB0014 // Type or member is obsolete
        ServicePointManager.Expect100Continue = true;
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls
            | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
#pragma warning restore SYSLIB0014 // Type or member is obsolete

        client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        HttpClient.DefaultProxy = WebProxy;
    }

    private static HttpProxy proxy = App.Settings.Proxy;
    public static HttpProxy Proxy
    {
        private get => proxy;
        set
        {
            proxy = value;
            proxy.PropertyChanged += Proxy_PropertyChanged;
            UpdateProxy(ProxyProperty.Enabled);
        }
    }

    private static void Proxy_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(HttpProxy.Enabled):
                UpdateProxy(ProxyProperty.Enabled);
                break;
            case nameof(HttpProxy.Server):
                UpdateProxy(ProxyProperty.Server);
                break;
            case nameof(HttpProxy.Port):
                UpdateProxy(ProxyProperty.Port);
                break;
            case nameof(HttpProxy.UserName):
                UpdateProxy(ProxyProperty.UserName);
                break;
            case nameof(HttpProxy.Password):
                UpdateProxy(ProxyProperty.Password);
                break;
        }
    }

    /// <summary>
    /// Update the Address of the Proxy to modify the client Proxy
    /// </summary>
    private static void UpdateProxy(ProxyProperty property)
    {
        if (string.IsNullOrEmpty(Proxy.Server)) return;

        try
        {
            (WebProxy.Address, WebProxy.Credentials) = property switch
            {
                ProxyProperty.Enabled => Proxy.Enabled switch
                {
                    true when !string.IsNullOrEmpty(Proxy.Server) => Proxy.UserName switch
                    {
                        var userName when string.IsNullOrEmpty(userName) =>
                            (new Uri($"http://{Proxy.Server}:{Proxy.Port}"), null),
                        _ => (new Uri($"http://{Proxy.Server}:{Proxy.Port}"),
                            new NetworkCredential(Proxy.UserName, Proxy.Password))
                    },
                    _ => (null, null)
                },
                ProxyProperty.Server => (new Uri($"http://{Proxy.Server}:{Proxy.Port}"), WebProxy.Credentials),
                ProxyProperty.Port => (new Uri($"http://{Proxy.Server}:{Proxy.Port}"), WebProxy.Credentials),
                ProxyProperty.UserName => (WebProxy.Address, new NetworkCredential(Proxy.UserName, Proxy.Password)),
                ProxyProperty.Password => (WebProxy.Address, new NetworkCredential(Proxy.UserName, Proxy.Password)),
                _ => throw new ArgumentOutOfRangeException(null)
            };
        }
        catch (UriFormatException e)
        {
            App.API.ShowMsg(Localize.HTTPHelper_PleaseTryAgain(), Localize.HTTPHelper_ParseProxyFailed());
            App.API.LogFatal(ClassName, "Unable to parse uri", e);
        }
    }

    public static WebProxy WebProxy { get; } = new WebProxy();

    public static async Task DownloadAsync([NotNull] string url, [NotNull] string filePath, Action<double>? reportProgress = null, CancellationToken token = default)
    {
        try
        {
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var canReportProgress = totalBytes != -1;

                if (canReportProgress && reportProgress != null)
                {
                    await using var contentStream = await response.Content.ReadAsStreamAsync(token);
                    await using var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 8192, true);

                    var buffer = new byte[8192];
                    long totalRead = 0;
                    int read;
                    double progressValue = 0;

                    reportProgress(0);

                    while ((read = await contentStream.ReadAsync(buffer, token)) > 0)
                    {
                        await fileStream.WriteAsync(buffer.AsMemory(0, read), token);
                        totalRead += read;

                        progressValue = totalRead * 100.0 / totalBytes;

                        if (token.IsCancellationRequested)
                            return;
                        else
                            reportProgress(progressValue);
                    }

                    if (progressValue < 100)
                        reportProgress(100);
                }
                else
                {
                    await using var fileStream = new FileStream(filePath, FileMode.CreateNew);
                    await response.Content.CopyToAsync(fileStream, token);
                }
            }
            else
            {
                throw new HttpRequestException($"Error code <{response.StatusCode}> returned from <{url}>");
            }
        }
        catch (HttpRequestException e)
        {
            App.API.LogFatal(ClassName, "Http request error", e, "DownloadAsync");
            throw;
        }
    }

    /// <summary>
    /// Asynchrously get the result as string from url.
    /// When supposing the result larger than 83kb, try using GetStreamAsync to avoid reading as string
    /// </summary>
    /// <param name="url"></param>
    /// <returns>The Http result as string. Null if cancellation requested</returns>
    public static Task<string> GetAsync([NotNull] string url, CancellationToken token = default)
    {
        App.API.LogDebug(ClassName, $"Url <{url}>");
        return GetAsync(new Uri(url), token);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <param name="token"></param>
    /// <returns>The Http result as string. Null if cancellation requested</returns>
    public static async Task<string> GetAsync([NotNull] Uri url, CancellationToken token = default)
    {
        App.API.LogDebug(ClassName, $"Url <{url}>");
        using var response = await client.GetAsync(url, token);
        var content = await response.Content.ReadAsStringAsync(token);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new HttpRequestException(
                $"Error code <{response.StatusCode}> with content <{content}> returned from <{url}>");
        }

        return content;
    }

    /// <summary>
    /// Send a GET request to the specified Uri with an HTTP completion option and a cancellation token as an asynchronous operation.
    /// </summary>
    /// <param name="url">The Uri the request is sent to.</param>
    /// <param name="completionOption">An HTTP completion option value that indicates when the operation should be considered completed.</param>
    /// <param name="token">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns></returns>
    public static Task<Stream> GetStreamAsync([NotNull] string url,
        CancellationToken token = default) => GetStreamAsync(new Uri(url), token);

    /// <summary>
    /// Send a GET request to the specified Uri with an HTTP completion option and a cancellation token as an asynchronous operation.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task<Stream> GetStreamAsync([NotNull] Uri url,
        CancellationToken token = default)
    {
        App.API.LogDebug(ClassName, $"Url <{url}>");
        return await client.GetStreamAsync(url, token);
    }

    public static async Task<HttpResponseMessage> GetResponseAsync(string url, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken token = default)
        => await GetResponseAsync(new Uri(url), completionOption, token);

    public static async Task<HttpResponseMessage> GetResponseAsync([NotNull] Uri url, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken token = default)
    {
        App.API.LogDebug(ClassName, $"Url <{url}>");
        return await client.GetAsync(url, completionOption, token);
    }

    /// <summary>
    /// Asynchrously send an HTTP request.
    /// </summary>
    public static async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead, CancellationToken token = default)
    {
        try
        {
            return await client.SendAsync(request, completionOption, token);
        }
        catch (Exception)
        {
            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        }
    }

    public static async Task<string> GetStringAsync(string url, CancellationToken token = default)
    {
        try
        {
            App.API.LogDebug(ClassName, $"Url <{url}>");
            return await client.GetStringAsync(url, token);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}
