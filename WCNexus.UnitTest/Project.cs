using System;
using Microsoft.Extensions.DependencyInjection;
using WCNexus.App.Database;
using WCNexus.App.Services;
using WCNexus.App.Models;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.Json;
using MongoDB.Driver;

namespace WCNexus.UnitTest
{
    [Collection("Sequential")]
    public class ProjectTest : IClassFixture<ServiceFixture>, IDisposable
    {
        private readonly ServiceProvider serviceProvider;
        private readonly IDBContext dbContext;
        private readonly IProjectService projectService;
        private readonly INexusService nexusService;
        private readonly IProjectDBService projectDBService;

        public ProjectTest(ServiceFixture fixture)
        {
            serviceProvider = fixture.ServiceProvider;

            dbContext = serviceProvider.GetService<IDBContext>();
            projectService = serviceProvider.GetService<IProjectService>();
            nexusService = serviceProvider.GetService<INexusService>();
            projectDBService = serviceProvider.GetService<IProjectDBService>();
        }

        public void Dispose()
        {
            // called after each test method
            dbContext.Drop();
        }

        [Theory(DisplayName = "Project singular test")]
        [ClassData(typeof(DataSingleProject))]
        public async void SingleCRUDTest(InputProject newProject, IEnumerable<InputNexus> technologies)
        {
            var toAddNewTechnology = "tech-mssql";
            var updateTokenLiteral = $@"{{
                ""$push"": {{
                    ""techs"": ""{toAddNewTechnology}""
                }}
            }}";
            // remove all white spaces
            updateTokenLiteral = Regex.Replace(updateTokenLiteral, @"\s+", "");
            // validate JSON structure
            UpdateDefinition<ProjectDB> projectDBUpdateToken = JsonDocument.Parse(updateTokenLiteral).RootElement.ToString();

            // Add existing technologies
            await nexusService.Add(technologies);

            // Add single
            List<CUDMessage> addMessages = (await projectService.Add(newProject)).ToList();

            foreach (var message in addMessages)
            {
                Assert.True(message.OK);
            }

            // Read single
            Project projectInDB = await projectService.Get(newProject.DBName);
            Assert.NotNull(projectInDB);
            Assert.NotNull(projectInDB.DBName);
            Assert.NotNull(projectInDB.Techs);

            // Update single - add a new technology
            CUDMessage updateMessage = await projectDBService.Update(newProject.DBName, projectDBUpdateToken);
            projectInDB = await projectService.Get(newProject.DBName);

            Nexus techInProject = projectInDB.Techs.FirstOrDefault(tech => tech.DBName == toAddNewTechnology);

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
        public async void PluralCRUDTest(IEnumerable<InputProject> newProjects, IEnumerable<Nexus> technologies)
        {
            var newURL = "https://404.org";

            var updateConditionLiteral = $@"{{
                ""url"": null
            }}";
            // remove all white spaces
            updateConditionLiteral = Regex.Replace(updateConditionLiteral, @"\s+", "");
            // validate JSON structure
            FilterDefinition<ProjectDB> updateCondition = JsonDocument.Parse(updateConditionLiteral).RootElement.ToString();

            var updateTokenLiteral = $@"{{
                    ""$set"": {{
                        ""url"": ""{newURL}""
                    }}
                }}
            ";
            // remove all white spaces
            updateTokenLiteral = Regex.Replace(updateTokenLiteral, @"\s+", "");
            // validate JSON structure
            UpdateDefinition<ProjectDB> updateToken = JsonDocument.Parse(updateTokenLiteral).RootElement.ToString();

            FilterDefinition<ProjectDB> getCondition = JsonDocument.Parse("{}").RootElement.ToString();
            JsonElement afterUpdateGetJsonCondition = JsonDocument.Parse($@"{{""url"": ""{newURL}"" }}").RootElement;
            FilterDefinition<ProjectDB> afterUpdateGetCondition = afterUpdateGetJsonCondition.ToString();

            // Add many + Get many
            List<CUDMessage> addMessages = (await projectService.Add(newProjects)).ToList();

            foreach (var message in addMessages)
            {
                Assert.True(message.OK);
            }

            List<Project> projectsInDB = (await projectService.Get(getCondition)).ToList();
            Assert.Equal(2, projectsInDB.Count);

            // Update many
            CUDMessage updateMessage = await projectDBService.Update(updateCondition, updateToken);
            Assert.Equal(2, updateMessage.NumAffected);
            projectsInDB = (await projectService.Get(afterUpdateGetCondition)).ToList();
            Assert.Equal(2, projectsInDB.Count);
            
            foreach (var project in projectsInDB)
            {
                Assert.Equal(newURL, project.URL);
            }

            // Delete many
            List<CUDMessage> deleteMessages = (await projectService.Delete(afterUpdateGetJsonCondition)).ToList();

            foreach (var message in deleteMessages)
            {
                Assert.True(message.OK);
                Assert.Equal(2, message.NumAffected);
            }

            projectsInDB = (await projectService.Get(getCondition)).ToList();
            Assert.Equal(0, projectsInDB.Count);
        }

    }

    #region Test Data Section

    public class DataSingleProject : TheoryData<InputProject, IEnumerable<InputNexus>>
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
                // new InputNexus()
                // {
                //     DBName = "tech-idunnowhat",
                //     Name = "I Don't know what",
                //     Description = "A dummy tech trying to scramble the test",
                //     URL = "https://idunnowhat.tm/",
                //     Logo = "idunnowhat.jpg",
                //     Type = "type-technology",
                // },
            };

            Add(newProject, technologies);

        }
    }

    public class DataMultipleProjects : TheoryData<IEnumerable<InputProject>, IEnumerable<Nexus>>
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
                    Logo = null,
                    Type = "type-project",
                    Techs = new List<string>() {
                        "tech-angular",
                        "tech-javascript",
                    },
                },
            };

            var technologies = new List<Nexus>()
            {
                new Nexus()
                {
                    DBName = "tech-java",
                    Name = "Java Language",
                    Description = "Language spoken by Spring eco-system",
                    URL = null,
                    Logo = "java-island.webp",
                    Type = "type-technology",
                },
                new Nexus()
                {
                    DBName = "tech-angular",
                    Name = "Angular",
                    Description = "The modern web developer's platform",
                    URL = "https://angular.io/",
                    Logo = "angular.jpg",
                    Type = "type-technology",
                },
                new Nexus()
                {
                    DBName = "tech-javascript",
                    Name = "JavaScript Language",
                    Description = "Language spoken by all browsers + node eco-system",
                    URL = null,
                    Logo = "java-scripter.gif",
                    Type = "type-technology",
                },
            };

            Add(newProjects, technologies);

        }

    }

    #endregion



}