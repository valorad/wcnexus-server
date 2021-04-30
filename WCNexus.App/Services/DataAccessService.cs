using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WCNexus.App.Database;
using WCNexus.App.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq;
using WCNexus.App.Library;

namespace WCNexus.App.Services
{
    public class DataAccessService<T> : IDataAccessService<T>
    {
        private readonly IMongoCollection<T> collection;
        public string indexFieldName { get; }
        public CollectionNamespace collectionNamespace { get; }
        public DataAccessService(
            IMongoCollection<T> collection,
            string indexFieldName
        )
        {
            this.indexFieldName = indexFieldName;
            this.collection = collection;
            this.collectionNamespace = collection.CollectionNamespace;
        }

        private string BuildConditions(string indexFieldValue)
        {

            var condition = "";
            if (indexFieldName == "_id")
            {
                condition = Builders<T>.Filter.Eq("_id", ObjectId.Parse(indexFieldValue)).RenderToBsonDocument().ToString();
            }
            else
            {
                condition = "{" + $" \"{indexFieldName}\": " + $"\"{indexFieldValue}\"" + "}";
            }
            return condition;
        }

        private void AddFilterToPipeline(IList<BsonDocument> pipelineStages, string conditionLiteral)
        {
            pipelineStages.Add(BsonDocument.Parse(JsonUtil.CreateCompactLiteral($@"{{
                ""$match"": {conditionLiteral}
            }}")));
        }

        public async Task<T> Get(string indexFieldValue, IDBViewOption options = null)
        {

            FilterDefinition<T> condition = BuildConditions(indexFieldValue);

            var query = collection.Find(condition);

            if (options is { })
            {
                query = View.MakePagination(query, options);
                query = query.Project<T>(View.BuildProjection(options));
                query.Sort(View.BuildSort(options));
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> Get(FilterDefinition<T> condition, IDBViewOption options = null)
        {
            var query = collection.Find(condition);

            if (options is { })
            {
                query = View.MakePagination(query, options);
                query = query.Project<T>(View.BuildProjection(options));
                query.Sort(View.BuildSort(options));
            }

            return await query.ToListAsync();
        }

        public async Task<CUDMessage> Add(T newItem)
        {
            try
            {
                await collection.InsertOneAsync(newItem);
                return new CUDMessage()
                {
                    OK = true,
                    NumAffected = 1,
                    Message = "",
                };
            }
            catch (Exception e)
            {
                return new CUDMessage()
                {
                    OK = false,
                    NumAffected = 0,
                    Message = e.Message.ToString(),
                };
            }
        }

        public async Task<CUDMessage> Add(IEnumerable<T> newItems)
        {
            try
            {
                await collection.InsertManyAsync(newItems);
                return new CUDMessage()
                {
                    OK = true,
                    NumAffected = newItems.Count(),
                    Message = "",
                };
            }
            catch (Exception e)
            {
                return new CUDMessage()
                {
                    OK = false,
                    NumAffected = 0,
                    Message = e.Message.ToString(),
                };
            }
        }

        public async Task<CUDMessage> Update(string indexFieldValue, UpdateDefinition<T> token)
        {

            FilterDefinition<T> condition = BuildConditions(indexFieldValue);

            try
            {
                UpdateResult result = await collection.UpdateOneAsync(condition, token);
                long numUpdated = result.ModifiedCount;
                return new CUDMessage()
                {
                    NumAffected = numUpdated,
                    OK = true,
                };
            }
            catch (Exception e)
            {
                return new CUDMessage()
                {
                    Message = e.Message.ToString(),
                    NumAffected = 0,
                    OK = false,
                };
            }
        }

        public async Task<CUDMessage> Update(FilterDefinition<T> condition, UpdateDefinition<T> token)
        {
            try
            {
                UpdateResult result = await collection.UpdateManyAsync(condition, token);
                long numUpdated = result.ModifiedCount;
                return new CUDMessage()
                {
                    NumAffected = numUpdated,
                    OK = true,
                };
            }
            catch (Exception e)
            {
                return new CUDMessage()
                {
                    Message = e.Message.ToString(),
                    NumAffected = 0,
                    OK = false,
                };
            }
        }

        public async Task<CUDMessage> Delete(string indexFieldValue)
        {

            FilterDefinition<T> condition = BuildConditions(indexFieldValue);

            try
            {
                DeleteResult result = await collection.DeleteOneAsync(condition);
                long numDeleted = result.DeletedCount;
                return new CUDMessage()
                {
                    OK = true,
                    NumAffected = numDeleted,
                    Message = "",
                };
            }
            catch (Exception e)
            {
                return new CUDMessage()
                {
                    OK = false,
                    NumAffected = 0,
                    Message = e.Message.ToString(),
                };
            }
        }

        public async Task<CUDMessage> Delete(FilterDefinition<T> condition)
        {
            try
            {
                DeleteResult result = await collection.DeleteManyAsync(condition);
                long numDeleted = result.DeletedCount;
                return new CUDMessage()
                {
                    OK = true,
                    NumAffected = numDeleted,
                    Message = "",
                };
            }
            catch (Exception e)
            {
                return new CUDMessage()
                {
                    OK = false,
                    NumAffected = 0,
                    Message = e.Message.ToString(),
                };
            }
        }

        public IEnumerable<BsonDocument> BuildLeftJoinPipelineStages(DBLeftJoinOption options)
        {
            return new List<BsonDocument>()
            {
                BsonDocument.Parse(JsonUtil.CreateCompactLiteral($@"{{
                    ""$lookup"": {{
                        ""from"": ""{options.collectionName}"",
                        ""localField"": ""{options.localField}"",
                        ""foreignField"": ""{options.foreignField}"",
                        ""as"": ""jointDBName""
                    }}
                }}")),

                BsonDocument.Parse(JsonUtil.CreateCompactLiteral($@"{{
                    ""$replaceRoot"": {{
                        ""newRoot"": {{
                            ""$mergeObjects"": [
                                {{
                                    ""$arrayElemAt"": [ ""$jointDBName"", 0 ]
                                }},
                                ""$$ROOT""
                            ]
                        }}
                    }}
                }}")),

                BsonDocument.Parse(JsonUtil.CreateCompactLiteral($@"{{
                   ""$project"": {{ ""jointDBName"": 0 }}
                }}")),

            };
        }

        public async Task<TJoint> LeftJoinAndGet<TJoint>(string indexFieldValue, DBLeftJoinOption joinOptions, IDBViewOption viewOption = null)
        {
            List<BsonDocument> pipelineStages = BuildLeftJoinPipelineStages(joinOptions).ToList();

            AddFilterToPipeline(pipelineStages, BuildConditions(indexFieldValue));

            if (viewOption is { })
            {
                if (viewOption.Includes is { } || viewOption.Excludes is { })
                {
                    View.AddProjectionToPipeline(pipelineStages, viewOption);
                }
                if (viewOption.OrderBy != null)
                {
                    View.AddSortToPipeline(pipelineStages, viewOption);
                }
            }

            PipelineDefinition<T, TJoint> pipeline = pipelineStages;

            return await collection.Aggregate(pipeline).FirstOrDefaultAsync();

        }
        public async Task<IEnumerable<TJoint>> LeftJoinAndGet<TJoint>(FilterDefinition<TJoint> condition, DBLeftJoinOption joinOptions, IDBViewOption viewOption = null)
        {
            List<BsonDocument> pipelineStages = BuildLeftJoinPipelineStages(joinOptions).ToList();

            AddFilterToPipeline(pipelineStages, condition.RenderToBsonDocument().ToString());

            if (viewOption is null)
            {
                viewOption = new DBViewOption();
                View.AddPaginationToPipeline(pipelineStages, viewOption);
            }
            else 
            {
                if (viewOption.Includes is { } || viewOption.Excludes is { })
                {
                    View.AddProjectionToPipeline(pipelineStages, viewOption);
                }
                if (viewOption.OrderBy != null)
                {
                    View.AddSortToPipeline(pipelineStages, viewOption);
                }
            }

            PipelineDefinition<T, TJoint> pipeline = pipelineStages;

            return await collection.Aggregate(pipeline).ToListAsync();

        }

        public async Task<CUDMessage> AddItemToList(string instanceArrayFieldName, string arrayIndexFieldValue, string instanceIndexFieldValue)
        {
            UpdateDefinition<T> updateToken = JsonUtil.CreateCompactLiteral($@"{{
                ""$push"": {{
                    ""{instanceArrayFieldName}"": ""{arrayIndexFieldValue}""
                }}
            }}");
            return await Update(instanceIndexFieldValue, updateToken);
        }

        public async Task<CUDMessage> AddItemToList(string instanceArrayFieldName, IEnumerable<string> arrayIndexFieldValues, string instanceIndexFieldValue)
        {
            List<string> quotedNames = (
                from name in arrayIndexFieldValues
                select $@"""{name}"""
            ).ToList();

            UpdateDefinition<T> updateToken = JsonUtil.CreateCompactLiteral($@"{{
                ""$push"": {{
                    ""{instanceArrayFieldName}"": {{
                        ""$each"": [ { string.Join(',', quotedNames) } ]
                    }}
                }}
            }}");
            return await Update(instanceIndexFieldValue, updateToken);
        }

        public async Task<CUDMessage> RemoveItemFromList(string instanceArrayFieldName, string arrayIndexFieldValue, string instanceIndexFieldValue)
        {
            UpdateDefinition<T> updateToken = JsonUtil.CreateCompactLiteral($@"{{
                ""$pull"": {{
                    ""{instanceArrayFieldName}"": ""{arrayIndexFieldValue}""
                }}
            }}");
            return await Update(instanceIndexFieldValue, updateToken);
        }

        public async Task<CUDMessage> RemoveItemFromList(string instanceArrayFieldName, IEnumerable<string> arrayIndexFieldValues, string instanceIndexFieldValue)
        {
            List<string> quotedNames = (
                from name in arrayIndexFieldValues
                select $@"""{name}"""
            ).ToList();

            UpdateDefinition<T> updateToken = JsonUtil.CreateCompactLiteral($@"{{
                ""$pull"": {{
                    ""{instanceArrayFieldName}"": {{
                        ""$in"": [ { string.Join(',', quotedNames) } ]
                    }}
                }}
            }}");
            return await Update(instanceIndexFieldValue, updateToken);
        }

    }

}