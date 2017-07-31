using MongoDB.Bson;

namespace Mongoizer.Domain {
    public interface IDocument {
        ObjectId Id { get; set; }
    }

    public interface IDocument<TKey> {
        TKey Id { get; set; }
    }
}
