using System;
using System.Runtime.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace Mongoizer.Domain {
    [DataContract]
    [Serializable]
    [BsonIgnoreExtraElements(Inherited = true)]
    public abstract class BaseDocument<TKey> : IDocument<TKey> {
        [DataMember]
        [BsonId]
        public virtual TKey Id { get; set; }
    }
}
