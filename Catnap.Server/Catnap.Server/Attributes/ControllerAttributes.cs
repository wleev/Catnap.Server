using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace Catnap.Server
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class Body : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class JsonBody : Body
    {

    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RoutePrefix : Attribute
    {
        private string path;

        public RoutePrefix(string path)
        {
            this.path = path;
        }

        public virtual string Path
        {
            get { return this.path; }
        }
    }


    [AttributeUsage(AttributeTargets.Method, AllowMultiple =false)]
    public class Route : Attribute
    {
        private string path;

        public Route(string path)
        {
            this.path = path;
        }

        public Route()
        {
            this.path = String.Empty;
        }


        public virtual string Path
        {
            get { return this.path; }
        }

    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public abstract class HttpRequestMethod : Attribute
    {
        public virtual HttpMethod Method { get; }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class HttpGet : HttpRequestMethod
    {
        public override HttpMethod Method { get; } = HttpMethod.Get;
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class HttpPost : HttpRequestMethod
    {
        public override HttpMethod Method { get; } = HttpMethod.Post;
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class HttpDelete : HttpRequestMethod
    {
        public override HttpMethod Method { get; } = HttpMethod.Delete;
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class HttpPut : HttpRequestMethod
    {
        public override HttpMethod Method { get; } = HttpMethod.Put;
    }

}
