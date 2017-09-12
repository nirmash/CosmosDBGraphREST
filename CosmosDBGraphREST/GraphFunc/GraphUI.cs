using System.Linq;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace GraphFunc
{
    public static class GraphUI
    {
        [FunctionName("GraphUI")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log, ExecutionContext context)
        {
            log.Info("C# HTTP trigger function processed a request.");
            //debug file location 
            //string sFilePath = context.FunctionDirectory.Replace(context.FunctionName, "content\\GraphInteract.html");
            //production file location
            string sFilePath = context.FunctionDirectory.Replace(context.FunctionName, "bin\\content\\GraphInteract.html");
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new FileStream(sFilePath, FileMode.Open);
            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}