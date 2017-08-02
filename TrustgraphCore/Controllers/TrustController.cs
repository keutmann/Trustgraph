using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using TrustchainCore.Business;
using TrustchainCore.Configuration;
using TrustgraphCore.Service;
using TrustchainCore.Extensions;
using TrustchainCore.Model;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TrustgraphCore.Controllers
{
    public class TrustController : ApiController
    {
        public const string Path = "/api/trust/";

        private IGraphBuilder graphBuilder;

        public TrustController(IGraphBuilder builder)
        {
            graphBuilder = builder;
        }

        [HttpPost]
        public IHttpActionResult Add([FromBody]PackageModel package)
        {
            try
            {
                var trustBuilder = new TrustBuilder(package);
                trustBuilder.Verify();

                graphBuilder.Append(package);

                var buildserverUrl = App.Config["buildserver"].ToStringValue("http://127.0.01:12601");
                if (!string.IsNullOrEmpty(buildserverUrl))
                {
                    var fullUrl = new UriBuilder(buildserverUrl);
                    fullUrl.Path = Path;
                    using (var client = new HttpClient())
                    {
                        var response = client.PostAsJsonAsync(fullUrl.ToString(), package);
                        Task.WaitAll(response);
                        var result = response.Result;
                        if (result.StatusCode != System.Net.HttpStatusCode.OK)
                            return InternalServerError();
                    }
                }
                return Ok(new { status = "succes" });
            }
            catch (Exception ex)
            {
                return new ExceptionResult(ex, this);
            }
        }
    }
}
