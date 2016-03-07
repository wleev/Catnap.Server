using Catnap.Server.Handlers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.Web.Http;

namespace Catnap.Server
{
    public sealed class HttpServer : IDisposable
    {
        private readonly StreamSocketListener listener;
        private readonly int port;
        public RESTHandler restHandler { get; } = new RESTHandler();

        private List<String> AcceptedVerbs = new List<String> { HttpMethod.Get.Method, HttpMethod.Post.Method,
                                                                HttpMethod.Delete.Method, HttpMethod.Put.Method };

        public HttpServer(int serverPort = 1337)
        {
            listener = new StreamSocketListener();
            port = serverPort;
            listener.ConnectionReceived += (s, e) => ThreadPool.RunAsync((w) => ProcessRequestAsync(e.Socket));
        }

        public async void StartServer()
        {
            await listener.BindServiceNameAsync(port.ToString());
        }

        private async void ProcessRequestAsync(StreamSocket socket)
        {
            HttpRequest request;
            try
            {
                request = HttpRequest.Read(socket);
            }
            catch (Exception ex)
            {
                await WriteInternalServerErrorResponse(socket, ex);
                return;
            }

            if (AcceptedVerbs.Contains(request.Method.Method))
            {
                HttpResponse response;
                try
                {
                    response = await restHandler.Handle(request);
                }
                catch (Exception ex)
                {
                    await WriteInternalServerErrorResponse(socket, ex);
                    return;
                }
                await WriteResponse(response, socket);

                await socket.CancelIOAsync();
                socket.Dispose();
            }
        }

        private static async Task WriteInternalServerErrorResponse(StreamSocket socket, Exception ex)
        {
            var httpResponse = GetInternalServerError(ex);
            await WriteResponse(httpResponse, socket);
        }

        private static HttpResponse GetInternalServerError(Exception exception)
        {
            var errorMessage = "Internal server error occurred.";
            if (Debugger.IsAttached)
                errorMessage += Environment.NewLine + exception;

            var httpResponse = new HttpResponse(HttpStatusCode.InternalServerError, errorMessage);
            return httpResponse;
        }

        private static async Task WriteResponse(HttpResponse response, StreamSocket socket)
        {
            var output = socket.OutputStream;
            using (var stream = output.AsStreamForWrite())
            {
                await response.WriteToStream(stream);
            }
        }

        public void Dispose()
        {
            listener.Dispose();
        }
    }
}