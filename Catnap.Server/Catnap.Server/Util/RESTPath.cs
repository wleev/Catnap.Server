using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Catnap.Server.Util
{
    public class RESTPath
    {
        public String Path { get; set; } = "";
        public bool ContainsParameters { get; set; } = false;
        public Dictionary<int, string> Parameters = new Dictionary<int, string>();

        private RESTPath()
        {

        }


        public static RESTPath Combine(params string[] paths)
        {
            var restPath = new RESTPath();
            var segments = paths.SelectMany(p => p.Split('/'));

            String path = "";
            int segmentCount = 0;
            foreach(var segment in segments)
            {
                if (segment == String.Empty)
                    continue;

                var trimSegment = segment.Trim('/');
                if (trimSegment.StartsWith("{") && trimSegment.EndsWith("}"))
                {
                    restPath.Parameters.Add(segmentCount, trimSegment.Trim('{', '}'));
                    path += "/[a-zA-Z0-9]+"; 
                } 
                else
                    path += "/" + trimSegment;

                segmentCount++;
            }
            if (restPath.Parameters.Count > 0)
                restPath.ContainsParameters = true;

            restPath.Path = path;

            return restPath;
        }

        public bool Matches(String path)
        {
            var regex = new Regex($"^{Path}/?$");

            return regex.IsMatch(path);
        }
    }
}
