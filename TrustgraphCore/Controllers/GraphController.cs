using System.Web.Http;
using TrustgraphCore.Service;

namespace TrustgraphCore.Controllers
{
    public class GraphController : ApiController
    {
        public const string Path = "/api/graph/";

        public IGraphDump Service { get; set; }

        public GraphController(IGraphDump service)
        {
            Service = service;
        }

        [HttpGet]
        public IHttpActionResult Get()
        {
            var result = Service.GetFullGraph();

            return Ok(result);
        }
    }
}
