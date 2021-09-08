using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using MongoDB.Driver;
using WCNexus.App.Models;

namespace WCNexus.App.Services
{
    public interface IProjectService
    {
        Task<JointProject> Get(string dbname, IDBViewOption options = null); 
        Task<IEnumerable<JointProject>> Get(FilterDefinition<JointProject> projectCondition, IDBViewOption options = null);
        Task<IEnumerable<CUDMessage>> Add(InputProject newProject);
        Task<IEnumerable<CUDMessage>> Add(IEnumerable<InputProject> newProjects);
        Task<IEnumerable<CUDMessage>> Update(string dbname, UpdateDefinition<JointProject> token);
        Task<IEnumerable<CUDMessage>> Update(FilterDefinition<JointProject> condition, UpdateDefinition<JointProject> token);
        Task<IEnumerable<CUDMessage>> Delete(string dbname);
        Task<IEnumerable<CUDMessage>> Delete(FilterDefinition<JointProject> projectCondition);
        Task<CUDMessage> AddTechnology(string techDBName, string projectDBName);
        Task<CUDMessage> AddTechnology(IEnumerable<string> techDBNames, string projectDBName);
        Task<CUDMessage> RemoveTechnology(string techDBName, string projectDBName);
        Task<CUDMessage> RemoveTechnology(IEnumerable<string> techDBNames, string projectDBName);
    }
}