using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using Mongoizer.Core;
using Mongoizer.Tests.Mocks;
using Xunit;
using XunitOrderer;

namespace Mongoizer.Tests {
    [TestPriority(20)]
    public class MongoRepositoryTests : TestClassBase {
        // NOTE: You should provide the connection settings at the `app.config` before running the tests
        private readonly string _testConnectionString = ConfigurationManager.AppSettings["TEST_CONNSTRING"];
        private readonly string _testDatabase = ConfigurationManager.AppSettings["TEST_DATABASE"];
        private readonly MongoRepository<MockObject> _mockObjectRepo;
        private readonly MongoRepository<MockDocument, string> _mockDocumentRepo;

        private readonly MockObject _mockObject;
        private readonly MockDocument _mockDocument98;
        private readonly MockDocument _mockDocument99;

        public MongoRepositoryTests() {
            _mockObject = new MockObject {
                Name = "mock #0"
            };

            _mockDocument99 = new MockDocument {
                Id = "98",
                Name = "mock #1",
                Value = "mock #1's value",
                CreationDate = DateTime.UtcNow,
                IsActive = true
            };

            _mockDocument98 = new MockDocument {
                Id = "99",
                Name = "mock #2",
                Value = "mock #2's value",
                CreationDate = DateTime.UtcNow,
                IsActive = false
            };

            MongoClientManager.Initialize(_testConnectionString, _testDatabase);
            
            var database = MongoClientManager.GetInstance(_testDatabase);
            _mockObjectRepo = new MongoRepository<MockObject>(database);
            _mockDocumentRepo = new MongoRepository<MockDocument, string>(database);
        }

        #region Create
        [Fact]
        [TestPriority(10)]
        public async Task CreateAsyncByItemShouldSucceed() {
            await _mockObjectRepo.CreateAsync(_mockObject);
            await _mockDocumentRepo.CreateAsync(_mockDocument98);

            var item = await _mockDocumentRepo.FindAsync(_mockDocument98.Id);

            Assert.Equal(_mockDocument98.Id, item.Id);
            Assert.Equal(_mockDocument98.Name, item.Name);
            Assert.Equal(_mockDocument98.Value, item.Value);
            Assert.True(_mockDocument98.CreationDate.EqualsMongoDateTime(item.CreationDate));
            Assert.Equal(_mockDocument98.IsActive, item.IsActive);
        }
        
        [Fact]
        [TestPriority(11)]
        public async Task CreateAsyncByExistingItemShouldFail() {
            var results1 = await _mockDocumentRepo.FindAsync();

            await _mockDocumentRepo.CreateAsync(_mockDocument98);
            var results2 = await _mockDocumentRepo.FindAsync();

            Assert.Equal(1, results1.Count);
            Assert.Equal(1, results2.Count);
        }

        [Fact]
        [TestPriority(12)]
        public async Task CreateAsyncByNullItemShouldThrow() => await Assert.ThrowsAsync<ArgumentNullException>(
            async delegate { await _mockDocumentRepo.CreateAsync((MockDocument)null); });

        [Fact]
        [TestPriority(13)]
        public async Task CreateAsyncByItemsShouldSucceed() {
            var deleteCount = await _mockDocumentRepo.DeleteAsync(x => true);
            var items = new List<MockDocument> {_mockDocument98, _mockDocument99};
            await _mockDocumentRepo.CreateAsync(items);

            var results = await _mockDocumentRepo.FindAsync();
            
            Assert.Equal(1, deleteCount);
            Assert.NotEmpty(results);
            Assert.Equal(2, results.Count);
        }

        [Fact]
        [TestPriority(14)]
        public async Task CreateAsyncByExistingItemsShouldFail() {
            var items = new List<MockDocument> {_mockDocument98, _mockDocument99};
            
            var results1 = await _mockDocumentRepo.FindAsync();
            
            await _mockDocumentRepo.CreateAsync(items);
            var results2 = await _mockDocumentRepo.FindAsync();
            
            Assert.Equal(2, results1.Count);
            Assert.Equal(2, results2.Count);
        }

