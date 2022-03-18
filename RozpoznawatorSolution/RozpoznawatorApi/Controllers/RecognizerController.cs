using System;
using System.IO;
using System.Web;
using System.Web.Http;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using RozpoznawatorApi.Recognizer;
using System.Threading;

namespace RozpoznawatorApi.Controllers
{
    [RoutePrefix("api/recognizer")]
    public class RecognizerController : ApiController
    {

        [HttpGet]
        public IHttpActionResult Get() => Ok("Ping");

        [HttpGet]
        [Route("{guid:Guid}")]
        public HttpResponseMessage Get(Guid guid)
        {
            var upload = HttpContext.Current.Server.MapPath("~/Processed");
            var sciezkaObrazu = Path.Combine(upload, guid.ToString() + ".png");
            var isFound = false;
            var counter = 0;
            do
            {
                if (!File.Exists(sciezkaObrazu))
                    Thread.Sleep(5000);
                else
                {
                    isFound = true;
                    break;
                }
            }
            while (++counter < 10);
            if (!isFound)
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            var bajty = File.ReadAllBytes(sciezkaObrazu);
            var rezultat = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(bajty)
            };
            rezultat.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment")
                {
                    FileName = $"{guid}.png"
                };
            rezultat.Content.Headers.ContentType =
                new MediaTypeHeaderValue("image/png");
            return rezultat;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Post()
        {
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            var root = HttpContext.Current.Server.MapPath("~/App_Data");
            var upload = HttpContext.Current.Server.MapPath("~/Upload");
            var processed = HttpContext.Current.Server.MapPath("~/Processed");
            var provider = new MultipartFormDataStreamProvider(root);
            await Request.Content.ReadAsMultipartAsync(provider);
            var guid = string.Empty;
            foreach (var key in provider.FormData.AllKeys)
                foreach (var val in provider.FormData.GetValues(key))
                    if (key == "guid")
                        guid = val;
            var sciezkaObrazu = string.Empty;
            var sciezkaWzorca = string.Empty;
            var sciezkaObrobione = string.Empty;
            foreach (var file in provider.FileData)
            {
                var name = file.Headers.ContentDisposition.Name.Replace(@"""", string.Empty);
                var fileName = file.Headers.ContentDisposition.FileName.Replace(@"""", string.Empty);
                byte[] bytes = null;
                using (var reader = new StreamReader(file.LocalFileName))
                using (var memoryStream = new MemoryStream())
                {
                    var buffer = new byte[512];
                    var bytesRead = default(int);
                    while ((bytesRead = reader.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                        memoryStream.Write(buffer, 0, bytesRead);
                    bytes = memoryStream.ToArray();
                    if (name == "analiza")
                    {
                        sciezkaObrazu = Path.Combine(upload, $"{guid}.png");
                        sciezkaObrobione = Path.Combine(processed, $"{guid}.png");
                    }
                    else
                        sciezkaWzorca = Path.Combine(upload, $"{name}.png");
                    File.WriteAllBytes(Path.Combine(upload, (name == "analiza" ? guid : name) + ".png"), bytes);
                }
            }
            new SilnikRozpoznawania().Procesuj(sciezkaWzorca, sciezkaObrazu, sciezkaObrobione);
            return Request.CreateResponse(HttpStatusCode.Created);
        }
    }
}
