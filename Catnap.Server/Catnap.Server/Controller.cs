using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web;
using Windows.Web.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.IO;
using Catnap.Server.Util;

namespace Catnap.Server
{
    public abstract class Controller
    {
        public string Prefix = "";
        public List<MethodInfo> RoutingMethods = new List<MethodInfo>();

        public Controller()
        {
            var type = this.GetType().GetTypeInfo();
            var routePrefix = (RoutePrefix) this.GetType().GetTypeInfo().GetCustomAttribute(typeof(RoutePrefix));
            Prefix = routePrefix.Path;

            var methods = GetType().GetMethods().ToList();
            methods = methods.Where(m => m.GetCustomAttribute(typeof(Route)) != null).ToList();

            foreach(var m in methods)
            {
                RoutingMethods.Add(m);
            }
        }

        public async Task<HttpResponse> Handle(HttpRequest request)
        {
            var url = request.Path;
            var httpMethod = request.Method;

            foreach (var route in RoutingMethods)
            {
                var requestPath = url.AbsolutePath;

                var routingMethod = ((HttpRequestMethod)route.GetCustomAttribute(typeof(HttpRequestMethod)))?.Method ?? HttpMethod.Get;
                var routingPath = RESTPath.Combine(Prefix, ((Route)route.GetCustomAttribute(typeof(Route))).Path);

                bool sameMethod = String.Equals(routingMethod.Method, httpMethod.Method);
                bool matchingUrl = routingPath.Matches(requestPath);

                if(sameMethod && matchingUrl)
                {
                    var method = route;
                    var parameters = ExtractParameters(method, routingPath, request);

                    if (method.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null)
                        return await (Task<HttpResponse>)method.Invoke(this, parameters.ToArray());
                    else
                        return (HttpResponse)method.Invoke(this, parameters.ToArray());
                }
            }
            
            return NotFound($"Couldn't find a fitting method on the on matched controller '{ GetType().Name }' for path '{ url }'");
        }

        private List<object> ExtractParameters(MethodInfo method, RESTPath path, HttpRequest request)
        {
            var parameters = new List<object>();
            var methodParams = method.GetParameters();
            var requestSegments = request.Path.AbsolutePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            foreach(var param in methodParams)
            {
                if (param.GetCustomAttribute(typeof(Body)) != null)
                    parameters.Add(request.Content);
                else
                    parameters.Add(
                        requestSegments[path.Parameters.Single(p => p.Value.Equals(param.Name)).Key]);
            }

            return parameters;
        }

        public HttpResponse BadRequest(string message = "")
        {
            return new HttpResponse(HttpStatusCode.BadRequest, message);
        }

        public HttpResponse NotFound(string message = "")
        {
            return new HttpResponse(HttpStatusCode.NotFound, message);
        }
    }
}
