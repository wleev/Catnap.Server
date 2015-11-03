namespace Catnap.Server.Util
{
    internal class ContentTypeMapper
    {
        public string GetContentTypeForExtension(string extension)
        {
            switch (extension)
            {
                case ".html":
                    return "text/html; charset=UTF-8";
                case ".txt":
                    return "text/plain";
                case ".css":
                    return "text/css";
                case ".js":
                    return "application/javascript";                
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".svg":
                    return "image/svg+xml";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";                
                default:
                    return string.Empty;
            }
        }
    }
}