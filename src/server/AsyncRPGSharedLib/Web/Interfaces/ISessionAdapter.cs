namespace AsyncRPGSharedLib.Web.Interfaces
{
    public interface ISessionAdapter : ICacheAdapter
    {
        void Abandon();
    }
}
