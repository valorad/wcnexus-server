using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using MongoDB.Driver;
using WCNexus.App.Models;

namespace WCNexus.App.Services
{
    public interface IDataAccessService<T>
    {
        string indexFieldName { get; set; }
        Task<T> Get(string indexField, IDBViewOption options = null);
        Task<IEnumerable<T>> Get(FilterDefinition<T> condition, IDBViewOption options = null);
        Task<CUDMessage> Add(T newItem);
        Task<CUDMessage> Add(List<T> newItems);
        Task<CUDMessage> Update(string indexField, UpdateDefinition<T> token);
        Task<CUDMessage> Update(FilterDefinition<T> condition, UpdateDefinition<T> token);
        Task<CUDMessage> Delete(string indexField);
        Task<CUDMessage> Delete(FilterDefinition<T> condition);
    }
}