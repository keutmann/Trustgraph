using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using TrustchainCore.Business;
using TrustchainCore.Configuration;
using TrustgraphCore.Service;
using TrustchainCore.Extensions;
using System.Net;

namespace TrustgraphCore.Controllers
{
    public class TrustController : ApiController
    {
        public const string Path = "/api/trust/";

        public IGraphBuilder Builder { get; set; }

        public TrustController(IGraphBuilder builder)
        {
            Builder = builder;
        }

        [HttpGet]
        public IHttpActionResult Get()
        {
            return Ok("OK");
        }

        [HttpPost]
        public IHttpActionResult Add(HttpRequestMessage requrest)
        {
            try
            {
                var content = requrest.Content.ReadAsStringAsync().Result;
                var package = new TrustBuilder(content).Verify().Package;

                Builder.Append(package);

                var buildserverUrl = App.Config["buildserver"].ToStringValue();
                if (!string.IsNullOrEmpty(buildserverUrl))
                {
                    var fullUrl = new UriBuilder(buildserverUrl);
                    fullUrl.Path = Path;
                    using (WebClient web = new WebClient())
                    {
                        web.UploadString(fullUrl.ToString(), content);
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return new ExceptionResult(ex, this);
            }
        }
    }
}
