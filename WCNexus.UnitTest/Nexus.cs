using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCNexus.App.Database;
using WCNexus.App.Models;
using WCNexus.App.Services;
using Xunit;

namespace WCNexus.UnitTest
{
    [Collection("Sequential")]
    public class NexusTest : IClassFixture<ServiceFixture>, IDisposable
    {
        private readonly ServiceProvider serviceProvider;
        private readonly IDBContext dbContext;
        private readonly INexusService nexusService;

        public NexusTest(ServiceFixture fixture)
        {
            serviceProvider = fixture.ServiceProvider;
            dbContext = serviceProvider.GetService<IDBContext>();
            nexusService = serviceProvider.GetService<INexusService>();
        }
        public void Dispose()
        {
            // called after each test method
            dbContext.Drop();
        }

        [Theory(DisplayName = "Add images test")]
        [ClassData(typeof(DataSingleNexusWithImages))]
        public async void ImagesTest(InputNexus newNexus, IList<InputPhoto> photos)
        {
            // Add a nexus
            await nexusService.Add(newNexus);

            // Extract image dbnames
            List<string> imageDBNames = (
                from photo in photos
                select photo.DBName
            ).ToList();

            // Add 1 image
            CUDMessage message = await nexusService.AddImage(imageDBNames[0], newNexus.DBName);
            Assert.True(message.OK);
            Assert.Equal(1, message.NumAffected);
            Nexus nexusInDB = await nexusService.Get(newNexus.DBName);
            Assert.Single(nexusInDB.Photos);

            // Remove 1 image
            message = await nexusService.RemoveImage(imageDBNames[0], newNexus.DBName);
            Assert.True(message.OK);
            Assert.Equal(1, message.NumAffected);
            nexusInDB = await nexusService.Get(newNexus.DBName);
            Assert.Empty(nexusInDB.Photos);

            // Add many images
            message = await nexusService.AddImage(imageDBNames, newNexus.DBName);
            Assert.True(message.OK);
            Assert.Equal(1, message.NumAffected); // <- should still be 1 because only 1 project is updated
            nexusInDB = await nexusService.Get(newNexus.DBName);
            Assert.Equal(photos.Count, nexusInDB.Photos.Count());

            // Remove many images
            message = await nexusService.RemoveImage(imageDBNames, newNexus.DBName);
            Assert.True(message.OK);
            Assert.Equal(1, message.NumAffected); // <- should still be 1 because only 1 project is updated
            nexusInDB = await nexusService.Get(newNexus.DBName);
            Assert.Empty(nexusInDB.Photos);

        }

    }

    #region Test Data Section
    public class DataSingleNexusWithImages : TheoryData<InputNexus, IList<InputPhoto>>
    {
        public DataSingleNexusWithImages()
        {

            var nexus = new InputNexus()
            {
                DBName = "nexus-central",
                Name = "Central Nexus",
                Description = "This is the central nexus",
                URL = null,
                Type = "type-testdata",
                Avatar = "testdata.jpg",
                Cover = null,
            };

            var photos = new List<InputPhoto>()
            {
                new InputPhoto()
                {
                    Name = "RandomPhoto",
                    Description = "Random Photo",
                    URL = "goodyolo.jpg",
                },
                new InputPhoto()
                {
                    Name = "RandomPhoto2",
                    Description = "Random Photo 2",
                    URL = "strawberry.png",
                },
                new InputPhoto()
                {
                    DBName = "photo-timely-romance",
                    Name = "Timely Romance",
                    Description = "Timely Romance - a named photo",
                    URL = "timely-romance.jpg",
                },
            };

            Add(
                nexus,
                photos
            );
        }
    }

    #endregion

}


