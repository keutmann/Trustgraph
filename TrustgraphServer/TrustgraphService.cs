using Microsoft.Owin.Hosting;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using Owin;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web.Http;
using TrustchainCore.Business;
using TrustchainCore.Configuration;
using TrustchainCore.Extensions;
using TrustchainCore.IOC;
using TrustgraphCore.IO;
using TrustgraphCore.Service;

namespace TrustgraphServer
{
    public class TrustgraphService
    {
        private IDisposable _webApp;
        private RecoveringFileSystemWatcher _watcher;

        public object Key { get; private set; }

        public void Start()
        {

            var asm = new Assembly[] { typeof(IOCAttribute).Assembly, typeof(IGraphBuilder).Assembly };
            UnitySingleton.Container.RegisterTypesFromAssemblies(asm);
            Trace.TraceInformation("Register types from assemblies");

            var start = new StartOptions();
            start.Urls.Add("http://" + App.Config["endpoint"].ToStringValue("+") + ":" + App.Config["port"].ToInteger(12602) + "/");
            start.Urls.Add("https://" + App.Config["endpoint"].ToStringValue("+") + ":" + App.Config["sslport"].ToInteger(12702) + "/");
            
            _webApp = WebApp.Start<StartOwin>(start);
            Trace.Listeners.RemoveAt(Trace.Listeners.Count - 1);

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

            return watcher;
        }

        private void StopWatcher(RecoveringFileSystemWatcher watcher)
        {
            if (watcher != null)
            {
                watcher.Dispose();
                Trace.TraceInformation("FileWatcher stopped");
            }
        }

        private void _watcher_All(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Deleted || e.ChangeType == WatcherChangeTypes.Renamed)
                return;

            Trace.TraceInformation("Loading "+ e.Name);
            var loader = UnitySingleton.Container.Resolve<ITrustLoader>();
            loader.LoadFile(e.Name); // e.FullPath bug?!
        }

        private void _watcher_Error(object sender, FileWatcherErrorEventArgs e)
        {
            Trace.TraceError(e.Error.Message);
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
