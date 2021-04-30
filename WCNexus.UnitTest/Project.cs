using System;
using Microsoft.Extensions.DependencyInjection;
using WCNexus.App.Database;
using WCNexus.App.Services;
using WCNexus.App.Models;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using WCNexus.App.Library;
using MongoDB.Bson;

namespace WCNexus.UnitTest
{
    [Collection("Sequential")]
    public class ProjectTest : IClassFixture<ServiceFixture>, IDisposable
    {
        private readonly ServiceProvider serviceProvider;
        private readonly IDBContext dbContext;
        private readonly IProjectService projectService;
        private readonly INexusService nexusService;
        private readonly IStoredProjectService storedProjectService;

        public ProjectTest(ServiceFixture fixture)
        {
            serviceProvider = fixture.ServiceProvider;

            dbContext = serviceProvider.GetService<IDBContext>();
            projectService = serviceProvider.GetService<IProjectService>();
            nexusService = serviceProvider.GetService<INexusService>();
            storedProjectService = serviceProvider.GetService<IStoredProjectService>();
        }

        public void Dispose()
        {
            // called after each test method
            dbContext.Drop();
        }

        [Theory(DisplayName = "Project singular test")]
        [ClassData(typeof(DataSingleProject))]
        public async void SingleCRUDTest(InputProject newProject, IList<InputNexus> technologies)
        {

            // Add existing technologies
            await nexusService.Add(technologies);

            // Add single
            List<CUDMessage> addMessages = (await projectService.Add(newProject)).ToList();

            foreach (var message in addMessages)
            {
                Assert.True(message.OK);
                Assert.Equal(1, message.NumAffected);
            }

            // Read single
            JointProject projectInDB = await projectService.Get(newProject.DBName);
            Assert.NotNull(projectInDB);
            Assert.Equal(newProject.DBName, projectInDB.DBName);
            Assert.NotNull(projectInDB.Techs);

            // Update single - change name and add a new technology
            var toAddNewTechnology = "tech-mssql";
            var newName = "Project Endian Initializer";
            UpdateDefinition<JointProject> projectUpdateToken = JsonUtil.CreateCompactLiteral(
                $@"{{
                    ""$set"": {{
                        ""name"": ""{newName}""
                    }},
                    ""$push"": {{
                        ""techs"": ""{toAddNewTechnology}""
                    }}
                 }}"
            );
            List<CUDMessage> updateMessages = (await projectService.Update(newProject.DBName, projectUpdateToken)).ToList();
            foreach (var message in updateMessages)
            {
                Assert.True(message.OK);
                Assert.Equal(1, message.NumAffected);
            }

            projectInDB = await projectService.Get(newProject.DBName);

            Assert.Equal(newName, projectInDB.Name);

            string techInProject = projectInDB.Techs.FirstOrDefault(tech => tech == toAddNewTechnology);

            Assert.NotNull(techInProject);

            // Delete single
            List<CUDMessage> deleteMessages = (await projectService.Delete(newProject.DBName)).ToList();

            foreach (var message in deleteMessages)
            {
                Assert.True(message.OK);
                Assert.Equal(1, message.NumAffected);
            }