        [Fact]
        [TestPriority(15)]
        public async Task CreateAsyncByEmptyItemsShouldThrow() => await Assert.ThrowsAsync<ArgumentException>(
            async delegate { await _mockDocumentRepo.CreateAsync(new List<MockDocument>()); });
        #endregion

        #region Find
        [Fact]
        [TestPriority(20)]
        public async Task FindAsyncShouldSucceed() {
            var items = await _mockDocumentRepo.FindAsync();

            Assert.NotNull(items);
            Assert.Equal(2, items.Count);
        }

        [Fact]
        [TestPriority(21)]
        public async Task FindAsyncByIdShouldSucceed() {
            var item1 = await _mockDocumentRepo.FindAsync(_mockDocument98.Id);
            var item2 = await _mockDocumentRepo.FindAsync("xyz");

            Assert.NotNull(item1);
            Assert.Equal(_mockDocument98.Id, item1.Id);
            Assert.Equal(_mockDocument98.Name, item1.Name);
            Assert.Equal(_mockDocument98.Value, item1.Value);
            Assert.Equal(_mockDocument98.IsActive, item1.IsActive);

            Assert.Null(item2);
        }

        [Fact]
        [TestPriority(22)]
        public async Task FindAsyncByEmptyIdShouldThrow() => await Assert.ThrowsAsync<ArgumentException>(
            async delegate { await _mockDocumentRepo.FindAsync(""); });

        [Fact]
        [TestPriority(23)]
        public async Task FindAsyncByFilterShouldSucceed() {
            var items1 = await _mockDocumentRepo.FindAsync();
            var items2 = await _mockDocumentRepo.FindAsync(Builders<MockDocument>.Filter.Eq("isActive", false));
            var items3 = await _mockDocumentRepo.FindAsync(Builders<MockDocument>.Filter.Eq("name", "xyz"));

            Assert.NotEmpty(items1);
            Assert.Equal(2, items1.Count);

            Assert.NotEmpty(items2);
            Assert.Equal(1, items2.Count);

            Assert.NotNull(items3);
            Assert.Empty(items3);
        }

        [Fact]
        [TestPriority(24)]
        public async Task FindAsyncByNullFilterShouldThrow() => await Assert.ThrowsAsync<ArgumentNullException>(
            async delegate { await _mockDocumentRepo.FindAsync((FilterDefinition<MockDocument>)null); });

        [Fact]
        [TestPriority(25)]
        public async Task FindAsyncByPredicateShouldSucceed() {
            var items1 = await _mockDocumentRepo.FindAsync();
            var items2 = await _mockDocumentRepo.FindAsync(x => x.IsActive);
            var items3 = await _mockDocumentRepo.FindAsync(x => x.Name == "xyz");

            Assert.NotEmpty(items1);
            Assert.Equal(2, items1.Count);

            Assert.NotEmpty(items2);
            Assert.Equal(1, items2.Count);

            Assert.NotNull(items3);
            Assert.Empty(items3);
        }

        [Fact]
        [TestPriority(26)]
        public async Task FindAsyncByNullPredicateShouldThrow() => await Assert.ThrowsAsync<ArgumentNullException>(
            async delegate { await _mockDocumentRepo.FindAsync((Expression<Func<MockDocument, bool>>)null); });
        #endregion

