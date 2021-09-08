using System.Collections.Generic;
using System.Threading.Tasks;
using WCNexus.App.Models;

namespace WCNexus.App.Services
{
    public interface INexusService: IDataAccessService<Nexus>
    {
        Task<CUDMessage> Add(InputNexus newNexus);
        Task<CUDMessage> Add(IEnumerable<InputNexus> newNexuses);
        Task<CUDMessage> AddImage(string imageDBName, string projectDBName);
        Task<CUDMessage> AddImage(IEnumerable<string> imageDBNames, string projectDBName);
        Task<CUDMessage> RemoveImage(string imageDBName, string projectDBName);
        Task<CUDMessage> RemoveImage(IEnumerable<string> imageDBNames, string projectDBName);
    }
}