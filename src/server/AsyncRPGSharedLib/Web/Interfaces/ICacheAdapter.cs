namespace AsyncRPGSharedLib.Web.Interfaces
{
    public interface ICacheAdapter
    {
        object this[string name] { get; set; }
        void Add(string name, object value);
        void Clear();
        void Remove(string name);
        void RemoveAll();
    }
}
