using AsyncRPGSharedLib.Web.Interfaces;

namespace AsyncRPGSharedLib.Web.Modules
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class RestModuleName : System.Attribute
    {
        public string Name { get; private set; }

        public RestModuleName(string name)
        {
            this.Name = name;
        }
    }

    public class RestModule
    {
        public ICacheAdapter Application { get; private set; }
        public ISessionAdapter Session { get; private set; }
        public IResponseAdapter Response { get; private set; }

        public RestModule()
        {
            // I don't expect this case to get called
            this.Application = null;
            this.Session = null;
            this.Response = null;
        }

        public RestModule(
            ICacheAdapter appCache,
            ISessionAdapter session, 
            IResponseAdapter response)
        {
            this.Application = appCache;
            this.Session = session;
            this.Response = response;
        }
    }
}
