using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using WCNexus.App.Models;

namespace WCNexus.App.Services
{
    public interface IDataAccessService<T>
    {
        CollectionNamespace collectionNamespace { get; }
        string indexFieldName { get; }
        Task<T> Get(string indexField, IDBViewOption options = null);
        Task<IEnumerable<T>> Get(FilterDefinition<T> condition, IDBViewOption options = null);
        Task<CUDMessage> Add(T newItem);
        Task<CUDMessage> Add(IEnumerable<T> newItems);
        Task<CUDMessage> Update(string indexField, UpdateDefinition<T> token);
        Task<CUDMessage> Update(FilterDefinition<T> condition, UpdateDefinition<T> token);
        Task<CUDMessage> Delete(string indexField);
        Task<CUDMessage> Delete(FilterDefinition<T> condition);
        Task<TJoint> LeftJoinAndGet<TJoint>(string indexFieldValue, DBLeftJoinOption joinOptions, IDBViewOption viewOption = null);
        Task<IEnumerable<TJoint>> LeftJoinAndGet<TJoint>(FilterDefinition<TJoint> condition, DBLeftJoinOption joinOptions, IDBViewOption viewOption = null);

    }
}