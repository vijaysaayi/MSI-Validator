using CommandDotNet;
using msi_validator.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace msi_validator.Models
{
    public class Endpoints 
    {
        public Endpoints()
        {
            LoadConfig();
        }

        private Dictionary<string, Dictionary<string, string>> _endpointMap ;
        
        public List<string> GetKeys()
        {
            List<string> keyList = new List<string>(_endpointMap.Keys);
            return keyList;
        }

        public void LoadConfig()
        {
            var d = new Dictionary<string, Dictionary<string, string>>()
            {
                {
                    "keyvault", new Dictionary<string,string>
                    {
                        { "url", "https://vault.azure.net" },
                        { "apiVersion", "2016-10-01" }
                    }
                },
                {  "storage" , new Dictionary<string,string>
                    {
                        { "url" , "https://storage.azure.com" },
                        { "apiVersion" , "2018-02-01"  }
                    }
                },
                {  "sql" , new Dictionary<string,string>
                    {
                        { "url" , "https://database.windows.net" },
                        { "apiVersion" , ""  }
                    }
                },
                {  "default" , new Dictionary<string,string>
                    {
                        { "url" , "https://management.azure.com" },
                        { "apiVersion" , ""  }
                    }
                }
            };
            _endpointMap = d;         
                       
        }


        public string GetUrl(string resource)
        {
            Console.WriteLine($"url for {resource} is {_endpointMap[resource]["url"]}");
            return _endpointMap[resource]["url"];
        }

        public string GetApiVersion(string resource)
        {
            Console.WriteLine($"API version for {resource} is {_endpointMap[resource]["apiVersion"]}");
            return _endpointMap[resource]["apiVersion"];
        }
       
    }
}
