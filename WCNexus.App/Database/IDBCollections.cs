using WCNexus.App.Models;
using MongoDB.Driver;

namespace WCNexus.App.Database
{
    public interface IDBCollection
    {
        IMongoCollection<Nexus> Nexuses { get; }
        IMongoCollection<StoredProject> Projects { get; }
        IMongoCollection<Photo> Photos { get; }
    }
}