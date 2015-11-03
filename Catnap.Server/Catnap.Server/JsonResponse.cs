using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Windows.Web.Http;

namespace Catnap.Server
{
    public class JsonResponse : HttpResponse
    {
        protected JsonResponse() : base()
        {
            this.Headers.Add("Content-Type", "application/json");
            this.Headers.Add("Accept", "application/json");
        }

        public JsonResponse(String jsonString, HttpStatusCode statusCode = HttpStatusCode.Ok) 
            : this()
        {
            var jsonObject = JsonConvert.DeserializeObject(jsonString);

            this.StatusCode = statusCode;
            this.Content = jsonObject.ToString();
        }

        public JsonResponse(object jsonObject, HttpStatusCode statusCode = HttpStatusCode.Ok)
            : this()
        {
            var jsonString = JsonConvert.SerializeObject(jsonObject);

            this.StatusCode = statusCode;
            this.Content = jsonString;
        }
    }
}
