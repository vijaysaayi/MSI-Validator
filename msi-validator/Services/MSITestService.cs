using Microsoft.Extensions.Logging;
using msi_validator.Models;
using msi_validator.Services.Interfaces;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Data.SqlClient;


namespace msi_validator.Services
{
    public class MSITestService : IMSITestService
    {
        private string _url;
        private string _secret;
        private HttpClient _client;
        private Endpoints _endpoint;
        private readonly IHTTPClientService _http;
        private readonly ILogger<MSITestService> _logger;
        
        public MSITestService(IHTTPClientService http, ILogger<MSITestService> logger)
        {            
            _url = Environment.GetEnvironmentVariable("IDENTITY_ENDPOINT");
            _secret = Environment.GetEnvironmentVariable("IDENTITY_HEADER");
            _client = new HttpClient();
            _endpoint = new Endpoints();

            _http = http ?? throw new ArgumentNullException(nameof(http));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
        }

       bool IMSITestService.IsMSIEnabled()
        {
            if (string.IsNullOrEmpty(_url) || string.IsNullOrEmpty(_secret))
                return false;
            return true;
        }

        private string  ValidateResource(string resource)
        {
            if (string.IsNullOrEmpty(resource))
            {
                _logger.LogWarning("Resource not specified. Requesting for a token for the 'https://management.azure.com'");

                resource = "default";
            }

            return resource;
        }

        private async Task<string> GetToken(string resource, string clientId)
        {
            resource = ValidateResource(resource);

            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                { "X-IDENTITY-HEADER" , _secret }
            };

            string baseUrl = _endpoint.GetUrl(resource);
            string url = $"{_url}?resource={baseUrl}&api-version=2019-08-01";
            if (!string.IsNullOrEmpty(clientId))
                url += $"&client_id={clientId}";

            _logger.LogInformation($"Requesting Azure AD for an access token for the resource : {baseUrl}");
            string response = await _http.GetRequestWithHeaders(url, headers);

            return (response);
        }
         async Task<string> IMSITestService.GetToken(string resource, string clientId)
        {

            string response = await GetToken(resource, clientId);
            return response;
            
        }

        async Task<string> IMSITestService.TestConnection(string resource, string endpoint, string clientId)
        {
            resource = ValidateResource(resource);

            if(string.IsNullOrEmpty(endpoint))
            {
                return ("Invalid endpoint");
            }
            else
            {
                string responseWithToken = await GetToken(resource, clientId);
                dynamic json = JsonConvert.DeserializeObject(responseWithToken);
                string accessToken = json.access_token.ToString();

                switch (resource)
                {
                    case "keyvault":
                        {
                            return await KeyVaultTestResults(accessToken, endpoint);
                            
                        }
                    case "storage":
                        {
                            return await StorageTestResults(accessToken, endpoint);
                            
                        }
                    case "sql":
                        {
                            return await SQLTestResults(accessToken, endpoint);
                        }
                    default:
                        {
                            return "Invalid Resource";
                        }
                }


            }

        }

        private async Task<string> KeyVaultTestResults(string accessToken, string endpoint)
        {
            string resource = "keyvault";
            _logger.LogInformation($"Successfully obtained an access token {accessToken} from Azure AD for the resource {resource}");

            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                { "Authorization" , $"Bearer {accessToken}"}
            };

            endpoint = $"{endpoint}?api-version=2016-10-01";
            string response = await _http.GetRequestWithHeaders(endpoint, headers);
            return response;

        }

        private async Task<string> StorageTestResults(string accessToken, string endpoint)
        {
            string resource = "storage";
            _logger.LogInformation($"Successfully obtained an access token {accessToken} from Azure AD for the resource {resource}");

            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                { "Authorization" , $"Bearer {accessToken}"},
                { "x-ms-version" , $"2017-11-09"},

            };

            string response = await _http.GetRequestWithHeaders(endpoint, headers);
            return response;

        }

        private async Task<string> SQLTestResults(string accessToken, string connectionString)
        {
            string resource = "sql";
            _logger.LogInformation($"Successfully obtained an access token {accessToken} from Azure AD for the resource {resource}");
            
            SqlConnection conn = new SqlConnection(connectionString);
            string status;
            try
            {
                
                conn.AccessToken = accessToken;
                await conn.OpenAsync();
                status = "Success";
            }
            catch (Exception ex)
            {
                status = $"Unable to connect to SQL. Exception : {ex.Message}";
            }

            conn.Close();
            return status;
            
            

        }

        bool IMSITestService.ValidateResource(string resource)
        {
            if(! _endpoint.GetKeys().Contains(resource) )
            {
                _logger.LogError($"Argument {resource} is invalid. ");
                _logger.LogError($"Available arguments are { string.Join(",", _endpoint.GetKeys())}");
                return false;
            }
            return true;
        }
    }
}
