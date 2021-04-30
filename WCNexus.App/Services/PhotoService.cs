using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WCNexus.App.Database;
using WCNexus.App.Models;

namespace WCNexus.App.Services
{
    public class PhotoService: DataAccessService<Photo>, IPhotoService
    {
        public PhotoService(IDBCollection collection): base(
            collection: collection.Photos,
            indexFieldName: "dbname"
        ){}

        // The Add methods in this service aim to convert inputModel to Model in DB

       public async Task<CUDMessage> Add(InputPhoto newPhoto)
        {
            return await base.Add(new Photo()
            {
                DBName = string.IsNullOrWhiteSpace(newPhoto.DBName)? $"photo-{Guid.NewGuid()}": newPhoto.DBName,
                Name = newPhoto.Name,
                Description = newPhoto.Description,
                URL = newPhoto.URL,
            });
        }

        public async Task<CUDMessage> Add(IEnumerable<InputPhoto> newPhotos)
        {

            return await base.Add(
                from photo in newPhotos
                select new Photo()
                {
                    DBName = string.IsNullOrWhiteSpace(photo.DBName)? $"photo-{Guid.NewGuid()}": photo.DBName,
                    Name = photo.Name,
                    Description = photo.Description,
                    URL = photo.URL,
                }
            );

        }
        
    }
}