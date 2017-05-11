using Owin;
using System.Net;
using System.Web.Http;

namespace TrustgraphServer
{
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
}
