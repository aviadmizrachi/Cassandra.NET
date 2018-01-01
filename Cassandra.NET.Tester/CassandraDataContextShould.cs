using Cassandra.NET.Exceptions;
using Cassandra.NET.Tester.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Cassandra.NET.Tester
{
    [TestClass]
    public class CassandraDataContextShould
    {
        private CassandraDataContext dataContext;

        static string query = "drop keyspace IF EXISTS demo;" + Environment.NewLine +
                              "create keyspace demo with replication = {'class':'SimpleStrategy', 'replication_factor':1};" + Environment.NewLine +
                              "CREATE TABLE IF NOT EXISTS demo.user_results(user_id text, time timestamp, result float, PRIMARY KEY(user_id, time));";

        [TestInitialize]
        public void TestInitialize()
        {
            var contactPoints = new[] { "127.0.0.1" };
            var cluster = Cluster.Builder().AddContactPoints(contactPoints).Build();
            var session = cluster.Connect();

            var lines = query.Split(Environment.NewLine);
            foreach (var line in lines)
                session.Execute(line);

            dataContext = new CassandraDataContext(contactPoints, "demo");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            dataContext.Dispose();
        }

        [TestMethod]
        public void BeCreated()
        {
            Assert.IsNotNull(dataContext);
        }

        [TestMethod]
        public void AddUserResult()
        {
            var timestamp = DateTime.UtcNow;

            var userResult = new UserResultModel
            {
                user_id = "test_user_id",
                result = 15.55F,
                time = timestamp
            };

            dataContext.AddOrUpdate(userResult);
        }

        [TestMethod]
        public void FetchCreatedUser()
        {
            var timestamp = DateTimeOffset.UtcNow;

            var userResult = new UserResultModel
            {
                user_id = "test_user_id",
                result = 15.55F,
                time = timestamp
            };

            dataContext.AddOrUpdate(userResult);

            var currentUser = dataContext.SingleOrDefault<UserResultModel>(u => u.user_id == "test_user_id" && u.time == timestamp);
            Assert.IsNotNull(currentUser);
            Assert.AreEqual("test_user_id", currentUser.user_id);
            Assert.AreEqual(15.55F, currentUser.result);
        }

        [TestMethod]
        [ExpectedException(typeof(MissingTableAttributeException))]
        public void FailWhenCassandraTableIsNotDefined()
        {
            var anon = new
            {
                name = "1234",
                time = DateTimeOffset.Now
            };

            dataContext.AddOrUpdate(anon);
        }

        [TestMethod]
        public void AddUserResultWithMapping()
        {
            var timestamp = DateTime.UtcNow;

            var userResult = new UserResultModelWithMapping
            {
                UserId = "test_user_id",
                Result = 15.55F,
                Timestamp = timestamp
            };

            dataContext.AddOrUpdate(userResult);

            var savedResult = dataContext.SingleOrDefault<UserResultModelWithMapping>(u => u.UserId == "test_user_id" && u.Timestamp == timestamp);

            Assert.IsNotNull(savedResult);
            Assert.AreEqual("test_user_id", savedResult.UserId);
            Assert.AreEqual(15.55F, savedResult.Result);
        }

        [TestMethod]
        public void AddUserResultWithIgnoreProperty()
        {
            var timestamp = DateTime.UtcNow;

            var userResult = new UserResultModelWithIgnoreProperty
            {
                UserId = "test_user_id",
                Result = 15.55F,
                Timestamp = timestamp,
                ResultFactor = 5
            };

            dataContext.AddOrUpdate(userResult);

            dataContext.Sum<UserResultModelWithMapping, float>(u => u.UserId == "test_user_id", u => u.Result);

            var savedResult = dataContext.SingleOrDefault<UserResultModelWithIgnoreProperty>(u => u.UserId == "test_user_id" && u.Timestamp == timestamp);

            Assert.IsNotNull(savedResult);
            Assert.AreEqual("test_user_id", savedResult.UserId);
            Assert.AreEqual(15.55F, savedResult.Result);
            Assert.AreEqual(default(int), savedResult.ResultFactor);
        }

        [TestMethod]
        public void CalcAverageBasedOnTimeRange()
        {
            var from = DateTime.UtcNow.AddHours(-1);
            var to = DateTime.UtcNow;

            dataContext.Average<UserResultModelWithMapping, float>(u => u.UserId == "test_user_id" && u.Timestamp >= from && u.Timestamp <= to, u => u.Result);
        }

        [TestMethod]
        public void CalcMax()
        {
            dataContext.AddOrUpdate(new UserResultModelWithMapping("user_1", DateTime.Now, 55.5F));
            dataContext.AddOrUpdate(new UserResultModelWithMapping("user_1", DateTime.Now.AddMinutes(1), 88.8F));
            dataContext.AddOrUpdate(new UserResultModelWithMapping("user_1", DateTime.Now.AddMinutes(2), 22.2F));

            var max = dataContext.Max<UserResultModelWithMapping, float>(m => m.UserId == "user_1", m => m.Result);
            Assert.AreEqual(88.8F, max);
        }

        [TestMethod]
        public void CalcMin()
        {
            dataContext.AddOrUpdate(new UserResultModelWithMapping("user_1", DateTime.Now, 55.5F));
            dataContext.AddOrUpdate(new UserResultModelWithMapping("user_1", DateTime.Now.AddMinutes(1), 88.8F));
            dataContext.AddOrUpdate(new UserResultModelWithMapping("user_1", DateTime.Now.AddMinutes(2), 22.2F));

            var max = dataContext.Min<UserResultModelWithMapping, float>(m => m.UserId == "user_1", m => m.Result);
            Assert.AreEqual(22.2F, max);
        }

        [TestMethod]
        public void AddBatch()
        {
            dataContext.UseBatching = true;

            dataContext.AddOrUpdate(new UserResultModelWithMapping("user_1", DateTime.Now, 55.5F));
            dataContext.AddOrUpdate(new UserResultModelWithMapping("user_1", DateTime.Now.AddMinutes(1), 88.8F));
            dataContext.AddOrUpdate(new UserResultModelWithMapping("user_1", DateTime.Now.AddMinutes(2), 22.2F));
            dataContext.AddOrUpdate(new UserResultModelWithMapping("user_1", DateTime.Now.AddMinutes(3), 22.2F));
            dataContext.AddOrUpdate(new UserResultModelWithMapping("user_1", DateTime.Now.AddMinutes(4), 22.2F));
            dataContext.AddOrUpdate(new UserResultModelWithMapping("user_1", DateTime.Now.AddMinutes(5), 22.2F));
        }
    }
}
