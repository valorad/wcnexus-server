using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using WCNexus.App.Database;
using WCNexus.App.Models;
using WCNexus.App.Services;
using Xunit;

namespace WCNexus.UnitTest
{
    [Collection("Sequential")]
    public class DataAccessTest : IClassFixture<ServiceFixture>, IDisposable
    {

        private readonly ServiceProvider serviceProvider;
        private readonly IDBContext dbContext;
        private readonly INexusService nexusService;

        public DataAccessTest(ServiceFixture fixture)
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

        [Theory(DisplayName = "Data Access singular test")]
        [ClassData(typeof(DataSingleNexus))]
        public async void TestCRUDSingle(Nexus newNexus)
        {

            var newLogo = "central-station.png";
            var updateTokenLiteral = $@"{{
                ""$set"": {{
                    ""logo"": ""{newLogo}""
                }}
            }}";
            // remove all white spaces
            updateTokenLiteral = Regex.Replace(updateTokenLiteral, @"\s+", "");
            // validate JSON structure
            UpdateDefinition<Nexus> updateToken = JsonDocument.Parse(updateTokenLiteral).RootElement.ToString();

            // Add single
            CUDMessage addMessage = await nexusService.Add(newNexus);
            Assert.True(addMessage.OK);
            Nexus nexusInDB = await nexusService.Get(newNexus.DBName);
            Assert.NotNull(nexusInDB);
            // update single
            CUDMessage updateMessage = await nexusService.Update(newNexus.DBName, updateToken);
            nexusInDB = await nexusService.Get(newNexus.DBName);
            Assert.Equal(newLogo, nexusInDB.Logo);
            // delete single
            CUDMessage deleteMessage = await nexusService.Delete(newNexus.DBName);
            Assert.True(deleteMessage.OK);
            nexusInDB = await nexusService.Get(newNexus.DBName);
            Assert.Null(nexusInDB);

        }

        [Theory(DisplayName = "Data Access plural test")]
        [ClassData(typeof(DataMultipleNexuses))]
        public async void TestCRUDList(List<Nexus> newNexuses)
        {
            var newURL = "https://404.org";

            var updateConditionLiteral = $@"{{
                ""url"": null
            }}";
            // remove all white spaces
            updateConditionLiteral = Regex.Replace(updateConditionLiteral, @"\s+", "");
            // validate JSON structure
            FilterDefinition<Nexus> updateCondition = JsonDocument.Parse(updateConditionLiteral).RootElement.ToString();

            var updateTokenLiteral = $@"{{
                    ""$set"": {{
                        ""url"": ""{newURL}""
                    }}
                }}
            ";
            // remove all white spaces
            updateTokenLiteral = Regex.Replace(updateTokenLiteral, @"\s+", "");
            // validate JSON structure
            UpdateDefinition<Nexus> updateToken = JsonDocument.Parse(updateTokenLiteral).RootElement.ToString();

            FilterDefinition<Nexus> getCondition = JsonDocument.Parse("{}").RootElement.ToString();
            FilterDefinition<Nexus> afterUpdateGetCondition = JsonDocument.Parse($@"{{""url"": ""{newURL}"" }}").RootElement.ToString();

            // Add many
            CUDMessage addMessage = await nexusService.Add(newNexuses);
            Assert.True(addMessage.OK);
            List<Nexus> nexusesInDB = (await nexusService.Get(getCondition)).ToList();
            Assert.Equal(3, nexusesInDB.Count);
            // update many
            CUDMessage updateMessage = await nexusService.Update(updateCondition, updateToken);
            Assert.Equal(2, updateMessage.NumAffected);
            nexusesInDB = (await nexusService.Get(afterUpdateGetCondition)).ToList();
            Assert.Equal(2, nexusesInDB.Count);
            foreach (var nexus in nexusesInDB) {
                Assert.Equal(newURL, nexus.URL);
            }
            // delete many
            CUDMessage deleteMessage = await nexusService.Delete(afterUpdateGetCondition);
            Assert.Equal(2, deleteMessage.NumAffected);
            nexusesInDB = (await nexusService.Get(getCondition)).ToList();
            Assert.Single(nexusesInDB);
        }

    }

    #region Test Data Section

    public class DataSingleNexus : TheoryData<Nexus>
    {
        public DataSingleNexus()
        {

            Add(
                new Nexus()
                {
                    DBName = "nexus-central",
                    Name = "Central Nexus",
                    Description = "This is the central nexus",
                    URL = null,
                    Logo = null,
                    Type = "type-testdata",
                }
            );
        }
    }

    public class DataMultipleNexuses : TheoryData<List<Nexus>>
    {
        public DataMultipleNexuses()
        {
            Add(
              new List<Nexus>() {
                new Nexus() {
                    DBName = "nexus-csharp",
                    Name = "C# Language",
                    Description = "One of the language spoken by .Net",
                    URL = "http://asp.net",
                    Logo = "C-well.jpg",
                    Type = "type-technology",
                },
                new Nexus() {
                    DBName = "nexus-java",
                    Name = "Java Language",
                    Description = "Language spoken by Spring eco-system",
                    URL = null,
                    Logo = "java-island.webp",
                    Type = "type-technology",
                },
                new Nexus() {
                    DBName = "nexus-javascript",
                    Name = "JavaScript Language",
                    Description = "Language spoken by all browsers + node eco-system",
                    URL = null,
                    Logo = "java-scripter.gif",
                    Type = "type-technology",
                },
              }
            );
        }
    }

    #endregion




}