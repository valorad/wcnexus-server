using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using WCNexus.App.Database;
using WCNexus.App.Models;

namespace WCNexus.App.Services
{
    public class ProjectService: IProjectService
    {
        private readonly IProjectDBService projectDBService;
        private readonly INexusService nexusService;

        public ProjectService(
            IProjectDBService projectDBService,
            INexusService nexusService
        )
        {
            this.projectDBService = projectDBService;
            this.nexusService = nexusService;
        }

        public async Task<Project> Get(string dbname, IDBViewOption options = null)
        {
            Nexus nexus = await nexusService.Get(dbname, options);
            ProjectDB projectDB = await projectDBService.Get(dbname, options);

            if (nexus is {} && projectDB is {})
            {
                return new Project()
                {
                    DBName = nexus.DBName,
                    Name = nexus.Name,
                    Description = nexus.Description,
                    URL = nexus.URL,
                    Logo = nexus.Logo,
                    Type = nexus.Type,
                    Techs = await GetTechnologies(projectDB.Techs),
                    Images = null,
                };
            }

            return null;

        }

        public async Task<IEnumerable<Project>> Get(FilterDefinition<ProjectDB> projectCondition, IDBViewOption options = null)
        {

            IEnumerable<ProjectDB> projectDBInstances = await projectDBService.Get(projectCondition, options);

            if (projectDBInstances.Count() <= 0)
            {
                return Enumerable.Empty<Project>();
            }

            IEnumerable<string> projectDBNames = (
                from projectDB in projectDBInstances
                select projectDB.DBName
            );

            List<string> quotedNames = (
                from name in projectDBNames
                select $@"""{name}"""
            ).ToList();

            var projectNexusFilterLiteral = $@"{{
                ""dbname"": {{
                    ""$in"": [ { string.Join(',', quotedNames) } ]
                }}
            }}";

            // remove all white spaces
            projectNexusFilterLiteral = Regex.Replace(projectNexusFilterLiteral, @"\s+", "");
            
            FilterDefinition<Nexus> projectNexusFilter = JsonDocument.Parse(projectNexusFilterLiteral).RootElement.ToString();

            ProjectionOption nexusProjections = null;

            if (options is {})
            {
                nexusProjections = new ProjectionOption()
                {
                    Includes = options.Includes,
                    Excludes = options.Excludes,
                };
            }

            IEnumerable<Nexus> nexuses = await nexusService.Get(projectNexusFilter, nexusProjections as IDBViewOption);

            var projectNexusItems = projectDBInstances.Join(
                nexuses,
                projectDB => projectDB.DBName,
                nexus => nexus.DBName,
                (projectDB, nexus) => new
                {
                    DBName = nexus.DBName,
                    Name = nexus.Name,
                    Description = nexus.Description,
                    URL = nexus.URL,
                    Logo = nexus.Logo,
                    Type = nexus.Type,
                    Techs = projectDB.Techs,
                }
            );

            IEnumerable<Project> projects = await Task.WhenAll(projectNexusItems.Select(async (project) => new Project()
            {
                    DBName = project.DBName,
                    Name = project.Name,
                    Description = project.Description,
                    URL = project.URL,
                    Logo = project.Logo,
                    Type = project.Type,
                    Techs = await GetTechnologies(project.Techs),
            }));

            return projects;
            
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

            var newProjectDBInstance = new ProjectDB()
            {
                DBName = newProject.DBName,
                Techs = newProject.Techs,
                Images = newProject.Images,
            };

            CUDMessage nexusAddMessage = await nexusService.Add(newNexus);
            CUDMessage projectDBAddMessage = await projectDBService.Add(newProjectDBInstance);

            return new List<CUDMessage>()
            {
                nexusAddMessage,
                projectDBAddMessage
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

            IEnumerable<ProjectDB> newProjectDBInstances = (
                from project in newProjects
                select new ProjectDB()
                {
                    Techs = project.Techs,
                    Images = project.Images,
                }
            );

            CUDMessage nexusAddMessage = await nexusService.Add(newNexuses);
            CUDMessage projectDBAddMessage = await projectDBService.Add(newProjectDBInstances);

            return new List<CUDMessage>()
            {
                nexusAddMessage,
                projectDBAddMessage
            };

        }

