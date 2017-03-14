using Microsoft.Owin.Hosting;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Web.Http;
using TrustchainCore.Configuration;
using TrustchainCore.Data;
using TrustchainCore.Extensions;
using TrustgraphCore.Configuration;
using TrustgraphCore.Controllers;

namespace TrustgraphServer
{
    public class BrowserJsonFormatter : JsonMediaTypeFormatter
    {
        public BrowserJsonFormatter()
        {
            this.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            this.SerializerSettings.Formatting = Formatting.Indented;
        }

        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            base.SetDefaultContentHeaders(type, headers, mediaType);
            headers.ContentType = new MediaTypeHeaderValue("application/json");
        }
    }

    public class TrustbuildService
    {
        private IDisposable _webApp;
        private Timer timer;
        private int timeInMs = 1000*60; // 1 minute
        private volatile bool process = true;

        public object Key { get; private set; }

        public void Start()
        {
            var url = "http://" + App.Config["endpoint"].ToStringValue("localhost") + ":" + App.Config["port"].ToInteger(12702)+ "/";
            _webApp = WebApp.Start<StartOwin>(url);

            //using (var db = TrustchainDatabase.Open())
            //{
            //    db.CreateIfNotExist();
            //}

            timeInMs = App.Config["processinterval"].ToInteger(timeInMs);

            RunTimer(Execute);
        }

        public class StartOwin
        {
            public void Configuration(IAppBuilder appBuilder)
            {
                var config = new HttpConfiguration();
                config.Formatters.Add(new BrowserJsonFormatter());

                var asm = new Assembly[] { typeof(IOCAttribute).Assembly };

                var container = new UnityContainer();
                container.RegisterTypesFromAssemblies(asm);
                config.DependencyResolver = new UnityResolver(container);

                config.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "api/{controller}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                    );
                
                appBuilder.UseWebApi(config);
            }
        }

        private void RunTimer(Action method)
        {

            method(); // Start now!

            timer = new Timer((o) =>
            {
                try
                {
                    if (process) // Run the job
                        method();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }
                finally
                {
                    // only set the initial time, do not set the recurring time
                        timer.Change(timeInMs, Timeout.Infinite);
                }
            });

            // only set the initial time, do not set the recurring time
            timer.Change(timeInMs, Timeout.Infinite);
        }

        public void Execute()
        {
            Console.WriteLine(DateTime.Now.ToLocalTime() + " : Processing...");
        }

        public void Pause()
        {
            process = false;
        }

        public void Continue()
        {
            process = true;
        }

        public void Stop()
        {
            _webApp.Dispose();
        }
    }
}