        #region Replace/Update
        [Fact]
        [TestPriority(30)]
        public async Task ReplaceAsyncByIdAndItemShouldSucceed() {
            var item1 = await _mockDocumentRepo.FindAsync(_mockDocument98.Id);
            var item2 = await _mockDocumentRepo.FindAsync(_mockDocument99.Id);

            var utcNow = DateTime.UtcNow;

            var updateResult1 = await _mockDocumentRepo.ReplaceAsync(item1.Id,
                new MockDocument {
                    Id = item1.Id,
                    UpdateDate = utcNow,
                    IsActive = true
                });
            var updateResult2 = await _mockDocumentRepo.ReplaceAsync(item2.Id,
                new MockDocument {
                    Id = item2.Id,
                    UpdateDate = utcNow,
                    IsActive = true
                });

            var updatedItem1 = await _mockDocumentRepo.FindAsync(item1.Id);
            var updatedItem2 = await _mockDocumentRepo.FindAsync(item2.Id);

            Assert.True(updateResult1);
            Assert.NotNull(updatedItem1);
            Assert.Null(updatedItem1.Name);
            Assert.Equal(DateTime.MinValue, updatedItem1.CreationDate);
            Assert.True(utcNow.EqualsMongoDateTime(updatedItem1.UpdateDate));
            Assert.True(updatedItem1.IsActive);

            Assert.True(updateResult2);
            Assert.NotNull(updatedItem2);
            Assert.Null(updatedItem2.Name);
            Assert.Equal(DateTime.MinValue, updatedItem2.CreationDate);
            Assert.True(utcNow.EqualsMongoDateTime(updatedItem2.UpdateDate));
            Assert.True(updatedItem2.IsActive);
        }

        [Fact]
        [TestPriority(31)]
        public async Task ReplaceAsyncByIdAndNonExistingItemShouldFail() {
            var updateResult = await _mockDocumentRepo.ReplaceAsync("0",
                new MockDocument {
                    UpdateDate = DateTime.UtcNow,
                    IsActive = true
                });

            Assert.False(updateResult);
        }

        [Fact]
        [TestPriority(32)]
        public async Task ReplaceAsyncByEmptyIdAndItemShouldThrow() => await Assert.ThrowsAsync<ArgumentException>(
            async delegate { await _mockDocumentRepo.ReplaceAsync("", new MockDocument()); });

        [Fact]
        [TestPriority(33)]
        public async Task ReplaceAsyncByIdAndNullItemShouldThrow() => await Assert.ThrowsAsync<ArgumentNullException>(
            async delegate { await _mockDocumentRepo.ReplaceAsync("0", null); });

        [Fact]
        [TestPriority(34)]
        public async Task UpdateAsyncByIdAndUpdateDefinitionShouldSucceed() {
            var item1 = await _mockDocumentRepo.FindAsync(_mockDocument98.Id);
            var item2 = await _mockDocumentRepo.FindAsync(_mockDocument99.Id);

            var utcNow = DateTime.UtcNow;

            var updateResult1 = await _mockDocumentRepo.UpdateAsync(item1.Id, Builders<MockDocument>.Update.Set("updateDate", utcNow).Set("isActive", false));
            var updateResult2 = await _mockDocumentRepo.UpdateAsync(item2.Id, Builders<MockDocument>.Update.Set("updateDate", utcNow).Set("isActive", false));

            var updatedItem1 = await _mockDocumentRepo.FindAsync(item1.Id);
            var updatedItem2 = await _mockDocumentRepo.FindAsync(item2.Id);

            Assert.True(updateResult1);
            Assert.NotNull(updatedItem1);
            Assert.True(utcNow.EqualsMongoDateTime(updatedItem1.UpdateDate));
            Assert.False(updatedItem1.IsActive);

            Assert.True(updateResult2);
            Assert.NotNull(updatedItem2);
            Assert.True(utcNow.EqualsMongoDateTime(updatedItem2.UpdateDate));
            Assert.False(updatedItem2.IsActive);
        }

        [Fact]
        [TestPriority(35)]
        public async Task UpdateAsyncByNonExistingIdAndUpdateDefinitionShouldFail() {
            var updateResult = await _mockDocumentRepo.UpdateAsync("0", Builders<MockDocument>.Update.Set("updateDate", DateTime.UtcNow).Set("isActive", false));

            Assert.False(updateResult);
        }

        [Fact]
        [TestPriority(36)]
        public async Task UpdateAsyncByEmptyIdAndUpdateDefinitionShouldThrow() => await Assert.ThrowsAsync<ArgumentException>(
            async delegate { await _mockDocumentRepo.UpdateAsync("", null); });

