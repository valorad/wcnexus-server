using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using WCNexus.App.Library;
using WCNexus.App.Models;

namespace WCNexus.App.Services
{
    public class ProjectService: IProjectService
    {
        private readonly IStoredProjectService storedProjectService;
        private readonly INexusService nexusService;
        private List<string> storedProjectInputFields = new List<string>() {"techs", "images"};
        private List<string> nexusInputFields = new List<string>() {"dbname", "name", "description", "url", "logo"}; // does not allow updating "type" field

        public ProjectService(
            IStoredProjectService storedProjectService,
            INexusService nexusService
        )
        {
            this.storedProjectService = storedProjectService;
            this.nexusService = nexusService;
        }

        public async Task<JointProject> Get(string dbname, IDBViewOption options = null)
        {
            return await storedProjectService.LeftJoinAndGet<JointProject>(
                dbname,
                new DBLeftJoinOption()
                {
                    collectionName = nexusService.collectionNamespace.CollectionName,
                    localField = "dbname",
                    foreignField = "dbname",
                },
                options
            );

        }

        public async Task<IEnumerable<JointProject>> Get(FilterDefinition<JointProject> projectCondition, IDBViewOption options = null)
        {
            return await storedProjectService.LeftJoinAndGet(
                projectCondition,
                new DBLeftJoinOption()
                {
                    collectionName = nexusService.collectionNamespace.CollectionName,
                    localField = "dbname",
                    foreignField = "dbname",
                },
                options
            );
            
        }
        public async Task<IEnumerable<CUDMessage>> Add(InputProject newProject)
        {
            var newNexus = new Nexus()
            {
                DBName = newProject.DBName,
                Name = newProject.Name,
                Description = newProject.Description,
                URL = newProject.URL,
                Logo = newProject.Logo,
                Type = newProject.Type,
            };

            var newStoredProjectInstance = new StoredProject()
            {
                DBName = newProject.DBName,
                Techs = newProject.Techs,
                Images = newProject.Images,
            };

            CUDMessage nexusAddMessage = await nexusService.Add(newNexus);
            CUDMessage StoredProjectAddMessage = await storedProjectService.Add(newStoredProjectInstance);

            return new List<CUDMessage>()
            {
                nexusAddMessage,
                StoredProjectAddMessage
            };

        }

        public async Task<IEnumerable<CUDMessage>> Add(IEnumerable<InputProject> newProjects)
        {
            IEnumerable<Nexus> newNexuses = (
                from project in newProjects
                select new Nexus()
                {
                    DBName = project.DBName,
                    Name = project.Name,
                    Description = project.Description,
                    URL = project.URL,
                    Logo = project.Logo,
                    Type = project.Type,
                }
            );

            IEnumerable<StoredProject> newStoredProjectInstances = (
                from project in newProjects
                select new StoredProject()
                {
                    DBName = project.DBName,
                    Techs = project.Techs,
                    Images = project.Images,
                }
            );

            CUDMessage nexusAddMessage = await nexusService.Add(newNexuses);
            CUDMessage StoredProjectAddMessage = await storedProjectService.Add(newStoredProjectInstances);

            return new List<CUDMessage>()
            {
                nexusAddMessage,
                StoredProjectAddMessage
            };

        }

        public async Task<IEnumerable<CUDMessage>> Update(string dbname, UpdateDefinition<JointProject> token)
        {
            // extract update token for nexus and storedproject, by field existing in the nested doc
            var tokenDoc = token.RenderToBsonDocument();
            var storedProjectUpdateToken = ExtractUpdateToken<StoredProject>(tokenDoc, storedProjectInputFields);
            var nexusUpdateToken = ExtractUpdateToken<Nexus>(tokenDoc, nexusInputFields);

            // perform update on collections by that dbname and respective token
            var storedProjectUpdateMessage = new CUDMessage()
            {
                OK = true,
                NumAffected = 0,
            };
            var nexusUpdateMessage = new CUDMessage()
            {
                OK = true,
                NumAffected = 0,
            };

            if (storedProjectUpdateToken is { })
            {
                storedProjectUpdateMessage = await storedProjectService.Update(dbname, storedProjectUpdateToken);
            }

            if (nexusUpdateToken is { })
            {
                nexusUpdateMessage = await nexusService.Update(dbname, nexusUpdateToken);
            }

            return new List<CUDMessage>()
            {
                storedProjectUpdateMessage,
                nexusUpdateMessage
            };

        }  

        public async Task<IEnumerable<CUDMessage>> Update(FilterDefinition<JointProject> condition, UpdateDefinition<JointProject> token)
        {
            // join 2 colllections and find the results
            var projects = await Get(
                condition,
                new DBViewOption()
                {
                    Includes = new List<string>() {"dbname"},
                }
            );

            // extract dbnames
            List<string> dbnames = (
                from project in projects
                select project.DBName
            ).ToList();

            List<string> quotedNames = (
                from name in dbnames
                select $@"""{name}"""
            ).ToList();

            // extract update token for nexus and storedproject, by field existing in the nested doc
            var tokenDoc = token.RenderToBsonDocument();
            var storedProjectUpdateToken = ExtractUpdateToken<StoredProject>(tokenDoc, storedProjectInputFields);
            var nexusUpdateToken = ExtractUpdateToken<Nexus>(tokenDoc, nexusInputFields);

            // perform update on collections by those dbnames and respective token
            string updateCondition = JsonUtil.CreateCompactLiteral($@"{{
                ""dbname"": {{
                    ""$in"": [ { string.Join(',', quotedNames) } ]
                }}
            }}");

