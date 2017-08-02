using System.Web.Http;

namespace TrustgraphCore.Controllers
{
    public class PingController : ApiController
    {
        public const string Path = "/api/ping/";

        [HttpGet]
        public IHttpActionResult Get()
        {
            return Ok("OK");
        }
    }
}