        /// <summary>
        /// [This method is not implemented and should not be used!]
        /// </summary>
        public async Task<IEnumerable<CUDMessage>> Update(string dbname, UpdateDefinition<Project> token)
        {
            throw new NotImplementedException();
        }  

        /// <summary>
        /// [This method is not implemented and should not be used!]
        /// </summary>
        public async Task<IEnumerable<CUDMessage>> Update(FilterDefinition<Project> condition, UpdateDefinition<Project> token)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<CUDMessage>> Delete(string dbname)
        {
            CUDMessage nexusDeleteMessage = await nexusService.Delete(dbname);
            CUDMessage projectDBDeleteMessage = await projectDBService.Delete(dbname);

            return new List<CUDMessage>()
            {
                nexusDeleteMessage,
                projectDBDeleteMessage
            };

        }

        public async Task<IEnumerable<CUDMessage>> Delete(JsonElement deleteProjectDBNameFilter)
        {

            FilterDefinition<Nexus> deleteNexusFilter = deleteProjectDBNameFilter.ToString();
            FilterDefinition<ProjectDB> deleteProjectDBFilter = deleteProjectDBNameFilter.ToString();

            CUDMessage nexusDeleteMessage = await nexusService.Delete(deleteNexusFilter);
            CUDMessage projectDBDeleteMessage = await projectDBService.Delete(deleteProjectDBFilter as FilterDefinition<ProjectDB>);

            return new List<CUDMessage>()
            {
                nexusDeleteMessage,
                projectDBDeleteMessage
            };

        }

        public async Task<IEnumerable<CUDMessage>> FindManyAndDelete(FilterDefinition<ProjectDB> projectDBCondition)
        {
            IEnumerable<ProjectDB> projectDBInstances = await projectDBService.Get(
                projectDBCondition,
                new DBViewOption()
                {
                    Includes = new List<string>() { "dbname" }
                }
            );

            List<string> deleteProjectDBNames = (
                from project in projectDBInstances
                select project.DBName
            ).ToList();

            
            List<string> quotedNames = (
                from name in deleteProjectDBNames
                select $@"""{name}"""
            ).ToList();

            var deleteProjectFilterLiteral = $@"{{
                ""dbname"": {{
                    ""$in"": [ { string.Join(',', quotedNames) } ]
                }}
            }}";

            // remove all white spaces
            deleteProjectFilterLiteral = Regex.Replace(deleteProjectFilterLiteral, @"\s+", "");

            return await Delete(JsonDocument.Parse(deleteProjectFilterLiteral).RootElement);

        }

        public async Task<IEnumerable<CUDMessage>> FindManyAndDelete(FilterDefinition<Nexus> nexusCondition)
        {
            IEnumerable<Nexus> nexusInstances = await nexusService.Get(
                nexusCondition,
                new DBViewOption()
                {
                    Includes = new List<string>() { "dbname", "type" }
                }
            );

            IEnumerable<Nexus> deleteNexusInstances = (
                from nexus in nexusInstances
                where nexus.Type == "type-project"
                select nexus
            );

            List<string> deleteProjectDBNames = (
                from nexus in deleteNexusInstances
                select nexus.DBName
            ).ToList();

            List<string> quotedNames = (
                from name in deleteProjectDBNames
                select $@"""{name}"""
            ).ToList();

            var deleteProjectFilterLiteral = $@"{{
                ""dbname"": {{
                    ""$in"": [ { string.Join(',', quotedNames) } ]
                }}
            }}";
            
            return await Delete(JsonDocument.Parse(deleteProjectFilterLiteral).RootElement);
        }

        // get technology nexus
        public async Task<IEnumerable<Nexus>> GetTechnologies(IEnumerable<string> techNames)
        {

            List<string> quotedTechnologyNames = (
                from name in techNames
                select $@"""{name}"""
            ).ToList();

            var technologyFilterLiteral = $@"{{
                ""type"": ""type-technology"",
                ""dbname"": {{
                    ""$in"": [ { string.Join(',', quotedTechnologyNames) } ]
                }}
            }}";

            // remove all white spaces
            technologyFilterLiteral = Regex.Replace(technologyFilterLiteral, @"\s+", "");

            FilterDefinition<Nexus> technologyFilter = JsonDocument.Parse(technologyFilterLiteral).RootElement.ToString();

            return await nexusService.Get(technologyFilter);

        }

        // get project images

    }
}