using System.Collections.Generic;
using System.Threading.Tasks;
using WCNexus.App.Models;

namespace WCNexus.App.Services
{
    public interface IPhotoService: IDataAccessService<Photo>
    {
        Task<CUDMessage> Add(InputPhoto newPhoto);
        Task<CUDMessage> Add(IEnumerable<InputPhoto> newPhotos);
    }
}