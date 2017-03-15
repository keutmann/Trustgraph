using Newtonsoft.Json;
using System.Web.Http;
using TrustgraphCore.Service;

namespace TrustgraphCore.Controllers
{
    public class ResolveController : ApiController
    {

        public IGraphSearch Service { get; set; }

        public ResolveController(IGraphSearch service)
        {
            Service = service;
        }

        // GET api/
        [HttpGet]
        public string Get([FromUri]string id)
        {
            var query = new GraphQuery();
            //query.Issuer = 

            var result = Service.Query(query);

            return JsonConvert.SerializeObject(result);
        }

    }
}
