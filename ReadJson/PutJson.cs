using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace ReadJson
{
    public class Extracted
    {
        public static  string Data { get; set; }
    }
    public static class PutJson
    {
        [FunctionName("PutResultJson")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "PutResultJson/{Id}")] HttpRequest req, string Id,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request.{Id}");        

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            HttpResponseMessage response = new HttpResponseMessage()
            {
               StatusCode = System.Net.HttpStatusCode.OK
            };
            log.LogInformation($"Read Json:\n\t:{data.ToString()}");
            Extracted.Data = data.ToString();
            return new OkObjectResult(response);
        }
    }
}
