using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace msi_validator.Services.Interfaces
{
    public interface IHTTPClientService
    {
        Task<string> GetRequestWithHeaders(string url, Dictionary<string, string> headers);
    }
}
