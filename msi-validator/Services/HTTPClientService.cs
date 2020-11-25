using CommandDotNet.Builders;
using Microsoft.Extensions.Logging;
using msi_validator.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace msi_validator.Services
{
    public class HTTPClientService : IHTTPClientService
    {
        private readonly ILogger<HTTPClientService> _logger;
        private readonly HttpClient _httpClient;

        public HTTPClientService(ILogger<HTTPClientService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = new HttpClient();
        }

        async Task<string> IHTTPClientService.GetRequestWithHeaders(string url , Dictionary<string,string> headers)
        {
            _httpClient.DefaultRequestHeaders.Clear();

            foreach (KeyValuePair<string, string> header in headers)
            {
                _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            string responseContent = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Unable to make a request to the endpoint ");
                _logger.LogError($"Request : {response.RequestMessage}");
                _logger.LogError($"HTTP Status Code : {response.StatusCode}");
                _logger.LogError($"{responseContent}");
                return "";
            }

            return responseContent;
        }

        
    }
}