            var storedProjectUpdateMessage = new CUDMessage()
            {
                OK = true,
                NumAffected = 0,
            };
            var nexusUpdateMessage = new CUDMessage()
            {
                OK = true,
                NumAffected = 0,
            };

            if (storedProjectUpdateToken is { })
            {
                storedProjectUpdateMessage = await storedProjectService.Update((FilterDefinition<StoredProject>) updateCondition, storedProjectUpdateToken);
            }

            if (nexusUpdateToken is { })
            {
                nexusUpdateMessage = await nexusService.Update((FilterDefinition<Nexus>) updateCondition , nexusUpdateToken);
            }

            return new List<CUDMessage>()
            {
                storedProjectUpdateMessage,
                nexusUpdateMessage
            };

        }

        public async Task<IEnumerable<CUDMessage>> Delete(string dbname)
        {
            CUDMessage nexusDeleteMessage = await nexusService.Delete(dbname);
            CUDMessage StoredProjectDeleteMessage = await storedProjectService.Delete(dbname);

            return new List<CUDMessage>()
            {
                nexusDeleteMessage,
                StoredProjectDeleteMessage
            };

        }

        public async Task<IEnumerable<CUDMessage>> Delete(FilterDefinition<JointProject> projectCondition)
        {
            IEnumerable<JointProject> projects = await storedProjectService.LeftJoinAndGet(
                projectCondition,
                new DBLeftJoinOption()
                {
                    collectionName = nexusService.collectionNamespace.CollectionName,
                    localField = "dbname",
                    foreignField = "dbname",
                },
                new DBViewOption()
                {
                    Includes = new List<string>() {"dbname"}
                }
            );

            List<string> deleteStoredProjectNames = (
                from project in projects
                select project.DBName
            ).ToList();

            List<string> quotedNames = (
                from name in deleteStoredProjectNames
                select $@"""{name}"""
            ).ToList();

            string deleteToken = JsonUtil.CreateCompactLiteral($@"{{
                ""dbname"": {{
                    ""$in"": [ { string.Join(',', quotedNames) } ]
                }}
            }}");

            CUDMessage nexusAddMessage = await nexusService.Delete((FilterDefinition<Nexus>)deleteToken);
            CUDMessage StoredProjectAddMessage = await storedProjectService.Delete((FilterDefinition<StoredProject>)deleteToken);

            return new List<CUDMessage>()
            {
                nexusAddMessage,
                StoredProjectAddMessage
            };

        }

        // get technology nexus
        private async Task<IEnumerable<Nexus>> GetTechnologies(IEnumerable<string> techNames)
        {

            List<string> quotedTechnologyNames = (
                from name in techNames
                select $@"""{name}"""
            ).ToList();

            FilterDefinition<Nexus> technologyFilter = JsonUtil.CreateCompactLiteral($@"{{
                ""type"": ""type-technology"",
                ""dbname"": {{
                    ""$in"": [ { string.Join(',', quotedTechnologyNames) } ]
                }}
            }}");

            return await nexusService.Get(technologyFilter);

        }

        // get project images
        private async Task GetImages(IEnumerable<string> techNames)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Extract Update Token for a collection from a big Update Token that sets across multiple collections
        /// </summary>
        /// <typeparam name="T">The target collection type</typeparam>
        /// <param name="updateTokenDocument">A Bson Document that should look like: {<$updateOperator1>: { <field1>: xxx, <field2>: xxx }, <$updateOperator2>: { <field1>: xxx, <field2>: xxx }}</param>
        /// <param name="fields">Fields in a collection to filter out</param>
        /// <returns></returns>
        private UpdateDefinition<T> ExtractUpdateToken<T>(BsonDocument updateTokenDocument, IEnumerable<string> fields)
        {
            var originalTokenMap = updateTokenDocument.ToDictionary(
                kv => kv.Name,
                (kv) => {
                    return kv.Value.ToBsonDocument().Elements.ToDictionary(el => el.Name, el => el.Value);
                }
            );

            var updateTokenMap = new Dictionary<string, Dictionary<string, BsonValue>>();

            foreach (var operatorKV in originalTokenMap)
            {
                var targetOperatorDict = updateTokenMap.GetValueOrDefault(operatorKV.Key);
                foreach (var fieldKV in operatorKV.Value)
                {
                    if (fields.Contains(fieldKV.Key))
                    {
                        if (targetOperatorDict is null)
                        {
                            targetOperatorDict = new Dictionary<string, BsonValue>();
                            updateTokenMap[operatorKV.Key] = targetOperatorDict;
                        }
                        targetOperatorDict.Add(fieldKV.Key, fieldKV.Value);
                    }
                }
            }

            if (updateTokenMap.Count > 0)
            {
                return updateTokenMap.ToJson();
            }

            return null;

        }

    }
}