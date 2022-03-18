using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace RozpoznawatorApi.Controllers
{
    [RoutePrefix("api/ocr")]
    public class OcrController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Get() => Ok("Ping");
        [HttpGet]
        [Route("{guid:Guid}")]
        public HttpResponseMessage Get(Guid guid) => null;
        [HttpPost]
        public async Task<HttpResponseMessage> Post() => 
            Request.CreateResponse(HttpStatusCode.Created);
    }
}