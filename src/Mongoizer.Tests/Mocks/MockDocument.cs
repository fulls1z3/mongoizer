using System;
using MongoDB.Bson;
using Mongoizer.Domain;

namespace Mongoizer.Tests.Mocks {
    public class MockObject: BaseDocument<ObjectId> {
        public string Name { get; set; }
    }

    public class MockDocument: BaseDocument<string> {
        public string Name { get; set; }
        public string Value { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public bool IsActive { get; set; }
    }
}
