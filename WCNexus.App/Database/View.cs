using System.Collections.Generic;
using WCNexus.App.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using WCNexus.App.Library;

namespace WCNexus.App.Database
{

    public static class View
    {
        public static IFindFluent<T, T> MakePagination<T>(IFindFluent<T, T> query, IPaginationOption options)
        {
            var pageSize = (options.PerPage > 0 ? options.PerPage : 10);
            var currentPage = (options.Page > 0 ? options.Page : 1);
            return query.Limit(pageSize).Skip((currentPage - 1) * pageSize);
        }

        public static string BuildProjection(IProjectionOption options)
        {
            var projectionToken = new Dictionary<string, int>() { };

            if (options.Includes is { })
            {
                foreach (var field in options.Includes)
                {
                    projectionToken.Add(field, 1);
                }
            }

            if (options.Excludes is { })
            {
                foreach (var field in options.Excludes)
                {
                    projectionToken.Add(field, 0);
                }
            }

            return projectionToken.ToJson();

        }

        public static string BuildSort(ISortOption options)
        {
            if (options.OrderBy != null)
            {
                return $"{{ {options.OrderBy}: { (options.Order == "desc" ? -1 : 1) } }}";
            }
            return "{}";
        }

        public static void AddPaginationToPipeline(IList<BsonDocument> pipelineStages, IPaginationOption options)
        {
            int pageSize = (options.PerPage > 0 ? options.PerPage : 10);
            int currentPage = (options.Page > 0 ? options.Page : 1);

            pipelineStages.Add(BsonDocument.Parse(JsonUtil.CreateCompactLiteral($@"{{
                ""$limit"": {pageSize}
            }}")));

            pipelineStages.Add(BsonDocument.Parse(JsonUtil.CreateCompactLiteral($@"{{
                ""$skip"": {(currentPage - 1) * pageSize}
            }}")));

        }

        public static void AddProjectionToPipeline(IList<BsonDocument> pipelineStages, IProjectionOption options)
        {
            pipelineStages.Add(BsonDocument.Parse(JsonUtil.CreateCompactLiteral($@"{{
                ""$project"": {BuildProjection(options)}
            }}")));
        }

        public static void AddSortToPipeline(IList<BsonDocument> pipelineStages, ISortOption options)
        {
            pipelineStages.Add(BsonDocument.Parse(JsonUtil.CreateCompactLiteral($@"{{
                ""$sort"": {BuildSort(options)}
            }}")));
        }

    }
}