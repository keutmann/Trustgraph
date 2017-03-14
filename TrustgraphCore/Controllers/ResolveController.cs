using System.Web.Http;
using TrustpathCore.Service;

namespace TrustpathCore.Controllers
{
    public class ResolveController : ApiController
    {

        public IGraphService Service { get; set; }

        public ResolveController(IGraphService service)
        {
            Service = service;
        }

        // GET api/
        [HttpGet]
        public string Get([FromUri]string id)
        {
            var result = Service.Resolve(null);
            return result.Result + id;
        }

    }
}
