using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace msi_validator.Services.Interfaces
{
    public interface IMSITestService
    {
        bool IsMSIEnabled();

        bool ValidateResource(string resource);

        Task<string> GetToken(string resource);

        Task<string> TestConnection(string resource, string endpoint);
    }
}
