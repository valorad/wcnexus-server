using Microsoft.AspNetCore.Mvc;
using WCNexus.App.Database;
using WCNexus.App.Models;

namespace WCNexus.App.Services
{
    public class NexusService: DataAccessService<Nexus>, INexusService
    {
        public NexusService(IDBCollection collection): base(collection.Nexuses)
        {
            this.indexFieldName = "dbname";
        }
        
    }
}