using System;
using System.Configuration;
using System.Linq;
using Mongoizer.Core;
using Xunit;
using XunitOrderer;

namespace Mongoizer.Tests {
    [TestPriority(10)]
    public class MongoClientManagerTests : TestClassBase {
        //// NOTE: You should provide the connection settings at the `app.config` before running the tests
        private readonly string _testConnectionString = ConfigurationManager.AppSettings["TEST_CONNSTRING"];
        private readonly string _testDatabase = ConfigurationManager.AppSettings["TEST_DATABASE"];

        [Fact]
        [TestPriority(10)]
        public void GetInstanceWithoutInitializationShouldThrow() => Assert.Throws<NullReferenceException>(delegate {
            MongoClientManager.GetInstance(_testDatabase);
        });

        [Fact]
        [TestPriority(21)]
        public void InitializationWithNoConnectionStringShouldThrow()  => Assert.Throws<ArgumentException>(delegate {
            MongoClientManager.Initialize("", _testDatabase);
        });

        [Fact]
        [TestPriority(21)]
        public void InitializationWithNoDatabaseShouldThrow() => Assert.Throws<ArgumentException>(delegate {
            MongoClientManager.Initialize(_testConnectionString, "");
        });

        [Fact]
        [TestPriority(22)]
        public void InitializationSucceed() {
            MongoClientManager.Initialize(_testConnectionString, _testDatabase);

            var testDatabase = MongoClientManager.GetInstance(_testDatabase);
            var servers = ConfigurationManager.AppSettings["TEST_SERVERS"].Split(',');

            for (var i = 0; i < servers.Length; i++) {
                var host = servers[i].Split(':')[0];
                var port = int.Parse(servers[i].Split(':')[1]);

                var server = testDatabase.Client.Settings.Servers.ToList()[i];

                Assert.Equal(host, server.Host);
                Assert.Equal(port, server.Port);
            }
        }
    }
}
