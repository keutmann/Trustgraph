using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;
using Topshelf.Logging;
using TrustchainCore.Business;
using TrustchainCore.Configuration;


namespace TrustgraphServer
{
    class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                return Setup();
            }
            catch (Exception ex)
            {
                Trace.Fail(ex.Message);
                return -1;
            }
        }

        private static int Setup()
        {
            App.LoadConfigFile("config.json");

            // Ensure AppData directories
            AppDirectory.Setup();
            App.InitializeLogging();

            // Only when we need to create a config file. 
            //App.SaveConfigFile("config.json");
            var result = (int)HostFactory.Run(configurator =>
            {
                // Setup configuration from commandline 
                foreach (JProperty property in App.Config.OfType<JProperty>())
                    switch (property.Value.Type)
                    {
                        case JTokenType.String: configurator.AddCommandLineDefinition(property.Name, value => { property.Value = value; }); break;
                        case JTokenType.Integer: configurator.AddCommandLineDefinition(property.Name, value => { property.Value = int.Parse(value); }); break;
                        case JTokenType.Boolean: configurator.AddCommandLineDefinition(property.Name, value => { property.Value = bool.Parse(value); }); break;
                    }
                configurator.ApplyCommandLine();
                
                configurator.Service<TrustgraphService>(s =>
                {
                    s.ConstructUsing(() => new TrustgraphService());
                    s.WhenStarted(service => service.Start());
                    s.WhenPaused(service => service.Pause());
                    s.WhenContinued(service => service.Continue());
                    s.WhenStopped(service => service.Stop());
                });

                configurator.RunAsLocalSystem();
                configurator.SetDescription("Trustchain Graph Server");
                configurator.SetDisplayName("Trustgraph");
                configurator.SetServiceName("Trustgraph");
            });

            return result;
        }
    }
}
