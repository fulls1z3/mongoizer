using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Mongoizer.Domain;

namespace Mongoizer.Core {
    public class MongoRepository<T> : MongoRepository<T, ObjectId>
        where T : IDocument<ObjectId> {

        public MongoRepository(IMongoDatabase database)
            : base(database) {
        }
    }

    public class MongoRepository<T, TKey>
        where T : IDocument<TKey> {

        private readonly IMongoCollection<T> _collection;

        public MongoRepository(IMongoDatabase database) {
            var typeName = typeof(T).Name;
            var collectionName = typeName.First().ToString(CultureInfo.InvariantCulture).ToLower() + typeName.Substring(1);

            _collection = database.GetCollection<T>(collectionName);
        }

        // TODO: paging
        // TODO: size
        // TODO: etc
        public async Task<IList<T>> FindAsync() {
            var req = await _collection.FindAsync<T>(new BsonDocument());
            var res = await req.ToListAsync();

            return res;
        }

        public async Task<T> FindAsync(TKey id) {
            if (string.IsNullOrWhiteSpace(id.ToString()))
                throw new ArgumentException(string.Format(Utils.ARGUMENT_EMPTY_MESSAGE, nameof(id)), nameof(id));

            var filter = Builders<T>.Filter.Eq("_id", id);
            var req = await _collection.FindAsync(filter);
            var res = await req.FirstOrDefaultAsync();

            return res;
        }

        // TODO: paging
        // TODO: size
        // TODO: etc
        public async Task<IList<T>> FindAsync(FilterDefinition<T> filter) {
            if (filter == null)
                throw new ArgumentNullException(string.Format(Utils.ARGUMENT_NULL_MESSAGE, nameof(filter)), nameof(filter));

            var req = await _collection.FindAsync<T>(filter);
            var res = await req.ToListAsync();

            return res;
        }

        // TODO: paging
        // TODO: size
        // TODO: etc
        public async Task<IList<T>> FindAsync(Expression<Func<T, bool>> predicate) {
            if (predicate == null)
                throw new ArgumentNullException(string.Format(Utils.ARGUMENT_NULL_MESSAGE, nameof(predicate)), nameof(predicate));

            var req = await _collection.FindAsync<T>(predicate);
            var res = await req.ToListAsync();

            return res;
        }

        public async Task CreateAsync(T item) {
            if (item == null)
                throw new ArgumentNullException(string.Format(Utils.ARGUMENT_NULL_MESSAGE, nameof(item)), nameof(item));
            
            var ex = await FindAsync(item.Id);

            if (ex != null)
                return;

            await _collection.InsertOneAsync(item);
        }

        public async Task CreateAsync(IList<T> items) {
            if (!items.HasItems())
                throw new ArgumentException(string.Format(Utils.ARGUMENT_EMPTY_LIST_MESSAGE, nameof(items)), nameof(items));

            var ids = items.Where(x => !string.IsNullOrWhiteSpace(x.Id.ToString())).Select(x => x.Id).ToList();

            var ex = await FindAsync(Builders<T>.Filter.In("_id", ids));

            if (ex.HasItems())
                return;

            await _collection.InsertManyAsync(items);
        }

        public async Task<bool> ReplaceAsync(string id, T item) {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(string.Format(Utils.ARGUMENT_EMPTY_MESSAGE, nameof(id)), nameof(id));

            if (item == null)
                throw new ArgumentNullException(string.Format(Utils.ARGUMENT_NULL_MESSAGE, nameof(item)), nameof(item));

            var filter = Builders<T>.Filter.Eq("_id", id);
            var response = await _collection.ReplaceOneAsync(filter, item);
            
            return response.IsAcknowledged && response.ModifiedCount == 1;
        }
        
        public async Task<bool> UpdateAsync(string id, UpdateDefinition<T> update) {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(string.Format(Utils.ARGUMENT_EMPTY_MESSAGE, nameof(id)), nameof(id));

            if (update == null)
                throw new ArgumentNullException(string.Format(Utils.ARGUMENT_NULL_MESSAGE, nameof(update)), nameof(update));

            var filter = Builders<T>.Filter.Eq("_id", id);
            var response = await _collection.UpdateOneAsync(filter, update);

            return response.IsAcknowledged && response.ModifiedCount == 1;
        }

        public async Task<long> UpdateAsync(List<string> ids, UpdateDefinition<T> update) {
            if (!ids.HasItems())
                throw new ArgumentException(string.Format(Utils.ARGUMENT_EMPTY_LIST_MESSAGE, nameof(ids)), nameof(ids));

            if (update == null)
                throw new ArgumentNullException(string.Format(Utils.ARGUMENT_NULL_MESSAGE, nameof(update)), nameof(update));

            var filter = Builders<T>.Filter.In("_id", ids);
            var response = await _collection.UpdateManyAsync(filter, update);

            return !(response.IsAcknowledged && response.ModifiedCount > 0) ? 0 : response.ModifiedCount;
        }

        public async Task<long> UpdateAsync(Expression<Func<T, bool>> predicate, UpdateDefinition<T> update) {
            if (predicate == null)
                throw new ArgumentNullException(string.Format(Utils.ARGUMENT_NULL_MESSAGE, nameof(predicate)), nameof(predicate));

            if (update == null)
                throw new ArgumentNullException(string.Format(Utils.ARGUMENT_NULL_MESSAGE, nameof(update)), nameof(update));

            var response = await _collection.UpdateManyAsync(predicate, update);

            return !(response.IsAcknowledged && response.ModifiedCount > 0) ? 0 : response.ModifiedCount;
        }

        public async Task<bool> DeleteAsync(string id) {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(string.Format(Utils.ARGUMENT_EMPTY_MESSAGE, nameof(id)), nameof(id));

            var filter = Builders<T>.Filter.Eq("_id", id);
            var result = await _collection.DeleteOneAsync(filter);

            return result.IsAcknowledged && result.DeletedCount == 1;
        }

        public async Task<long> DeleteAsync(IList<string> ids) {
            if (!ids.HasItems())
                throw new ArgumentException(string.Format(Utils.ARGUMENT_EMPTY_LIST_MESSAGE, nameof(ids)), nameof(ids));

            var filter = Builders<T>.Filter.In("_id", ids);
            var result = await _collection.DeleteManyAsync(filter);

            return !result.IsAcknowledged ? 0 : result.DeletedCount;
        }

        public async Task<long> DeleteAsync(Expression<Func<T, bool>> predicate) {
            if (predicate == null)
                throw new ArgumentNullException(string.Format(Utils.ARGUMENT_NULL_MESSAGE, nameof(predicate)), nameof(predicate));

            var result = await _collection.DeleteManyAsync(predicate);
            
            return !result.IsAcknowledged ? 0 : result.DeletedCount;
        }
    }
}
