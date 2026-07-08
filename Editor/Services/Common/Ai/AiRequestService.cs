using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Common.Ai
{
    public static class AiRequestService
    {
        private const int DefaultTimeoutSeconds = 300;
        private static readonly HttpClient HttpClient = new HttpClient();

        public static Task<AiRequestResult> PostJsonAsync(
            string url,
            string json,
            IReadOnlyDictionary<string, string> headers = null,
            int timeoutSeconds = DefaultTimeoutSeconds)
        {
            var content = new StringContent(json ?? string.Empty, Encoding.UTF8, "application/json");
            return SendAsync(url, content, headers, false, timeoutSeconds);
        }
        
        public static Task<AiRequestResult> PostJsonForBytesAsync(
            string url,
            string json,
            IReadOnlyDictionary<string, string> headers = null,
            int timeoutSeconds = DefaultTimeoutSeconds)
        {
            var content = new StringContent(json ?? string.Empty, Encoding.UTF8, "application/json");
            return SendAsync(url, content, headers, true, timeoutSeconds);
        }

        public static Task<AiRequestResult> PostForBytesAsync(
            string url,
            HttpContent content,
            IReadOnlyDictionary<string, string> headers = null,
            int timeoutSeconds = DefaultTimeoutSeconds)
        {
            return SendAsync(url, content, headers, true, timeoutSeconds);
        }

        private static async Task<AiRequestResult> SendAsync(
            string url,
            HttpContent content,
            IReadOnlyDictionary<string, string> headers,
            bool readBytes,
            int timeoutSeconds)
        {
            if (string.IsNullOrWhiteSpace(url))
                return AiRequestResult.Failure(HttpStatusCode.BadRequest, "Request URL is empty.");

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

                request.Content = content;

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        if (string.IsNullOrWhiteSpace(header.Key) || string.IsNullOrEmpty(header.Value)) continue;
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                using var response = await HttpClient.SendAsync(request, cancellationTokenSource.Token);
                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync();
                    return AiRequestResult.Failure(
                        response.StatusCode,
                        $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}",
                        errorText);
                }

                if (readBytes)
                    return AiRequestResult.Success(response.StatusCode, null, await response.Content.ReadAsByteArrayAsync());

                return AiRequestResult.Success(response.StatusCode, await response.Content.ReadAsStringAsync(), null);
            }
            catch (OperationCanceledException)
            {
                return AiRequestResult.Failure(HttpStatusCode.RequestTimeout, "Request timed out.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AiRequestService] Request failed: {ex.Message}");
                return AiRequestResult.Failure(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
