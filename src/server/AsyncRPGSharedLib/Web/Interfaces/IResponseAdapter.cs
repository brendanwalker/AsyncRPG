namespace AsyncRPGSharedLib.Web.Interfaces
{
    public interface IResponseAdapter
    {
        int StatusCode { get; set; }
        void AddHeader(string key, string value);
    }
}
