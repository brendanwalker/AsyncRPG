using AsyncRPGSharedLib.Web.Interfaces;
using System.Web;

namespace AsyncRPGWebService.Environment
{
    public class HttpResponseAdapter : IResponseAdapter
    {
        private HttpResponse m_reponse;

        public HttpResponseAdapter(HttpResponse response)
        {
            m_reponse = response;
        }

        // IResponseAdapter
        public int StatusCode 
        {
            get
            {
                return m_reponse.StatusCode;
            }

            set
            {
                m_reponse.StatusCode = value;
            }
        }

        public void AddHeader(string name, string value)
        {
            m_reponse.AddHeader(name, value);
        }
    }
}