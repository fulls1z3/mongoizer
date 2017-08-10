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
        public readonly IMongoCollection<T> Collection;

        public MongoRepository(IMongoDatabase database) {
            var typeName = typeof(T).Name;
            var collectionName = typeName.First().ToString(CultureInfo.InvariantCulture).ToLower() + typeName.Substring(1);

            Collection = database.GetCollection<T>(collectionName);
        }

        public async Task<IList<T>> FindAsync(FindOptions<T> findOptions = null) {
            var req = await Collection.FindAsync(new BsonDocument(), findOptions);
            var res = await req.ToListAsync();

            return res;
        }

        public async Task<T> FindAsync(TKey id) {
            if (string.IsNullOrWhiteSpace(id.ToString()))
                throw new ArgumentException(string.Format(Utils.ARGUMENT_EMPTY_MESSAGE, nameof(id)), nameof(id));

            var filter = Builders<T>.Filter.Eq("_id", id);
            var req = await Collection.FindAsync(filter);
            var res = await req.FirstOrDefaultAsync();

            return res;
        }

        public async Task<IList<T>> FindAsync(FilterDefinition<T> filter, FindOptions<T> findOptions = null) {
            if (filter == null)
                throw new ArgumentNullException(string.Format(Utils.ARGUMENT_NULL_MESSAGE, nameof(filter)), nameof(filter));

            var req = await Collection.FindAsync(filter, findOptions);
            var res = await req.ToListAsync();

            return res;
        }

        public async Task<IList<T>> FindAsync(Expression<Func<T, bool>> predicate, FindOptions<T> findOptions = null) {
            if (predicate == null)
                throw new ArgumentNullException(string.Format(Utils.ARGUMENT_NULL_MESSAGE, nameof(predicate)), nameof(predicate));

            var req = await Collection.FindAsync(predicate, findOptions);
            var res = await req.ToListAsync();

            return res;
        }

        public async Task CreateAsync(T item) {
            if (item == null)
                throw new ArgumentNullException(string.Format(Utils.ARGUMENT_NULL_MESSAGE, nameof(item)), nameof(item));
            
            var ex = await FindAsync(item.Id);

            if (ex != null)
                return;

            await Collection.InsertOneAsync(item);
        }

        public async Task CreateAsync(IList<T> items) {
            if (!items.HasItems())
                throw new ArgumentException(string.Format(Utils.ARGUMENT_EMPTY_LIST_MESSAGE, nameof(items)), nameof(items));

            var ids = items.Where(x => !string.IsNullOrWhiteSpace(x.Id.ToString())).Select(x => x.Id).ToList();

            var ex = await FindAsync(Builders<T>.Filter.In("_id", ids));

            if (ex.HasItems())
                return;

            await Collection.InsertManyAsync(items);
        }

        public async Task<bool> ReplaceAsync(TKey id, T item) {
            if (string.IsNullOrWhiteSpace(id.ToString()))
                throw new ArgumentException(string.Format(Utils.ARGUMENT_EMPTY_MESSAGE, nameof(id)), nameof(id));

            if (item == null)
                throw new ArgumentNullException(string.Format(Utils.ARGUMENT_NULL_MESSAGE, nameof(item)), nameof(item));

            var filter = Builders<T>.Filter.Eq("_id", id);
            var response = await Collection.ReplaceOneAsync(filter, item);
            
            return response.IsAcknowledged && response.ModifiedCount == 1;
        }
        
        public async Task<bool> UpdateAsync(TKey id, UpdateDefinition<T> update) {
            if (string.IsNullOrWhiteSpace(id.ToString()))
                throw new ArgumentException(string.Format(Utils.ARGUMENT_EMPTY_MESSAGE, nameof(id)), nameof(id));

            if (update == null)
                throw new ArgumentNullException(string.Format(Utils.ARGUMENT_NULL_MESSAGE, nameof(update)), nameof(update));

            var filter = Builders<T>.Filter.Eq("_id", id);
            var response = await Collection.UpdateOneAsync(filter, update);

            return response.IsAcknowledged && response.ModifiedCount == 1;
        }

        public async Task<long> UpdateAsync(List<string> ids, UpdateDefinition<T> update) {
            if (!ids.HasItems())
                throw new ArgumentException(string.Format(Utils.ARGUMENT_EMPTY_LIST_MESSAGE, nameof(ids)), nameof(ids));

            if (update == null)
                throw new ArgumentNullException(string.Format(Utils.ARGUMENT_NULL_MESSAGE, nameof(update)), nameof(update));

            var filter = Builders<T>.Filter.In("_id", ids);
            var response = await Collection.UpdateManyAsync(filter, update);

            return !(response.IsAcknowledged && response.ModifiedCount > 0) ? 0 : response.ModifiedCount;
        }

        public async Task<long> UpdateAsync(Expression<Func<T, bool>> predicate, UpdateDefinition<T> update) {
            if (predicate == null)
                throw new ArgumentNullException(string.Format(Utils.ARGUMENT_NULL_MESSAGE, nameof(predicate)), nameof(predicate));

            if (update == null)
                throw new ArgumentNullException(string.Format(Utils.ARGUMENT_NULL_MESSAGE, nameof(update)), nameof(update));

            var response = await Collection.UpdateManyAsync(predicate, update);

            return !(response.IsAcknowledged && response.ModifiedCount > 0) ? 0 : response.ModifiedCount;
        }

        public async Task<bool> DeleteAsync(TKey id) {
            if (string.IsNullOrWhiteSpace(id.ToString()))
                throw new ArgumentException(string.Format(Utils.ARGUMENT_EMPTY_MESSAGE, nameof(id)), nameof(id));

            var filter = Builders<T>.Filter.Eq("_id", id);
            var result = await Collection.DeleteOneAsync(filter);

            return result.IsAcknowledged && result.DeletedCount == 1;
        }

        public async Task<long> DeleteAsync(IList<string> ids) {
            if (!ids.HasItems())
                throw new ArgumentException(string.Format(Utils.ARGUMENT_EMPTY_LIST_MESSAGE, nameof(ids)), nameof(ids));

            var filter = Builders<T>.Filter.In("_id", ids);
            var result = await Collection.DeleteManyAsync(filter);

            return !result.IsAcknowledged ? 0 : result.DeletedCount;
        }

        public async Task<long> DeleteAsync(Expression<Func<T, bool>> predicate) {
            if (predicate == null)
                throw new ArgumentNullException(string.Format(Utils.ARGUMENT_NULL_MESSAGE, nameof(predicate)), nameof(predicate));

            var result = await Collection.DeleteManyAsync(predicate);
            
            return !result.IsAcknowledged ? 0 : result.DeletedCount;
        }
    }
}
