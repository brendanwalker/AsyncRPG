using System.Text;
using System;
using System.Net;
using System.IO;
using System.Collections.Specialized;

namespace AsyncRPGSharedLib.Web
{
    public class RestRequest
    {
        private static char[] AMPERSAND_SEPERATOR = new char[] { '&' };
        private static char[] EQUALS_SEPERATOR = new char[] { '=' };

        public string Body { get; private set; }
        public long ContentLength { get; private set; }
        public string ContentType { get; private set; }
        public string HttpMethod { get; private set; }
        public Uri Url { get; private set; }
        public string RESTModuleName { get; private set; }
        public string RESTMethodName { get; private set; }
        public NameValueCollection RESTMethodParameters { get; private set; }
        public CookieCollection Cookies { get; private set; }

        public RestRequest(HttpListenerRequest request)
        {
            this.HttpMethod = request.HttpMethod;
            this.Url = request.Url;
            this.RESTModuleName = request.Url.Segments[1].Replace("/", "");
            this.RESTMethodName = request.Url.Segments[2].Replace("/", "");
            this.RESTMethodParameters = request.QueryString;
            this.Cookies = request.Cookies;
                
            if (request.HasEntityBody)
            {
                Encoding encoding = request.ContentEncoding;

                using (var bodyStream = request.InputStream)
                using (var streamReader = new StreamReader(bodyStream, encoding))
                {
                    if (request.ContentType != null)
                    {
                        this.ContentType = request.ContentType;
                    }

                    this.ContentLength = request.ContentLength64;
                    this.Body = streamReader.ReadToEnd();
                }

                if (this.HttpMethod == "POST" && this.ContentType == "application/x-www-form-urlencoded")
                {
                    this.RESTMethodParameters = ParseQueryString(System.Uri.UnescapeDataString(this.Body));
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("HttpMethod {0}", HttpMethod));
            sb.AppendLine(string.Format("Url {0}", Url));
            
            {
                StringBuilder args = new StringBuilder();

                foreach (string key in RESTMethodParameters.AllKeys)
                {
                    string value = RESTMethodParameters[key];

                    if (args.Length > 0)
                    {
                        args.AppendFormat(", {0}={1}", key, value);
                    }
                    else
                    {
                        args.AppendFormat("{0}={1}", key, value);
                    }
                }

                sb.AppendLine(string.Format("RESTMethod: {0}/{1}({2})",
                    RESTModuleName, RESTMethodName, args.ToString()));
            }

            sb.AppendLine(string.Format("ContentType {0}", ContentType));
            sb.AppendLine(string.Format("ContentLength {0}", ContentLength));
            sb.AppendLine(string.Format("Body {0}", Body));

            return sb.ToString();
        }

        private static NameValueCollection ParseQueryString(string queryString)
        {
            NameValueCollection collection = new NameValueCollection();
            string[] keyValuePairs = queryString.Split(AMPERSAND_SEPERATOR);

            foreach (string keyValuePair in keyValuePairs)
            {
                string[] pair = keyValuePair.Split(EQUALS_SEPERATOR);

                if (pair.Length == 2)
                {
                    collection.Add(pair[0], pair[1]);
                }
            }

            return collection;
        }
    }
}
