using System.Collections.Generic;
using System.Threading.Tasks;
using CommonLibrary.DataAccess.Abstractions;
using MongoDB.Driver;

namespace CommonLibrary.DataAccess
{
    public interface IDatabaseServicesSchema<T> where T : ICollectionSchema
    {
        Task<T> CreateItem(T collectionItem);
        Task<List<T>> GetAll();
        Task<T> GetById(string id);
        Task<ReplaceOneResult> UpdateById(string id, T updatedItem);
        Task<DeleteResult> RemoveById(string id);
        Task<DeleteResult> RemoveAll();
    }
}