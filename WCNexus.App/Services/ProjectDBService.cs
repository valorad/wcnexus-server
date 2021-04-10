using Microsoft.AspNetCore.Mvc;
using WCNexus.App.Database;
using WCNexus.App.Models;

namespace WCNexus.App.Services
{
    public class ProjectDBService: DataAccessService<ProjectDB>, IProjectDBService
    {
        public ProjectDBService(IDBCollection collection): base(collection.Projects)
        {
            this.indexFieldName = "dbname";
        }
        
    }
}