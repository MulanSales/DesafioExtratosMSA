using System.Collections.Generic;
using System.Threading.Tasks;
using CommonLibrary.DataAccess.Abstractions;
using MongoDB.Driver;

namespace CommonLibrary.DataAccess
{
    public class DatabaseServicesSchema<T> : IDatabaseServicesSchema<T> where T : ICollectionSchema 
    {
        protected IMongoCollection<T> _collection; 

        private IDatabaseConnectorSettings _settings;

        public DatabaseServicesSchema(IDatabaseConnectorSettings settings)
        {
            this._settings = settings;

            var client = new MongoClient(_settings.ConnectionString);
            IMongoDatabase database = client.GetDatabase(_settings.DatabaseName);

            string collectionName = typeof(T).Name;

            _collection = database.GetCollection<T>(collectionName);
        }

        public virtual async Task<T> CreateItem(T collectionItem) {
            await _collection.InsertOneAsync(collectionItem);
            return collectionItem;
        }

        public virtual async Task<List<T>> GetAll() {
            IAsyncCursor<T> itemsList = await _collection.FindAsync(item => true);
            return await itemsList.ToListAsync();
        }

        public virtual async Task<T> GetById(string id) {
            IAsyncCursor<T> collectionItem = await _collection.FindAsync<T>(item => item.Id == id);
            return await collectionItem.FirstOrDefaultAsync();
        }

        public virtual async Task<ReplaceOneResult> UpdateById(string id, T updatedItem) {
            return await _collection.ReplaceOneAsync(item => item.Id == id, updatedItem);
        }

        public virtual async Task<DeleteResult> RemoveById(string id) {
            return await _collection.DeleteOneAsync(item => item.Id == id);
        }

        public virtual async Task<DeleteResult> RemoveAll() {
            return await _collection.DeleteManyAsync(item => item.Id != null);
        }
    }
}