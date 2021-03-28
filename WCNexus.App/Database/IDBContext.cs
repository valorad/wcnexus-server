using MongoDB.Driver;

namespace WCNexus.App.Database
{
    public interface IDBContext
    {
        bool Drop();
        IMongoDatabase GetDatabase();
    }
}