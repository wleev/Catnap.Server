using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace Catnap.Server
{
    public sealed class HttpRequest
    {
        public HttpMethod Method { get; private set; }
        public Uri Path { get; private set; }
        public HttpVersion Version { get; private set; }
        public Dictionary<string, string> Headers { get; private set; }
        public string Content { get; private set; }
        
        public HttpRequest()
        {
            Headers = new Dictionary<string, string>();
        }

        public HttpRequest(HttpMethod method, Uri path, HttpVersion version, Dictionary<string, string> headerDictionary, string content)
        {
            Version = version;
            Method = method;
            Path = path;
            Headers = headerDictionary;
            Content = content;
        }

        public static HttpRequest Read(StreamSocket socket)
        {
            HttpRequest request = new HttpRequest();

            using (var input = socket.InputStream)
            {
                using (var reader = new StreamReader(input.AsStreamForRead()))
                {
                    var requestHeader = reader.ReadLine();

                    var headerSegments = requestHeader.Split(' ');
                    request.Method = new HttpMethod(headerSegments[0]);
                    request.Path = new Uri(headerSegments[1], UriKind.RelativeOrAbsolute);
                    request.Version = GetHttpVersion(headerSegments[2]);

                    if (request.Version.Equals(HttpVersion.Http10))
                        request.Headers.Add("Host", $"{socket.Information.LocalAddress}:{socket.Information.LocalPort}");


                    ParseRequest(reader, request);

                    if (!request.Path.IsAbsoluteUri)
                        request.Path = new UriBuilder("http", socket.Information.LocalAddress.ToString(), int.Parse(socket.Information.LocalPort), request.Path.OriginalString).Uri;
                }
            }

            return request;
        }

        private static void ParseRequest(StreamReader reader, HttpRequest request)
        {
            bool finishedParsingHeaders = false;

            while(true)
            {
                string line = "";
                if (!finishedParsingHeaders)
                    line = reader.ReadLine();
                else
                {
                    int contentLength = request.Headers.ContainsKey("Content-Length") ? int.Parse(request.Headers["Content-Length"]) : 0;
                    if (contentLength > 0)
                    {
                        char[] byteContent = new char[contentLength];
                        reader.ReadBlock(byteContent, 0, contentLength);

                        line = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(byteContent));
                        request.Content = line;
                    }
                    break;
                }

                if (String.IsNullOrWhiteSpace(line))
                {
                    finishedParsingHeaders = true;
                }
                else
                {
                    if (!finishedParsingHeaders)
                    {
                        var splitHeader = line.Split(new char[] { ':' }, 2);
                        request.Headers.Add(splitHeader[0].Trim(), splitHeader[1].Trim());
                    }                        
                }
            }
        } 

        private static HttpVersion GetHttpVersion(string httpVersion)
        {
            switch (httpVersion)
            {
                case "HTTP/1.0":
                    return HttpVersion.Http10;
                case "HTTP/1.1":
                    return HttpVersion.Http11;
                case "HTTP/2.0":
                    return HttpVersion.Http20;
                default:
                    return HttpVersion.None;
            }
        }
    }
}