        [Fact]
        [TestPriority(37)]
        public async Task UpdateAsyncByIdAndNullUpdateDefinitionShouldThrow() => await Assert.ThrowsAsync<ArgumentNullException>(
            async delegate { await _mockDocumentRepo.UpdateAsync("0", null); });

        [Fact]
        [TestPriority(38)]
        public async Task UpdateAsyncByIdsAndUpdateDefinitionShouldSucceed() {
            var items = await _mockDocumentRepo.FindAsync();
            var ids = items.Select(x => x.Id).ToList();

            const string value = "corrected value";
            var utcNow = DateTime.UtcNow;

            var count = await _mockDocumentRepo.UpdateAsync(ids, Builders<MockDocument>.Update.Set("value", value).Set("updateDate", utcNow));
            items = await _mockDocumentRepo.FindAsync();

            Assert.Equal(2, count);

            foreach (var item in items) {
                Assert.NotNull(item);
                Assert.Equal(value, item.Value);
                Assert.True(utcNow.EqualsMongoDateTime(item.UpdateDate));
            }
        }

        [Fact]
        [TestPriority(39)]
        public async Task UpdateAsyncByNonExistingIdsAndUpdateDefinitionShouldFail() {
            var count = await _mockDocumentRepo.UpdateAsync(new List<string>{"xyz"}, Builders<MockDocument>.Update.Set("updateDate", DateTime.UtcNow).Set("isActive", false));

            Assert.Equal(0, count);
        }

        [Fact]
        [TestPriority(40)]
        public async Task UpdateAsyncByEmptyIdsAndUpdateDefinitionShouldThrow() => await Assert.ThrowsAsync<ArgumentException>(
            async delegate { await _mockDocumentRepo.UpdateAsync(new List<string>(), null); });

        [Fact]
        [TestPriority(41)]
        public async Task UpdateAsyncByIdsAndNullUpdateDefinitionShouldThrow() => await Assert.ThrowsAsync<ArgumentNullException>(
            async delegate { await _mockDocumentRepo.UpdateAsync(new List<string>{ "1", "2" }, null); });

        [Fact]
        [TestPriority(42)]
        public async Task UpdateAsyncByPredicateAndUpdateDefinitionShouldSucceed() {
            const string value = "corrected value #2";

            var count = await _mockDocumentRepo.UpdateAsync(x => !x.IsActive, Builders<MockDocument>.Update.Set("value", value));
            var items = await _mockDocumentRepo.FindAsync();

            Assert.Equal(2, count);

            foreach (var item in items) {
                Assert.NotNull(item);
                Assert.Equal(value, item.Value);
            }
        }

        [Fact]
        [TestPriority(43)]
        public async Task UpdateAsyncByNonExistingPredicateAndUpdateDefinitionShouldFail() {
            var count = await _mockDocumentRepo.UpdateAsync(x => x.Id == "xyz", Builders<MockDocument>.Update.Set("value", "corrected value #2"));

            Assert.Equal(0, count);
        }

        [Fact]
        [TestPriority(44)]
        public async Task UpdateAsyncByNullPredicateAndUpdateDefinitionShouldThrow() => await Assert.ThrowsAsync<ArgumentNullException>(
            async delegate { await _mockDocumentRepo.UpdateAsync((Expression<Func<MockDocument, bool>>)null, null); });

        [Fact]
        [TestPriority(45)]
        public async Task UpdateAsyncByPredicateAndNullUpdateDefinitionShouldThrow() => await Assert.ThrowsAsync<ArgumentNullException>(
            async delegate { await _mockDocumentRepo.UpdateAsync(x => !string.IsNullOrWhiteSpace(x.Id), null); });
        #endregion

