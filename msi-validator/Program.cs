
using CommandDotNet;
using CommandDotNet.IoC.MicrosoftDependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using msi_validator.Controller;
using msi_validator.Models;
using msi_validator.Services;
using msi_validator.Services.Interfaces;
using Serilog;
using System;
using System.Threading.Tasks;

namespace msi_validator
{
    class Program
    {
        
        public static int Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<IHTTPClientService, HTTPClientService>();
            services.AddSingleton<IMSITestService, MSITestService>();
            services.AddSingleton<MSITestController>();

            

            var serilogLogger = new LoggerConfiguration()
                                .WriteTo.Console()
                                .WriteTo.RollingFile("msi-validator.log")
                                .CreateLogger();

            services.AddLogging(logging =>
           {
               logging.SetMinimumLevel(LogLevel.Information);
               logging.AddSerilog(logger: serilogLogger, dispose: true);

           });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            AppRunner<MSITestController> appRunner = new AppRunner<MSITestController>();
            appRunner.UseMicrosoftDependencyInjection(serviceProvider);

            string version = "v1.0.0.0";
            Console.WriteLine($"Running MSI Validator {version}");

            return appRunner.Run(args);
            
            
        }
    }
}
