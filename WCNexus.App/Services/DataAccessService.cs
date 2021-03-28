using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using WCNexus.App.Database;
using WCNexus.App.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace WCNexus.App.Services
{
    public class DataAccessService<T> : IDataAccessService<T>
    {

        private readonly IMongoCollection<T> collection;
        public string indexFieldName { get; set; }
        public DataAccessService(IMongoCollection<T> collection)
        {
            this.collection = collection;
        }

        private FilterDefinition<T> BuildConditions(string indexFieldValue)
        {

            FilterDefinition<T> condition;
            if (indexFieldName == "_id")
            {
                condition = Builders<T>.Filter.Eq("_id", ObjectId.Parse(indexFieldValue));
            }
            else
            {
                condition = "{" + $" \"{indexFieldName}\": " + $"\"{indexFieldValue}\"" + "}";
            }
            return condition;

        }

        public async Task<T> Get(string indexFieldValue, IDBViewOption options = null)
        {

            FilterDefinition<T> condition = BuildConditions(indexFieldValue);

            var query = collection.Find(condition);

            if (options is { })
            {
                query = View.MakePagination(query, options);
                query = query.Project<T>(View.BuildProjection<T>(options));
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
                query = query.Project<T>(View.BuildProjection<T>(options));
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
                    OK = true,
                    NumAffected = 0,
                    Message = e.ToString(),
                };
            }
        }

        public async Task<CUDMessage> Add(List<T> newItems)
        {
            try
            {
                await collection.InsertManyAsync(newItems);
                return new CUDMessage()
                {
                    OK = true,
                    NumAffected = newItems.Count,
                    Message = "",
                };
            }
            catch (Exception e)
            {
                return new CUDMessage()
                {
                    OK = false,
                    NumAffected = 0,
                    Message = e.ToString(),
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
                    Message = e.ToString(),
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
                    Message = e.ToString(),
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
                    Message = e.ToString(),
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
                    Message = e.ToString(),
                };
            }
        }

    }
}