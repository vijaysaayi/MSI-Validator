using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CommandDotNet;
using CommandDotNet.Builders;
using Microsoft.Extensions.Logging;
using msi_validator.Services.Interfaces;

namespace msi_validator.Controller
{
    public class MSITestController
    {
        private readonly IHTTPClientService _http;
        private readonly IMSITestService _msi;

        public ILogger<MSITestController> _logger { get; }

        public MSITestController(IHTTPClientService http , IMSITestService msi, ILogger<MSITestController> logger)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _msi = msi?? throw new ArgumentNullException(nameof(msi));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
           
        }

        [Command(Name = "get-token",
        Usage = "get-token <resource>",
        Description = "Gets a token for the specified resource")]
        public async Task GetToken(
        [Option(LongName = "resource", ShortName = "r", 
        Description = "URL of the resource for which token should be issued ")]
        string resource,
        [Option(LongName = "clientId", ShortName = "c",
        Description = "Client Id of the User Assigned Identity ")]
        string clientId
        )
        {
            if (!_msi.IsMSIEnabled())
                _logger.LogError("MSI is not enabled for the App Service");
            else
            {
                try
                {
                    if(_msi.ValidateResource(resource))
                    {
                        string response = await _msi.GetToken(resource, clientId);
                        _logger.LogInformation(response);
                    }
                    
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }



            }

        }

        [Command(Name = "test-connection",
        Usage = "test-connection <resource>",
        Description = "Gets a token for the specified resource")]
        public async Task TestConnection(
        [Option(LongName = "resource", ShortName = "r",
        Description = "Resource Type for which the token should be generated")]
        string resource,

        [Option(LongName = "endpoint", ShortName = "e",
        Description = "Endpoint URL fow which we can test the connection")]
        string endpoint,
        [Option(LongName = "clientId", ShortName = "c",
        Description = "Client Id of the User Assigned Identity ")]
        string clientId
        )
        {
            if (!_msi.IsMSIEnabled())
                _logger.LogError("MSI is not enabled for the App Service");
            else
            {
                try
                {
                    if (_msi.ValidateResource(resource))
                    {
                        string response = await _msi.TestConnection(resource, endpoint, clientId);
                        Console.WriteLine(response);
                    }
                        
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                

            }

        }

    }
}
