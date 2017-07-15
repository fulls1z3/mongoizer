using System;
using System.Runtime.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace Mongoizer.Domain {
    [DataContract]
    [Serializable]
    [BsonIgnoreExtraElements(Inherited = true)]
    public abstract class BaseDocument : IDocument {
        [DataMember]
        [BsonId]
        public virtual string Id { get; set; }
    }
}
