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

        [TestInitialize]
        public void TestInitialize()
        {
            dataContext = new CassandraDataContext(new[] { "127.0.0.1" }, "demo");
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

            var savedResult = dataContext.SingleOrDefault<UserResultModelWithIgnoreProperty>(u => u.UserId == "test_user_id" && u.Timestamp == timestamp);

            Assert.IsNotNull(savedResult);
            Assert.AreEqual("test_user_id", savedResult.UserId);
            Assert.AreEqual(15.55F, savedResult.Result);
            Assert.AreEqual(default(int), savedResult.ResultFactor);
        }
    }
}
