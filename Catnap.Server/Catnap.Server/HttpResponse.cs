using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace Catnap.Server
{
    public class HttpResponse
    {
        public HttpStatusCode StatusCode { get; protected set; } = HttpStatusCode.Ok;
        public Dictionary<string, string> Headers { get;  } = new Dictionary<string, string>();
        public string Content { get; protected set; } = string.Empty;

        protected HttpResponse() { }

        public HttpResponse(HttpStatusCode statusCode)
            : this()
        {
            StatusCode = statusCode;
        }

        public HttpResponse(HttpStatusCode statusCode, string content)
            : this(statusCode)
        {
            Content = content;
        }

        public HttpResponse(HttpStatusCode statusCode, Dictionary<string, string> headers, string content)
            : this(statusCode, content)
        {
            Headers = headers;
        }

        public async Task WriteToStream(Stream stream)
        {
            var bodyArray = Encoding.UTF8.GetBytes(Content);
            var contentStream = new MemoryStream(bodyArray);

            var headerBuilder = new StringBuilder();
            headerBuilder.AppendLine($"HTTP/1.1 {(int)StatusCode} {StatusCode}");
            headerBuilder.AppendLine("Server: catnap-srv/1.0.0");

            foreach (var header in Headers)
            {
                headerBuilder.AppendLine($"{header.Key}: {header.Value}");
            }

            headerBuilder.AppendLine($"Content-Length: {contentStream.Length}");
            headerBuilder.AppendLine("Connection: close");
            headerBuilder.AppendLine();

            var headerArray = Encoding.UTF8.GetBytes(headerBuilder.ToString());
            await stream.WriteAsync(headerArray, 0, headerArray.Length);
            await contentStream.CopyToAsync(stream);
            await stream.FlushAsync();
        }
    }
}