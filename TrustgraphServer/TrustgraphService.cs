using Microsoft.Owin.Hosting;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using Owin;
using System;
using System.IO;
using System.Net;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web.Http;
using TrustchainCore.Business;
using TrustchainCore.Configuration;
using TrustchainCore.Extensions;
using TrustgraphCore.Configuration;
using TrustgraphCore.IO;
using TrustgraphCore.Service;

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


    public sealed class UnitySingleton
    {
        private static readonly UnityContainer instance = new UnityContainer();

        private UnitySingleton() { }

        public static UnityContainer Container
        {
            get
            {
                return instance;
            }
        }
    }


    public class StartOwin
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            HttpListener listener = (HttpListener)appBuilder.Properties["System.Net.HttpListener"];
            listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;

            var config = new HttpConfiguration();
            config.Formatters.Add(new BrowserJsonFormatter());
            config.DependencyResolver = new UnityResolver(UnitySingleton.Container);

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
                );

            appBuilder.UseWebApi(config);
        }
    }

    public class TrustgraphService
    {
        private IDisposable _webApp;
        private RecoveringFileSystemWatcher _watcher;

        public object Key { get; private set; }

        public void Start()
        {
            var asm = new Assembly[] { typeof(IOCAttribute).Assembly };
            UnitySingleton.Container.RegisterTypesFromAssemblies(asm);

            var start = new StartOptions();
            start.Urls.Add("http://" + App.Config["endpoint"].ToStringValue("+") + ":" + App.Config["port"].ToInteger(12802) + "/");
            start.Urls.Add("https://" + App.Config["endpoint"].ToStringValue("+") + ":" + App.Config["sslport"].ToInteger(12702) + "/");

            _webApp = WebApp.Start<StartOwin>(start);

            //timeInMs = App.Config["processinterval"].ToInteger(timeInMs);
            _watcher = StartWatcher();
        }

        private RecoveringFileSystemWatcher StartWatcher()
        {
            var watcher = new RecoveringFileSystemWatcher(AppDirectory.LibraryPath);
            watcher.All += _watcher_All;
            watcher.Error += _watcher_Error;
            //watcher.DirectoryMonitorInterval = TimeSpan.FromSeconds(10);
            //watcher.EventQueueCapacity = 1;
            watcher.EnableRaisingEvents = true;
            Console.WriteLine("FileWatcher started on " + AppDirectory.LibraryPath);

            return watcher;
        }

        private void StopWatcher(RecoveringFileSystemWatcher watcher)
        {
            if (watcher != null)
            {
                watcher.Dispose();
                Console.WriteLine("FileWatcher stopped");
            }
        }

        private void _watcher_All(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Deleted || e.ChangeType == WatcherChangeTypes.Renamed)
                return;

            Console.WriteLine("Loading "+ e.Name);
            var loader = UnitySingleton.Container.Resolve<ITrustLoader>();
            loader.LoadFile(e.Name); // e.FullPath bug?!
        }

        private void _watcher_Error(object sender, FileWatcherErrorEventArgs e)
        {
            Console.Error.WriteLine(e.Error.Message);
            e.Handled = true;
        }

        public void Pause()
        {
            StopWatcher(_watcher);
        }

        public void Continue()
        {
            _watcher = StartWatcher();
        }

        public void Stop()
        {
            _webApp.Dispose();
            StopWatcher(_watcher);
        }
    }
}
