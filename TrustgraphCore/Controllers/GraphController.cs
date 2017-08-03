using System.Web.Http;
using TrustgraphCore.Service;

namespace TrustgraphCore.Controllers
{
    public class GraphController : ApiController
    {
        public const string Path = "/api/graph/";

        public IGraphExport Service { get; set; }

        public GraphController(IGraphExport service)
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
