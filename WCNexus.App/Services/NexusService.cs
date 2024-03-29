using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WCNexus.App.Database;
using WCNexus.App.Models;

namespace WCNexus.App.Services
{
    public class NexusService: DataAccessService<Nexus>, INexusService
    {

        public NexusService(IDBCollection collection): base(
            collection: collection.Nexuses,
            indexFieldName: "dbname"
        ){}

        // The Add methods in this service aim to convert inputModel to Model in DB

       public async Task<CUDMessage> Add(InputNexus newNexus)
        {
            return await base.Add(new Nexus()
            {
                DBName = string.IsNullOrWhiteSpace(newNexus.DBName)? $"nexus-{Guid.NewGuid()}": newNexus.DBName,
                Name = newNexus.Name,
                Description = newNexus.Description,
                URL = newNexus.URL,
                Type = newNexus.Type,
                Cover = newNexus.Cover,
                Avatar = newNexus.Avatar,
                Photos = newNexus.Photos ?? new List<string>(),
            });
        }

        public async Task<CUDMessage> Add(IEnumerable<InputNexus> newNexuses)
        {

            return await base.Add(
                from nexus in newNexuses
                select new Nexus()
                {
                    DBName = string.IsNullOrWhiteSpace(nexus.DBName)? $"nexus-{Guid.NewGuid()}": nexus.DBName,
                    Name = nexus.Name,
                    Description = nexus.Description,
                    URL = nexus.URL,
                    Type = nexus.Type,
                    Cover = nexus.Cover,
                    Avatar = nexus.Avatar,
                    Photos = nexus.Photos ?? new List<string>(),
                }
            );

        }

        public async Task<CUDMessage> AddImage(string imageDBName, string projectDBName)
        {
            return await AddItemToList("photos", imageDBName, projectDBName);
        }

        public async Task<CUDMessage> AddImage(IEnumerable<string> imageDBNames, string projectDBName)
        {
            return await AddItemToList("photos", imageDBNames, projectDBName);
        }

        public async Task<CUDMessage> RemoveImage(string imageDBName, string projectDBName)
        {
            return await RemoveItemFromList("photos", imageDBName, projectDBName);
        }

        public async Task<CUDMessage> RemoveImage(IEnumerable<string> imageDBNames, string projectDBName)
        {
            return await RemoveItemFromList("photos", imageDBNames, projectDBName);
        }

    }
}