            projectInDB = await projectService.Get(newProject.DBName);
            Assert.Null(projectInDB);
        }

        [Theory(DisplayName = "Project plural test")]
        [ClassData(typeof(DataMultipleProjects))]
        public async void PluralCRUDTest(IList<InputProject> newProjects, IList<InputNexus> technologies)
        {

            // Add existing technologies
            await nexusService.Add(technologies);

            // Add many + Get many
            List<CUDMessage> addMessages = (await projectService.Add(newProjects)).ToList();

            foreach (var message in addMessages)
            {
                Assert.True(message.OK);
            }

            FilterDefinition<JointProject> getConditionAll = "{}";

            List<JointProject> projectsInDB = (await projectService.Get(getConditionAll)).ToList();
            Assert.Equal(2, projectsInDB.Count);

            FilterDefinition<JointProject> getConditionHasLogoWithTechs = JsonUtil.CreateCompactLiteral($@"{{
                 ""logo"":{{
                     ""$ne"": null
                 }},
                 ""$expr"": {{
                     ""$eq"": [
                         {{""$size"": ""$techs""}}, 2 
                     ]
                 }}
             }}");
            projectsInDB = (await projectService.Get(getConditionHasLogoWithTechs)).ToList();
            Assert.Single(projectsInDB);

            // ===== Update Many =====
            // -> Find the project with no URL and with 2 technologies
            FilterDefinition<JointProject> updateCondition = JsonUtil.CreateCompactLiteral($@"{{
                ""url"": null,
                ""$expr"": {{""$eq"": [ {{""$size"": ""$techs""}}, 2 ]}}
            }}");

            var newURL = "https://404.org";
            var toAddNewTechnologyName = "tech-mssql";

            UpdateDefinition<JointProject> updateToken = JsonUtil.CreateCompactLiteral($@"{{
                    ""$set"": {{
                        ""url"": ""{newURL}""
                    }},
                    ""$push"": {{
                        ""techs"": ""{ toAddNewTechnologyName }""
                    }}
                }}
            ");

            List<CUDMessage> projectUpdateMessages = (await projectService.Update(updateCondition, updateToken)).ToList();

            foreach (var message in projectUpdateMessages)
            {
                Assert.True(message.OK);
                Assert.Equal(1, message.NumAffected);
            }

            FilterDefinition<JointProject> afterUpdateGetCondition = JsonUtil.CreateCompactLiteral($@"{{
                ""url"": ""{ newURL }"",
                ""$expr"": {{""$eq"": [ {{""$size"": ""$techs""}}, 3 ]}}
            }}");

            projectsInDB = (await projectService.Get(afterUpdateGetCondition)).ToList();
            Assert.Single(projectsInDB);
            Assert.Equal(newURL, projectsInDB[0].URL);
            Assert.NotNull(projectsInDB[0].Techs.First(tech => tech == toAddNewTechnologyName));

            // ===== Delete Many =====
            FilterDefinition<JointProject> projectDeleteCondition = JsonUtil.CreateCompactLiteral($@"{{
                 ""$expr"": {{""$eq"": [ {{""$size"": ""$techs""}}, 1 ]}}
             }}");

            List<CUDMessage> projectDeleteMessages = (await projectService.Delete(projectDeleteCondition)).ToList();

            foreach (var message in projectDeleteMessages)
            {
                Assert.True(message.OK);
                Assert.Equal(1, message.NumAffected);
            }

            List<JointProject> projectsToDeleteInDB = (await projectService.Get(projectDeleteCondition)).ToList();
            Assert.Empty(projectsToDeleteInDB);

        }
        
        [Theory(DisplayName = "Project add images test")]
        [ClassData(typeof(DataSingleProject))]
        public async void ImagesTest(InputProject newProject, IList<InputNexus> technologies, IList<InputPhoto> photos)
        {
            // Add existing technologies
            await nexusService.Add(technologies);

            // Add single
            await projectService.Add(newProject);

            // Extract image dbnames
            List<string> imageDBNames = (
                from photo in photos
                select photo.DBName
            ).ToList();

            // Add 1 image
            CUDMessage message = await projectService.AddImage(imageDBNames[0], newProject.DBName);
            Assert.True(message.OK);
            Assert.Equal(1, message.NumAffected);
            JointProject projectInDB = await projectService.Get(newProject.DBName);
            Assert.Single(projectInDB.Images);

            // Remove 1 image
            message = await projectService.RemoveImage(imageDBNames[0], newProject.DBName);
            Assert.True(message.OK);
            Assert.Equal(1, message.NumAffected);
            projectInDB = await projectService.Get(newProject.DBName);
            Assert.Empty(projectInDB.Images);

            // Add many images
            message = await projectService.AddImage(imageDBNames, newProject.DBName);
            Assert.True(message.OK);
            Assert.Equal(1, message.NumAffected); // <- should still be 1 because only 1 project is updated
            projectInDB = await projectService.Get(newProject.DBName);
            Assert.Equal(photos.Count, projectInDB.Images.Count());

            // Remove many images
            message = await projectService.RemoveImage(imageDBNames, newProject.DBName);
            Assert.True(message.OK);
            Assert.Equal(1, message.NumAffected); // <- should still be 1 because only 1 project is updated
            projectInDB = await projectService.Get(newProject.DBName);
            Assert.Empty(projectInDB.Images);

        }

    }

    #region Test Data Section

    public class DataSingleProject : TheoryData<InputProject, IList<InputNexus>, IList<InputPhoto>>
    {
        public DataSingleProject()
        {

            var newProject = new InputProject()
            {
                DBName = "project-endian",
                Name = "Endian",
                Description = "Project Endian",
                URL = null,
                Logo = null,
                Type = "type-project",
                Techs = new List<string>() {
                    "tech-csharp",
                    "tech-angular",
                }
            };

            var technologies = new List<InputNexus>()
            {
                new InputNexus()
                {
                    DBName = "tech-csharp",
                    Name = "C# Language",
                    Description = "One of the language spoken by .Net",
                    URL = "http://asp.net",
                    Logo = "C-well.jpg",
                    Type = "type-technology",
                },
                new InputNexus()
                {
                    DBName = "tech-angular",
                    Name = "Angular",
                    Description = "The modern web developer's platform",
                    URL = "https://angular.io/",
                    Logo = "angular.jpg",
                    Type = "type-technology",
                },
                new InputNexus()
                {
                    DBName = "tech-mssql",
                    Name = "My-cro-soft-super-quater-latency",
                    Description = "So, in short, MYSQL",
                    URL = "https://idunnowhat.tm/",
                    Logo = "idunnowhat.jpg",
                    Type = "type-technology",
                },
            };

            var photos = new List<InputPhoto>()
            {
                new InputPhoto()
                {
                    DBName = "photo-6d94a061-7262-4461-84d3-2dee7694d2cf",
                    Name = "RandomPhoto",
                    Description = "Random Photo",
                    URL = "goodyolo.jpg",
                },
                new InputPhoto()
                {
                    DBName = "photo-a1a50046-7647-4c31-8e0f-54aff5d9f07c",
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

            Add(newProject, technologies, photos);

        }
    }

    public class DataMultipleProjects : TheoryData<IList<InputProject>, List<InputNexus>>
    {

        public DataMultipleProjects()
        {
            var newProjects = new List<InputProject>()
            {
                new InputProject()
                {
                    DBName = "project-rgb-syncer",
                    Name = "RGM Synchronizer",
                    Description = "RGM Synchronizer",
                    URL = null,
                    Logo = null,
                    Type = "type-project",
                    Techs = new List<string>() {
                        "tech-java"
                    },
                },

                new InputProject()
                {
                    DBName = "project-fps-helper",
                    Name = "FPS Helper",
                    Description = "FPS Helper",
                    URL = null,
                    Logo = "fps-helper.png",
                    Type = "type-project",
                    Techs = new List<string>() {
                        "tech-angular",
                        "tech-javascript",
                    },
                },
            };

            var technologies = new List<InputNexus>()
            {
                new InputNexus()
                {
                    DBName = "tech-java",
                    Name = "Java Language",
                    Description = "Language spoken by Spring eco-system",
                    URL = null,
                    Logo = "java-island.webp",
                    Type = "type-technology",
                },
                new InputNexus()
                {
                    DBName = "tech-angular",
                    Name = "Angular",
                    Description = "The modern web developer's platform",
                    URL = "https://angular.io/",
                    Logo = "angular.jpg",
                    Type = "type-technology",
                },
                new InputNexus()
                {
                    DBName = "tech-javascript",
                    Name = "JavaScript Language",
                    Description = "Language spoken by all browsers + node eco-system",
                    URL = null,
                    Logo = "java-scripter.gif",
                    Type = "type-technology",
                },
                new InputNexus()
                {
                    DBName = "tech-mssql",
                    Name = "My-cro-soft-super-quater-latency",
                    Description = "So, in short, MYSQL",
                    URL = "https://idunnowhat.tm/",
                    Logo = "idunnowhat.jpg",
                    Type = "type-technology",
                },
            };

            Add(newProjects, technologies);

        }

    }

    #endregion



}