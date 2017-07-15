using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Mongoizer.Core {
    public static class MongoClientManager {
        private static Dictionary<string, IMongoDatabase> _databases;

        private static IMongoDatabase GetDatabase(string connectionString,
                                                  string database) {
            var client = new MongoClient(connectionString);

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("The argument `connectionString` cannot be null or all whitespace.",
                    nameof(connectionString));

            if (string.IsNullOrWhiteSpace(database))
                throw new ArgumentException("The argument `database` cannot be null or all whitespace.",
                    nameof(database));

            var conventionPack = new ConventionPack {
                new CamelCaseElementNameConvention()
            };
            ConventionRegistry.Register("mongoizer", conventionPack, type => true);

            return client.GetDatabase(database);
        }

        public static void Initialize(string connectionString,
                                      string database) {
            if (_databases == null)
                _databases = new Dictionary<string, IMongoDatabase>();

            if (_databases.ContainsKey(database))
                _databases[database] = GetDatabase(connectionString, database);
            else
                _databases.Add(database, GetDatabase(connectionString, database));
        }

        public static IMongoDatabase GetInstance(string index) => _databases[index];
    }
}
