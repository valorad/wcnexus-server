using WCNexus.App.Models;
using MongoDB.Driver;

namespace WCNexus.App.Database
    {
    public class DBCollection : IDBCollection
    {
        private readonly IDBContext context;
        private readonly IMongoDatabase dbInstance;

        public DBCollection(IDBContext context)
        {
            this.context = context;
            this.dbInstance = context.GetDatabase();
        }

        public IMongoCollection<Nexus> Nexuses => dbInstance.GetCollection<Nexus>("nexuses");
        
    }
}