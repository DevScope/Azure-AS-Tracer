using DevScope.Framework.Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace AzureASTrace
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            try
            {
                Logger.CurrentLogger = new NLogLogger();

                var service = new AzureASTraceService();

                if (Environment.UserInteractive)
                {
                    Logger.Log("Running Mode: Console");

                    service.StartConsole(args);

                    Console.WriteLine("...............................................");
                    Console.WriteLine("........ Press any key to stop program ........");
                    Console.WriteLine("...............................................");
                    Console.Read();

                    service.StopConsole();
                }
                else
                {
                    Logger.Log("Running Mode: Service");

                    ServiceBase.Run(service);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }            
        }
    }
}
