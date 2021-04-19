using Microsoft.AspNetCore.Mvc;
using WCNexus.App.Database;
using WCNexus.App.Models;

namespace WCNexus.App.Services
{
    public class StoredProjectService: DataAccessService<StoredProject>, IStoredProjectService
    {
        public StoredProjectService(IDBCollection collection): base(
            collection: collection.Projects,
            indexFieldName: "dbname"
        ){}
        
    }
}