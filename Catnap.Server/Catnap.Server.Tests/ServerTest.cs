using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Windows.Web.Http;
using Windows.System.Threading;
using Windows.Foundation;
using Catnap.Server;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Catnap.Server.Tests
{
    [RoutePrefix("unittest")]
    class TestController : Controller
    {
        [HttpGet]
        [Route]
        public HttpResponse Get()
        {
            return new HttpResponse(HttpStatusCode.Ok, "Test#1");
        }

        [HttpGet]
        [Route("routedget")]
        public HttpResponse Method()
        {
            return new HttpResponse(HttpStatusCode.Ok, "Test#2");
        }

        [HttpGet]
        [Route("getwithparam/{param1}")]
        public HttpResponse WithParam(string param1)
        {
            return new HttpResponse(HttpStatusCode.Ok, $"Test#3:{param1}");
        }

        [HttpPost]
        [Route]
        public HttpResponse Post([Body] string postContent)
        {
            return new HttpResponse(HttpStatusCode.Ok, $"Test#4:{postContent}");
        }

        [HttpPost]
        [Route("postwithparam/{param1}")]
        public HttpResponse Post(string param1, [Body] string postContent)
        {
            return new HttpResponse(HttpStatusCode.Ok, $"Test#5:{param1}:{postContent}");
        }

        [HttpPost]
        [Route("jsonbody")]
        public HttpResponse JsonPost([JsonBody]string body)
        {
            //return new HttpResponse(HttpStatusCode.Ok, $"received json: {body}");
            return new JsonResponse(body);
        }

        [HttpGet]
        [Route("jsonobject")]
        public HttpResponse JsonObject()
        {
            return new JsonResponse(new { id = 1337, child = new { childprop1 = "testprop", childprop2 = new int[] { 1, 2, 3, 4 } } });
        }

        [HttpGet]
        [Route("gettwoparams/{param1}/{param2}")]
        public HttpResponse TwoParams(string param2, string param1)
        {
            return new HttpResponse(HttpStatusCode.Ok, $"Test8:{param1}:{param2}");
        }
    }


    [TestClass]
    public class ServerTest
    {
        private static IAsyncAction ServerTask;
        private static HttpClient Client;

        [ClassInitialize]
        public static void Initalize(TestContext context)
        {
            var httpServer = new HttpServer();
            httpServer.restHandler.RegisterController(new TestController());

            ServerTask = 
                ThreadPool.RunAsync((w) =>
                {
                    httpServer.StartServer();
                });

            Client = new HttpClient();
        }


        [TestMethod]
        public async Task TestPlainGet()
        {
            Uri requestUri = new UriBuilder("http", "localhost", 1337, "unittest").Uri;

            var response = await Client.GetAsync(requestUri);
            var contentString = await response.Content.ReadAsStringAsync();

            Assert.AreEqual(response.StatusCode, HttpStatusCode.Ok);
            Assert.AreEqual(contentString, "Test#1");
        }

        [TestMethod]
        public async Task TestRoutedGet()
        {
            Uri requestUri = new UriBuilder("http", "localhost", 1337, "unittest/routedget").Uri;

            var response = await Client.GetAsync(requestUri);
            var contentString = await response.Content.ReadAsStringAsync();

            Assert.AreEqual(response.StatusCode, HttpStatusCode.Ok);
            Assert.AreEqual(contentString, "Test#2");
        }

        [TestMethod]
        public async Task TestRoutedGetWithParameter()
        {
            string parameter = "testPARAM";
            Uri requestUri = new UriBuilder("http", "localhost", 1337, $"unittest/getwithparam/{parameter}").Uri;

            var response = await Client.GetAsync(requestUri);
            var contentString = await response.Content.ReadAsStringAsync();

            Assert.AreEqual(response.StatusCode, HttpStatusCode.Ok);
            Assert.AreEqual(contentString, $"Test#3:{parameter}");
        }

        [TestMethod]
        public async Task TestPost()
        {
            string testContent = "testPOSTcontent";
            Uri requestUri = new UriBuilder("http", "localhost", 1337, "unittest").Uri;

            var response = await Client.PostAsync(requestUri, new HttpStringContent(testContent));
            var contentString = await response.Content.ReadAsStringAsync();

            Assert.AreEqual(response.StatusCode, HttpStatusCode.Ok);
            Assert.AreEqual(contentString, $"Test#4:{testContent}");
        }

        [TestMethod]
        public async Task TestJsonPost()
        {
            var json = new { test = "123", map = new { one = 1, two = 2 } };
            string testContent = JsonConvert.SerializeObject(json);
            Uri requestUri = new UriBuilder("http", "localhost", 1337, "unittest/jsonbody").Uri;

            var response = await Client.PostAsync(requestUri, new HttpStringContent(testContent));
            var contentString = await response.Content.ReadAsStringAsync();

            Assert.AreEqual(response.StatusCode, HttpStatusCode.Ok);
            Assert.IsTrue(JObject.DeepEquals(JObject.Parse(contentString), JObject.FromObject(json)));
        }

        [TestMethod]
        public async Task TestPostWithParameter()
        {
            string param1 = "test123";
            string testContent = "testPOSTcontent";
            Uri requestUri = new UriBuilder("http", "localhost", 1337, $"unittest/postwithparam/{param1}").Uri;

            var response = await Client.PostAsync(requestUri, new HttpStringContent(testContent));
            var contentString = await response.Content.ReadAsStringAsync();

            Assert.AreEqual(response.StatusCode, HttpStatusCode.Ok);
            Assert.AreEqual(contentString, $"Test#5:{param1}:{testContent}");
        }

        [TestMethod]
        public async Task TestJsonGet()
        {
            var expectedJson = new { id = 1337, child = new { childprop1 = "testprop", childprop2 = new int[] { 1, 2, 3, 4 } } };
            Uri requestUri = new UriBuilder("http", "localhost", 1337, "unittest/jsonobject").Uri;

            var response = await Client.GetAsync(requestUri);
            var contentString = await response.Content.ReadAsStringAsync();

            Assert.AreEqual(response.StatusCode, HttpStatusCode.Ok);
            Assert.IsTrue(JObject.DeepEquals(JObject.Parse(contentString), JObject.FromObject(expectedJson)));
        }

        [TestMethod]
        public async Task TestRoutedTwoParams()
        {
            string param1 = "testPARAM";
            string param2 = "PARAM2";
            Uri requestUri = new UriBuilder("http", "localhost", 1337, $"unittest/gettwoparams/{param1}/{param2}").Uri;

            var response = await Client.GetAsync(requestUri);
            var contentString = await response.Content.ReadAsStringAsync();

            Assert.AreEqual(response.StatusCode, HttpStatusCode.Ok);
            Assert.AreEqual(contentString, $"Test8:{param1}:{param2}");
        }


        [ClassCleanup]
        public static void Destroy()
        {
            ServerTask.Cancel();
        }
    }
}
