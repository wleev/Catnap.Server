# Catnap.Server ![Build status](https://ci.appveyor.com/api/projects/status/github/wleev/Catnap.Server?svg=true)
Lightweight REST server built with Windows 10 IOT Core in mind. Configuration created to resemble MVC and WebAPI attribute based configuration.

## Example

### Creating a new controller

Create a new class that interits from ''Controller and implement REST-like paths using an attribute based system.

```csharp
[RoutePrefix("test")]
public class TestController : Controller
{
	[HttpGet]
	[Route]
	public HttpResponse Get()
	{
		return new HttpResponse(HttpStatusCode.Ok, "Testing");
	}

	[HttpGet]
	[Route("method")]
	public HttpResponse Method()
	{
		return new HttpResponse(HttpStatusCode.Ok, "<h1>Method</h1>");
	}

	[HttpGet]
	[Route("withparam/{param1}")]
	public HttpResponse WithParam(string param1)
	{
		return new HttpResponse(HttpStatusCode.Ok, $"got param1: {param1}");
	}

	[HttpPost]
	[Route]
	public HttpResponse Post([Body]string test)
	{
		return new HttpResponse(HttpStatusCode.Ok, $"Posted:{test}");
	}

	[HttpPost]
	[Route("withbody")]
	public HttpResponse JsonPost([JsonBody]string body)
	{
		return new JsonResponse(body);
	}

	[HttpGet]
	[Route("jsonobject")]
	public HttpResponse JsonObject()
	{
		return new JsonResponse(new { id = 1337, child = new { childprop1 = "testprop", childprop2 = new int[] { 1, 2, 3, 4 } } });
	}
}
```

### Starting the server

Create an instance of the server optionally specifying the port on which to run. Then add Controllers to the RestHandler. Finally start up the server ( preferably in an asynchronous task).

```csharp
var httpServer = new HttpServer();
httpServer.RestHandler.RegisterController(new TestController());

ServerTask =
	ThreadPool.RunAsync((w) =>
	{
		httpServer.StartServer();
	});
```

## Installation

No NuGet package available yet. For now just add the project to your solution and reference it in the projects you need.

## License

This library is released under the MIT license. For more details see the LICENSE file.