        #region Delete
        [Fact]
        [TestPriority(50)]
        public async Task DeleteAsyncByIdShouldSucceed() {
            var deleteResult1 = await _mockDocumentRepo.DeleteAsync(_mockDocument98.Id);
            var deleteResult2 = await _mockDocumentRepo.DeleteAsync(_mockDocument99.Id);

            var items = await _mockDocumentRepo.FindAsync();

            Assert.True(deleteResult1);
            Assert.True(deleteResult2);

            Assert.NotNull(items);
            Assert.Collection(items);
            Assert.Empty(items);
        }

        [Fact]
        [TestPriority(51)]
        public async Task DeleteAsyncByNonExistingIdShouldFail() {
            var deleteResult = await _mockDocumentRepo.DeleteAsync("xyz");

            Assert.False(deleteResult);
        }

        [Fact]
        [TestPriority(52)]
        public async Task DeleteAsyncByEmptyIdShouldThrow() => await Assert.ThrowsAsync<ArgumentException>(
            async delegate { await _mockDocumentRepo.DeleteAsync(""); });

        [Fact]
        [TestPriority(53)]
        public async Task DeleteAsyncByIdsShouldSucceed() {
            var items = new List<MockDocument> {_mockDocument98, _mockDocument99};
            await _mockDocumentRepo.CreateAsync(items);
            
            var results = await _mockDocumentRepo.FindAsync();
            var ids = results.Select(x => x.Id).ToList();

            var deleteCount = await _mockDocumentRepo.DeleteAsync(ids);
            results = await _mockDocumentRepo.FindAsync();

            Assert.Equal(2, ids.Count);
            Assert.Equal(2, deleteCount);

            Assert.NotNull(results);
            Assert.Collection(results);
            Assert.Empty(results);
        }

        [Fact]
        [TestPriority(54)]
        public async Task DeleteAsyncByNonExistingIdsShouldFail() {
            var deleteCount = await _mockDocumentRepo.DeleteAsync(new List<string>{"xyz"});

            Assert.Equal(0, deleteCount);
        }

        [Fact]
        [TestPriority(55)]
        public async Task DeleteAsyncByEmptyIdsShouldThrow() => await Assert.ThrowsAsync<ArgumentException>(
            async delegate { await _mockDocumentRepo.DeleteAsync(new List<string>()); });

        [Fact]
        [TestPriority(56)]
        public async Task DeleteAsyncByPredicateShouldSucceed() {
            var itemsDoc = new List<MockDocument> {_mockDocument98, _mockDocument99};
            
            foreach (var item in itemsDoc)
                item.IsActive = false;

            await _mockDocumentRepo.CreateAsync(itemsDoc);
            
            var searchDocResults = await _mockDocumentRepo.FindAsync();
            var count = searchDocResults.Count;

            var searchObjResults = await _mockObjectRepo.FindAsync();
            count += searchObjResults.Count;

            var deleteCount = await _mockDocumentRepo.DeleteAsync(x => !x.IsActive);
            searchDocResults = await _mockDocumentRepo.FindAsync();

            deleteCount += await _mockObjectRepo.DeleteAsync(x => true);
            searchObjResults = await _mockObjectRepo.FindAsync();

            Assert.Equal(3, count);
            Assert.Equal(3, deleteCount);

            Assert.NotNull(searchDocResults);
            Assert.NotNull(searchObjResults);
            Assert.Collection(searchDocResults);
            Assert.Collection(searchObjResults);
            Assert.Empty(searchDocResults);
            Assert.Empty(searchObjResults);
        }

        [Fact]
        [TestPriority(57)]
        public async Task DeleteAsyncByNonExistingPredicateShouldFail() {
            var deleteCount = await _mockDocumentRepo.DeleteAsync(x => x.Id == "xyz");

            Assert.Equal(0, deleteCount);
        }

        [Fact]
        [TestPriority(58)]
        public async Task DeleteAsyncByNullDescriptorShouldThrow() => await Assert.ThrowsAsync<ArgumentNullException>(
            async delegate { await _mockDocumentRepo.DeleteAsync((Expression<Func<MockDocument, bool>>)null); });
        #endregion
    }
}
