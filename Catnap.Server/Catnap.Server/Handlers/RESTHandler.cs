using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Web.Http;

namespace Catnap.Server.Handlers
{
    public class RESTHandler : RequestHandler
    {
        private List<Controller> Controllers = new List<Controller>();

        public RESTHandler()
        {
        }

        public void RegisterController(Controller controller)
        {
            Controllers.Add(controller);
        }

        public async override Task<HttpResponse> Handle(HttpRequest request)
        {
            var url = request.Path;
            var controller = Controllers.SingleOrDefault(c => url.PathAndQuery.Contains(c.Prefix));

            if (controller != null)
                return await controller.Handle(request);
            else
                return new HttpResponse(HttpStatusCode.NotFound, $"No controllers found that support this path: '{ url }'.");
        }
    }
}
