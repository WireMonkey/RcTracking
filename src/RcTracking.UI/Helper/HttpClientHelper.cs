using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Headers;

namespace RcTracking.UI.Helper
{
    public static class HttpClientHelper
    {
        public static async Task<HttpClient> CreateHttpClient(string apiUrl, string functionKey, IAccessTokenProvider accessTokenProvider)
        {
            var accessToken = await GetAccessTokenAsync(accessTokenProvider);
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(apiUrl)
            };
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            httpClient.DefaultRequestHeaders.Add("x-functions-key", functionKey);

            return httpClient;
        }

        private static async Task<string> GetAccessTokenAsync(IAccessTokenProvider accessTokenProvider)
        {
            var tokenResult = await accessTokenProvider.RequestAccessToken();
            if (tokenResult.TryGetToken(out var accessToken))
            {
                return accessToken.Value;
            }
            throw new InvalidOperationException("Unable to acquire access token.");
        }
    }
}
