using System.Collections.Generic;
using System.Threading.Tasks;
using WCNexus.App.Models;

namespace WCNexus.App.Services
{
    public interface INexusService: IDataAccessService<Nexus>
    {
        Task<CUDMessage> Add(InputNexus newNexus);
        Task<CUDMessage> Add(IEnumerable<InputNexus> newNexuses);
    }
}